using App7.Presentation.Contracts.Services;

namespace App7.Presentation.Services;

/// <summary>
/// Implements IInstanceSyncService using a shared signal file + FileSystemWatcher.
///
/// Mechanism:
///   - SignalChange() writes the current UTC timestamp to "sync_signal.txt".
///   - A FileSystemWatcher watches the same folder for changes to that file.
///   - On change, raises DataChanged after a 500ms debounce so that rapid
///     successive signals only trigger one reload.
/// </summary>
public sealed class InstanceSyncService : IInstanceSyncService, IDisposable
{
    private const string SignalFileName = "sync_signal.txt";

    private readonly string _signalFilePath;
    private FileSystemWatcher? _watcher;
    private CancellationTokenSource? _debounceCts;
    private readonly object _lock = new();

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
            File.WriteAllText(_signalFilePath, DateTimeOffset.UtcNow.ToString("o"));
        }
        catch (IOException)
        {
            // Best-effort — if write fails, the other instance's write will still trigger watchers.
        }
    }

    public void Start()
    {
        if (_watcher != null) return;

        var folder = Path.GetDirectoryName(_signalFilePath)!;

        _watcher = new FileSystemWatcher(folder, SignalFileName)
        {
            NotifyFilter        = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnSignalFileChanged;
    }

    public void Stop()
    {
        if (_watcher == null) return;
        _watcher.EnableRaisingEvents = false;
        _watcher.Changed -= OnSignalFileChanged;
        _watcher.Dispose();
        _watcher = null;

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
                await Task.Delay(500, token);
                DataChanged?.Invoke();
            }
            catch (TaskCanceledException) { /* superseded by newer signal */ }
        }, token);
    }
}
