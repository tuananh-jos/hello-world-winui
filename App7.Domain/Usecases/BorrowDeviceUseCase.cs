using App7.Domain.IRepository;
using App7.Domain.Services;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class BorrowDeviceUseCase : IUseCase<BorrowDeviceRequest>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInstanceSyncService _syncService;

    public BorrowDeviceUseCase(IUnitOfWork unitOfWork, IInstanceSyncService syncService)
    {
        _unitOfWork  = unitOfWork;
        _syncService = syncService;
    }

    public async Task ExecuteAsync(BorrowDeviceRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Devices.BorrowAsync(request.ModelId, request.Quantity);
            await _unitOfWork.Models.DecrementAvailableAsync(request.ModelId, request.Quantity);
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
