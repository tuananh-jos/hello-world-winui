using App7.Domain.IRepository;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class BorrowDeviceUseCase : IUseCase<BorrowDeviceRequest>
{
    private readonly IUnitOfWork _unitOfWork;

    public BorrowDeviceUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
    }
}
