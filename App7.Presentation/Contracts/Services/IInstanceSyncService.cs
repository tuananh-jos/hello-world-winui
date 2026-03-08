namespace App7.Presentation.Contracts.Services;

/// <summary>
/// Notifies all running instances of the app that data has changed
/// (borrow or return occurred) so they can refresh their views.
/// </summary>
public interface IInstanceSyncService
{
    /// <summary>
    /// Raised when another instance signals a data change.
    /// Subscribers must dispatch to the UI thread before touching UI-bound state.
    /// </summary>
    event Action DataChanged;

    /// <summary>Writes a signal so other instances know data has changed.</summary>
    void SignalChange();

    /// <summary>Starts watching for signals from other instances.</summary>
    void Start();

    /// <summary>Stops watching and releases resources.</summary>
    void Stop();
}
