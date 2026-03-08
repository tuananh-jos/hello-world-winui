using App7.Domain.IRepository;
using App7.Domain.Services;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class ReturnDeviceUseCase : IUseCase<ReturnDeviceRequest>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInstanceSyncService _syncService;

    public ReturnDeviceUseCase(IUnitOfWork unitOfWork, IInstanceSyncService syncService)
    {
        _unitOfWork  = unitOfWork;
        _syncService = syncService;
    }

    public async Task ExecuteAsync(ReturnDeviceRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Devices.ReturnAsync(request.DeviceId);
            await _unitOfWork.Models.IncrementAvailableAsync(request.ModelId);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        _syncService.SignalChange();
    }
}
