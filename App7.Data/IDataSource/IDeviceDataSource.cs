using App7.Domain.Entities;
using App7.Domain.Dtos;

namespace App7.Data.IDataSource;

public interface IDeviceDataSource : IDataSourceBase<Device>
{
    Task<(List<Device> Items, int TotalCount)> GetBorrowedPagedAsync(GetBorrowedDevicesRequest request);

    Task BorrowAsync(Guid modelId, int quantity);

    Task ReturnAsync(Guid deviceId);

}
