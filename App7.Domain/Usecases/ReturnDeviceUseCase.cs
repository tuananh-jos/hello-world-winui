using App7.Domain.IRepository;
using App7.Domain.Services;

namespace App7.Domain.Usecases;

/// <summary>
/// UC6: Returns a borrowed device, resetting its status to "Available".
/// Also increments the model's Available count.
/// </summary>
public class ReturnDeviceUseCase
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IModelRepository _modelRepository;
    private readonly IInstanceSyncService _syncService;

    public ReturnDeviceUseCase(
        IDeviceRepository deviceRepository,
        IModelRepository modelRepository,
        IInstanceSyncService syncService)
    {
        _deviceRepository = deviceRepository;
        _modelRepository  = modelRepository;
        _syncService      = syncService;
    }

    /// <summary>
    /// Executes the return workflow.
    /// Throws Microsoft.Data.Sqlite.SqliteException on SQLite lock — caller should handle gracefully.
    /// </summary>
    public async Task ExecuteAsync(Guid deviceId, Guid modelId)
    {
        await _deviceRepository.ReturnAsync(deviceId);
        await _modelRepository.IncrementAvailableAsync(modelId);
        _syncService.SignalChange(); // notify other instances
    }
}
