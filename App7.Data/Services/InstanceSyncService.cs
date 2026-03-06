using App7.Domain.Services;
using System.IO;
using System.Text.Json;

namespace App7.Data.Services;

/// <summary>
/// Implements IInstanceSyncService using an append-only JSON-Lines event log (signal.txt).
///
/// Protocol:
///   - SignalChange(event) appends a single JSON line to signal.txt:
///     {"Seq":1,"Action":"borrow","ModelId":"...","DeviceIds":["..."],"NewAvailableCount":8}
///   - A FileSystemWatcher watches signal.txt for changes.
///   - On change, reads only new lines since lastReadPosition.
///   - Each new line is parsed into a SyncEvent and raised via EventReceived.
///   - If parsing fails, the line is skipped (caller falls back to targeted DB query).
///   - 200ms debounce on the watcher to batch rapid successive writes.
///
/// Thread safety:
///   - EventReceived is raised on a ThreadPool thread.
///   - Subscribers must dispatch to the UI thread before touching UI-bound state.
/// </summary>
public sealed class InstanceSyncService : IInstanceSyncService, IDisposable
{
    private const string SignalFileName = "signal.txt";

    private readonly string      _signalFilePath;
    private FileSystemWatcher?   _watcher;
    private CancellationTokenSource? _debounceCts;
    private readonly object      _lock            = new();
    private long                 _lastReadPosition = 0;
    private static long          _seqCounter;

    public event Action<SyncEvent>? EventReceived;

    public InstanceSyncService(string folderPath)
    {
        _signalFilePath = Path.Combine(folderPath, SignalFileName);

        // If file already exists, seek to end so we don't re-process old events on startup
        if (File.Exists(_signalFilePath))
        {
            _lastReadPosition = new FileInfo(_signalFilePath).Length;
            _seqCounter       = CountLines(_signalFilePath);
        }
    }

    // ── Public API ────────────────────────────────────────────────────

    public void SignalChange(SyncEvent syncEvent)
    {
        try
        {
            syncEvent.Seq = Interlocked.Increment(ref _seqCounter);
            var line = JsonSerializer.Serialize(syncEvent) + Environment.NewLine;
            // Append-only: use FileShare.ReadWrite so FileWatcher can read simultaneously
            using var fs = new FileStream(
                _signalFilePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite);
            using var sw = new StreamWriter(fs);
            sw.Write(line);
        }
        catch (IOException)
        {
            // Best-effort: if write fails (e.g., concurrent append),
            // the other instance's write still notifies watchers.
        }
    }

    public void Start()
    {
        if (_watcher != null) return;

        var folder = Path.GetDirectoryName(_signalFilePath)!;
        Directory.CreateDirectory(folder);

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
        // Debounce: cancel pending and schedule new fire 200ms out
        CancellationTokenSource newCts;
        CancellationTokenSource? oldCts;

        lock (_lock)
        {
            oldCts       = _debounceCts;
            newCts       = new CancellationTokenSource();
            _debounceCts = newCts;
        }

        oldCts?.Cancel();
        oldCts?.Dispose();

        var token = newCts.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(200, token);
                ReadAndRaiseNewEvents();
            }
            catch (TaskCanceledException) { /* superseded by newer signal */ }
        }, token);
    }

    private void ReadAndRaiseNewEvents()
    {
        if (!File.Exists(_signalFilePath)) return;

        try
        {
            using var fs = new FileStream(
                _signalFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            fs.Seek(_lastReadPosition, SeekOrigin.Begin);

            using var sr = new StreamReader(fs);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                _lastReadPosition = fs.Position;

                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var evt = JsonSerializer.Deserialize<SyncEvent>(line);
                    if (evt != null)
                        EventReceived?.Invoke(evt);
                }
                catch (JsonException)
                {
                    // Malformed line — skip; caller fallback handles it
                }
            }
        }
        catch (IOException)
        {
            // File locked briefly by writer — will retry on next watcher event
        }
    }

    private static long CountLines(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            long count = 0;
            while (sr.ReadLine() != null) count++;
            return count;
        }
        catch { return 0; }
    }
}
