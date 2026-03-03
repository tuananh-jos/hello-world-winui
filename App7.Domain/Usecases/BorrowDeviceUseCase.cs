using App7.Domain.IRepository;

namespace App7.Domain.Usecases;

/// <summary>
/// UC3: Borrows a given quantity of devices from a model.
/// Atomically marks devices as "Borrowed" and decrements model Available count.
/// </summary>
public class BorrowDeviceUseCase
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IModelRepository _modelRepository;

    public BorrowDeviceUseCase(
        IDeviceRepository deviceRepository,
        IModelRepository modelRepository)
    {
        _deviceRepository = deviceRepository;
        _modelRepository = modelRepository;
    }

    /// <summary>
    /// Executes the borrow workflow.
    /// Throws InvalidOperationException if stock is insufficient.
    /// Throws Microsoft.Data.Sqlite.SqliteException on SQLite lock — caller should handle gracefully.
    /// </summary>
    public async Task ExecuteAsync(Guid modelId, int quantity)
    {
        await _deviceRepository.BorrowAsync(modelId, quantity);
        await _modelRepository.DecrementAvailableAsync(modelId, quantity);
    }
}
