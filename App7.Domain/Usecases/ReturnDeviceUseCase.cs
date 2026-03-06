using App7.Domain.IRepository;
using App7.Domain.Services;

namespace App7.Domain.Usecases;

/// <summary>
/// UC6: Returns a borrowed device, resetting its status to "Available".
/// After DB commit:
///   1. Updates in-memory store directly (local instance — instant, no DB re-read).
///   2. Appends a SyncEvent JSON line to signal.txt (other instances apply via FileWatcher).
/// </summary>
public class ReturnDeviceUseCase
{
    private readonly IDeviceRepository   _deviceRepository;
    private readonly IModelRepository    _modelRepository;
    private readonly IInstanceSyncService _syncService;
    private readonly IInMemoryStore      _store;

    public ReturnDeviceUseCase(
        IDeviceRepository   deviceRepository,
        IModelRepository    modelRepository,
        IInstanceSyncService syncService,
        IInMemoryStore      store)
    {
        _deviceRepository = deviceRepository;
        _modelRepository  = modelRepository;
        _syncService      = syncService;
        _store            = store;
    }

    /// <summary>
    /// Executes the return workflow.
    /// Throws Microsoft.Data.Sqlite.SqliteException on SQLite lock — caller should handle gracefully.
    /// </summary>
    public async Task ExecuteAsync(Guid deviceId, Guid modelId)
    {
        // Atomic DB write
        await _deviceRepository.ReturnAsync(deviceId);
        await _modelRepository.IncrementAvailableAsync(modelId);

        // Compute new available count for store + signal
        var currentModel = _store.GetAllModels().FirstOrDefault(m => m.Id == modelId);
        var newAvailable  = (currentModel?.Available ?? 0) + 1;

        // Update local in-memory store directly (no DB re-read)
        _store.ApplyReturn(modelId, deviceId, newAvailable);

        // Append event to signal.txt for other instances (FR31)
        _syncService.SignalChange(new SyncEvent
        {
            Action            = "return",
            ModelId           = modelId,
            DeviceIds         = new List<Guid> { deviceId },
            NewAvailableCount = newAvailable,
        });
    }
}
