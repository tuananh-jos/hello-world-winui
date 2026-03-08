using App7.Domain.Entities;
using App7.Domain.Dtos;

namespace App7.Domain.IRepository;

public interface IDeviceRepository : IRepositoryBase<Device>
{

    Task<(IEnumerable<Device> Items, int TotalCount)> GetBorrowedPagedAsync(GetBorrowedDevicesRequest request);

    Task BorrowAsync(Guid modelId, int quantity);

    Task ReturnAsync(Guid deviceId);

}
