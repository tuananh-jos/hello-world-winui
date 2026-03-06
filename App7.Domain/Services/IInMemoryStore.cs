using App7.Domain.Entities;

namespace App7.Domain.Services;

/// <summary>
/// Singleton in-memory store for all Models and Devices.
/// Loaded once at startup (background chunk loading).
/// All search/filter/sort/pagination operates on this store — no DB queries after initial load.
/// Updated directly after borrow/return on the local instance.
/// Other instances update via signal.txt event log → ApplyEvent().
/// </summary>
public interface IInMemoryStore
{
    // ── State ─────────────────────────────────────────────────────────

    /// <summary>True once the initial chunk loading is fully complete.</summary>
    bool IsLoaded { get; }

    /// <summary>Raised (on UI thread via dispatcher) when any data in the store changes.</summary>
    event Action StoreChanged;

    // ── Load ──────────────────────────────────────────────────────────

    /// <summary>Replaces the Models collection (called per-chunk during initial load).</summary>
    void AddModelChunk(IEnumerable<Model> models);

    /// <summary>Replaces the Devices collection (called per-chunk during initial load).</summary>
    void AddDeviceChunk(IEnumerable<Device> devices);

    /// <summary>Called when all chunks have been loaded.</summary>
    void MarkLoaded();

    // ── Query ─────────────────────────────────────────────────────────

    /// <summary>Returns a snapshot of all Models for in-memory filtering.</summary>
    IReadOnlyList<Model> GetAllModels();

    /// <summary>Returns a snapshot of all Devices for in-memory filtering.</summary>
    IReadOnlyList<Device> GetAllDevices();

    // ── Mutation (local instance — borrow/return) ─────────────────────

    /// <summary>
    /// Updates in-memory state after a borrow:
    /// marks deviceIds as Borrowed, decrements model availableCount.
    /// </summary>
    void ApplyBorrow(Guid modelId, IReadOnlyList<Guid> deviceIds, int newAvailableCount);

    /// <summary>
    /// Updates in-memory state after a return:
    /// marks deviceId as Available, sets model availableCount.
    /// </summary>
    void ApplyReturn(Guid modelId, Guid deviceId, int newAvailableCount);

    // ── Mutation (from signal.txt event — other instances) ────────────

    /// <summary>
    /// Applies a sync event received from signal.txt.
    /// action = "borrow" | "return"
    /// </summary>
    void ApplyEvent(SyncEvent syncEvent);
}
