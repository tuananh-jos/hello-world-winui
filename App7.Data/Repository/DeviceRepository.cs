using App7.Data.IDataSource;
using App7.Domain.Entities;
using App7.Domain.IRepository;
using App7.Domain.Dtos;

namespace App7.Data.Repository;

public class DeviceRepository : RepositoryBase<Device>, IDeviceRepository
{
    private IDeviceDataSource DeviceDataSource => (IDeviceDataSource)_dataSource;

    public DeviceRepository(IDeviceDataSource dataSource) : base(dataSource)
    {
    }

    public async Task<(IEnumerable<Device> Items, int TotalCount)> GetBorrowedPagedAsync(GetBorrowedDevicesRequest request)
    {
        var result = await DeviceDataSource.GetBorrowedPagedAsync(request);
        return (result.Items, result.TotalCount);
    }

    public async Task BorrowAsync(Guid modelId, int quantity)
        => await DeviceDataSource.BorrowAsync(modelId, quantity);

    public async Task ReturnAsync(Guid deviceId)
        => await DeviceDataSource.ReturnAsync(deviceId);
}
