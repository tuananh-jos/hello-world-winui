using App7.Domain.IRepository;
using App7.Domain.Services;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class BorrowDeviceUseCase : IUseCase<BorrowDeviceRequest>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IModelRepository _modelRepository;
    private readonly IInstanceSyncService _syncService;

    public BorrowDeviceUseCase(IDeviceRepository deviceRepository, IModelRepository modelRepository, IInstanceSyncService syncService)
    {
        _deviceRepository = deviceRepository;
        _modelRepository  = modelRepository;
        _syncService      = syncService;
    }

    public async Task ExecuteAsync(BorrowDeviceRequest request)
    {
        await _deviceRepository.BorrowAsync(request.ModelId, request.Quantity);
        await _modelRepository.DecrementAvailableAsync(request.ModelId, request.Quantity);

        _syncService.SignalChange();
    }
}
