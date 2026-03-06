using App7.Domain.Entities;
using App7.Domain.Services;

namespace App7.Data.Services;

/// <summary>
/// Singleton in-memory store.
/// Thread-safe reads via snapshot; mutations under lock.
/// StoreChanged is raised synchronously on the calling thread (caller is responsible for UI dispatch).
/// </summary>
public sealed class InMemoryStore : IInMemoryStore
{
    private readonly object _lock = new();

    private List<Model>  _models  = new();
    private List<Device> _devices = new();

    public bool IsLoaded { get; private set; }

    public event Action? StoreChanged;

    // ── Load ──────────────────────────────────────────────────────────

    public void AddModelChunk(IEnumerable<Model> models)
    {
        lock (_lock) _models.AddRange(models);
    }

    public void AddDeviceChunk(IEnumerable<Device> devices)
    {
        lock (_lock) _devices.AddRange(devices);
    }

    public void MarkLoaded()
    {
        IsLoaded = true;
        StoreChanged?.Invoke();
    }

    // ── Query ─────────────────────────────────────────────────────────

    public IReadOnlyList<Model> GetAllModels()
    {
        lock (_lock) return _models.ToList(); // snapshot
    }

    public IReadOnlyList<Device> GetAllDevices()
    {
        lock (_lock) return _devices.ToList(); // snapshot
    }

    // ── Mutation — local borrow ───────────────────────────────────────

    public void ApplyBorrow(Guid modelId, IReadOnlyList<Guid> deviceIds, int newAvailableCount)
    {
        lock (_lock)
        {
            // Update model
            var model = _models.FirstOrDefault(m => m.Id == modelId);
            if (model != null) model.Available = newAvailableCount;

            // Update devices
            var idSet = new HashSet<Guid>(deviceIds);
            foreach (var d in _devices.Where(d => idSet.Contains(d.Id)))
                d.Status = "Borrowed";
        }
        StoreChanged?.Invoke();
    }

    // ── Mutation — local return ───────────────────────────────────────

    public void ApplyReturn(Guid modelId, Guid deviceId, int newAvailableCount)
    {
        lock (_lock)
        {
            var model = _models.FirstOrDefault(m => m.Id == modelId);
            if (model != null) model.Available = newAvailableCount;

            var device = _devices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null) device.Status = "Available";
        }
        StoreChanged?.Invoke();
    }

    // ── Mutation — from signal.txt event (other instance) ────────────

    public void ApplyEvent(SyncEvent syncEvent)
    {
        lock (_lock)
        {
            var model = _models.FirstOrDefault(m => m.Id == syncEvent.ModelId);
            if (model != null) model.Available = syncEvent.NewAvailableCount;

            var idSet = new HashSet<Guid>(syncEvent.DeviceIds);
            var newStatus = syncEvent.Action == "borrow" ? "Borrowed" : "Available";

            foreach (var d in _devices.Where(d => idSet.Contains(d.Id)))
                d.Status = newStatus;
        }
        StoreChanged?.Invoke();
    }
}
