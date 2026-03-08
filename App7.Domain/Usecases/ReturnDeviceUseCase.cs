using App7.Domain.IRepository;
using App7.Domain.Services;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class ReturnDeviceUseCase : IUseCase<ReturnDeviceRequest>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IModelRepository _modelRepository;
    private readonly IInstanceSyncService _syncService;

    public ReturnDeviceUseCase(IDeviceRepository deviceRepository, IModelRepository modelRepository, IInstanceSyncService syncService)
    {
        _deviceRepository = deviceRepository;
        _modelRepository  = modelRepository;
        _syncService      = syncService;
    }

    public async Task ExecuteAsync(ReturnDeviceRequest request)
    {
        await _deviceRepository.ReturnAsync(request.DeviceId);
        await _modelRepository.IncrementAvailableAsync(request.ModelId);
        
        _syncService.SignalChange();
    }
}
