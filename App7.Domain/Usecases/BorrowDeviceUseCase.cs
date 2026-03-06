using App7.Domain.IRepository;
using App7.Domain.Services;

namespace App7.Domain.Usecases;

/// <summary>
/// UC3: Borrows a given quantity of devices from a model.
/// Atomically marks devices as "Borrowed" and decrements model Available count.
/// After DB commit:
///   1. Updates in-memory store directly (local instance — instant, no DB re-read).
///   2. Appends a SyncEvent JSON line to signal.txt (other instances will apply via FileWatcher).
/// </summary>
public class BorrowDeviceUseCase
{
    private readonly IDeviceRepository  _deviceRepository;
    private readonly IModelRepository   _modelRepository;
    private readonly IInstanceSyncService _syncService;
    private readonly IInMemoryStore     _store;

    public BorrowDeviceUseCase(
        IDeviceRepository  deviceRepository,
        IModelRepository   modelRepository,
        IInstanceSyncService syncService,
        IInMemoryStore     store)
    {
        _deviceRepository = deviceRepository;
        _modelRepository  = modelRepository;
        _syncService      = syncService;
        _store            = store;
    }

    /// <summary>
    /// Executes the borrow workflow.
    /// Throws InvalidOperationException if stock is insufficient.
    /// Throws Microsoft.Data.Sqlite.SqliteException on SQLite lock — caller should handle gracefully.
    /// </summary>
    public async Task ExecuteAsync(Guid modelId, int quantity)
    {
        // Snapshot candidate device IDs BEFORE borrow (needed for store + signal)
        var deviceIds = await _deviceRepository.GetAvailableDeviceIdsAsync(modelId, quantity);
        if (deviceIds.Count < quantity)
            throw new InvalidOperationException(
                $"Not enough available devices. Requested: {quantity}, available: {deviceIds.Count}.");

        // Atomic DB write
        await _deviceRepository.BorrowAsync(modelId, quantity);
        await _modelRepository.DecrementAvailableAsync(modelId, quantity);

        // Compute new available count for store + signal
        var currentModel = _store.GetAllModels().FirstOrDefault(m => m.Id == modelId);
        var newAvailable  = (currentModel?.Available ?? quantity) - quantity;

        // Update local in-memory store directly (no DB re-read)
        _store.ApplyBorrow(modelId, deviceIds, newAvailable);

        // Append event to signal.txt for other instances (FR30)
        _syncService.SignalChange(new SyncEvent
        {
            Action            = "borrow",
            ModelId           = modelId,
            DeviceIds         = deviceIds.ToList(),
            NewAvailableCount = newAvailable,
        });
    }
}
