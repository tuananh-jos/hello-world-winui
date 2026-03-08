using App7.Domain.Entities;
using App7.Domain.IRepository;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class GetBorrowedDevicesUseCase : IUseCase<GetBorrowedDevicesRequest, (IEnumerable<Device> Items, int TotalCount)>
{
    private readonly IDeviceRepository _deviceRepository;

    public GetBorrowedDevicesUseCase(IDeviceRepository deviceRepository) => _deviceRepository = deviceRepository;

    public async Task<(IEnumerable<Device> Items, int TotalCount)> ExecuteAsync(GetBorrowedDevicesRequest request)
    {
        await Task.Delay(100);
        return await _deviceRepository.GetBorrowedPagedAsync(request);
    }
}
