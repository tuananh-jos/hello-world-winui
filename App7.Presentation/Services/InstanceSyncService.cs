using App7.Presentation.Contracts.Services;

namespace App7.Presentation.Services;

/// <summary>
/// Implements IInstanceSyncService using a shared signal file + FileSystemWatcher + polling fallback.
///
/// Mechanism:
///   - SignalChange() writes the current UTC timestamp to "sync_signal.txt".
///   - A FileSystemWatcher watches the same folder for changes to that file.
///   - A polling timer checks the file every 5 seconds as a fallback.
///   - On change, raises DataChanged after a 500ms debounce.
///   - Self-triggered changes (from our own SignalChange) are ignored.
/// </summary>
public sealed class InstanceSyncService : IInstanceSyncService, IDisposable
{
    private const string SignalFileName = "sync_signal.txt";
    private const int DebounceMs = 500;
    private const int PollingIntervalMs = 5000;

    private readonly string _signalFilePath;
    private FileSystemWatcher? _watcher;
    private Timer? _pollingTimer;
    private CancellationTokenSource? _debounceCts;
    private readonly object _lock = new();

    private DateTime _lastKnownWriteTime = DateTime.MinValue;
    private DateTime _selfSignalTime = DateTime.MinValue;

    public event Action? DataChanged;

    public InstanceSyncService(string folderPath)
    {
        _signalFilePath = Path.Combine(folderPath, SignalFileName);
    }

    // ── Public API ────────────────────────────────────────────────────

    public void SignalChange()
    {
        try
        {
            // Mark as self-signal so we skip our own FileSystemWatcher event
            lock (_lock)
            {
                _selfSignalTime = DateTime.UtcNow;
            }

            File.WriteAllText(_signalFilePath, DateTimeOffset.UtcNow.ToString("o"));

            // Update last known write time so polling won't trigger either
            lock (_lock)
            {
                _lastKnownWriteTime = File.GetLastWriteTimeUtc(_signalFilePath);
            }
        }
        catch (IOException)
        {
            // Best-effort — if write fails, the other instance's write will still trigger watchers.
        }
    }

    public void Start()
    {
        if (_watcher != null) return;

        // Initialize last known write time
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

    private void OnSignalFileChanged(object sender, FileSystemEventArgs e)
    {
        // Skip if this was our own write (within 2 seconds)
        lock (_lock)
        {
            if ((DateTime.UtcNow - _selfSignalTime).TotalSeconds < 2)
            {
                _lastKnownWriteTime = File.Exists(_signalFilePath)
                    ? File.GetLastWriteTimeUtc(_signalFilePath)
                    : DateTime.MinValue;
                return;
            }
        }

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

                // Skip if this was our own write
                if ((DateTime.UtcNow - _selfSignalTime).TotalSeconds < 2) return;
            }

            ScheduleDataChanged();
        }
        catch (IOException)
        {
            // File might be locked by another instance, try next polling cycle
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
