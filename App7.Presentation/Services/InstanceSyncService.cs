using App7.Presentation.Contracts.Services;

namespace App7.Presentation.Services;

/// <summary>
/// Implements IInstanceSyncService using a shared signal file + FileSystemWatcher + polling fallback.
///
/// Mechanism:
///   - SignalChange() writes "[instanceId]|[timestamp]" to "sync_signal.txt".
///   - FileSystemWatcher + polling timer detect changes.
///   - On change, reads the file — if instanceId differs from ours, raises DataChanged.
///   - Self-triggered changes are ignored by comparing instance IDs.
/// </summary>
public sealed class InstanceSyncService : IInstanceSyncService, IDisposable
{
    private const string SignalFileName = "sync_signal.txt";
    private const int DebounceMs = 500;
    private const int PollingIntervalMs = 5000;

    private readonly string _signalFilePath;
    private readonly string _instanceId = Guid.NewGuid().ToString("N")[..8]; // Unique per app instance

    private FileSystemWatcher? _watcher;
    private Timer? _pollingTimer;
    private CancellationTokenSource? _debounceCts;
    private readonly object _lock = new();

    private DateTime _lastKnownWriteTime = DateTime.MinValue;

    public event Action? DataChanged;
    public event Action<object>? LocalDataChanged;

    public void NotifyLocalChange(object senderViewModel)
    {
        LocalDataChanged?.Invoke(senderViewModel);
    }

    public InstanceSyncService(string folderPath)
    {
        _signalFilePath = Path.Combine(folderPath, SignalFileName);
    }

    // ── Public API ────────────────────────────────────────────────────

    public void SignalChange()
    {
        try
        {
            // Write our instance ID + timestamp so other instances can detect it's not from them
            File.WriteAllText(_signalFilePath, $"{_instanceId}|{DateTimeOffset.UtcNow:o}");

            // Update last known write time so polling won't re-trigger
            lock (_lock)
            {
                _lastKnownWriteTime = File.GetLastWriteTimeUtc(_signalFilePath);
            }
        }
        catch (IOException)
        {
            // Best-effort
        }
    }

    public void Start()
    {
        if (_watcher != null) return;

        if (File.Exists(_signalFilePath))
        {
            _lastKnownWriteTime = File.GetLastWriteTimeUtc(_signalFilePath);
        }

        var folder = Path.GetDirectoryName(_signalFilePath)!;

        // FileSystemWatcher — primary mechanism
        _watcher = new FileSystemWatcher(folder, SignalFileName)
        {
            NotifyFilter        = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };
        _watcher.Changed += OnSignalFileChanged;

        // Polling timer — fallback every 5 seconds
        _pollingTimer = new Timer(OnPollingTick, null, PollingIntervalMs, PollingIntervalMs);
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnSignalFileChanged;
            _watcher.Dispose();
            _watcher = null;
        }

        _pollingTimer?.Dispose();
        _pollingTimer = null;

        lock (_lock)
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = null;
        }
    }

    public void Dispose() => Stop();

    // ── Private ───────────────────────────────────────────────────────

    /// <summary>Returns true if the signal file was written by a DIFFERENT instance.</summary>
    private bool IsExternalSignal()
    {
        try
        {
            if (!File.Exists(_signalFilePath)) return false;
            var content = File.ReadAllText(_signalFilePath).Trim();
            // Format: "instanceId|timestamp"
            var parts = content.Split('|');
            if (parts.Length < 2) return true; // Unknown format → treat as external
            return parts[0] != _instanceId;
        }
        catch (IOException)
        {
            return false; // Can't read → skip this cycle
        }
    }

    private void OnSignalFileChanged(object sender, FileSystemEventArgs e)
    {
        // Update last known write time so polling doesn't re-trigger for the same change
        lock (_lock)
        {
            try { _lastKnownWriteTime = File.GetLastWriteTimeUtc(_signalFilePath); }
            catch (IOException) { }
        }

        if (!IsExternalSignal()) return; // Skip our own writes

        ScheduleDataChanged();
    }

    private void OnPollingTick(object? state)
    {
        try
        {
            if (!File.Exists(_signalFilePath)) return;

            var currentWriteTime = File.GetLastWriteTimeUtc(_signalFilePath);

            lock (_lock)
            {
                if (currentWriteTime <= _lastKnownWriteTime) return;
                _lastKnownWriteTime = currentWriteTime;
            }

            if (!IsExternalSignal()) return; // Skip our own writes

            ScheduleDataChanged();
        }
        catch (IOException)
        {
            // File might be locked, try next cycle
        }
    }

    private void ScheduleDataChanged()
    {
        CancellationTokenSource newCts;
        CancellationTokenSource? oldCts;

        lock (_lock)
        {
            oldCts  = _debounceCts;
            newCts  = new CancellationTokenSource();
            _debounceCts = newCts;
        }

        oldCts?.Cancel();
        oldCts?.Dispose();

        var token = newCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceMs, token);
                DataChanged?.Invoke();
            }
            catch (TaskCanceledException) { /* superseded by newer signal */ }
        }, token);
    }
}
