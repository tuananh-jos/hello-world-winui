namespace App7.Domain.Services;

/// <summary>
/// Notifies all running instances of the app that device data has changed
/// by appending a structured JSON event to signal.txt (append-only event log).
/// Other instances read only new lines since their lastReadPosition — no DB re-read.
/// </summary>
public interface IInstanceSyncService
{
    /// <summary>
    /// Raised on a background thread when another instance signals a data change.
    /// Carries the parsed SyncEvent for direct in-memory application.
    /// Subscribers must dispatch to the UI thread before touching UI-bound state.
    /// </summary>
    event Action<SyncEvent> EventReceived;

    /// <summary>
    /// Appends a JSON SyncEvent line to signal.txt after a borrow or return.
    /// Called by BorrowDeviceUseCase / ReturnDeviceUseCase AFTER DB commit.
    /// </summary>
    void SignalChange(SyncEvent syncEvent);

    /// <summary>Starts watching signal.txt for new events from other instances.</summary>
    void Start();

    /// <summary>Stops watching and releases resources.</summary>
    void Stop();
}
