using App7.Data.IDataSource;
using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Data.Repository;

public class DeviceRepository : IDeviceRepository
{
    private readonly IDeviceDataSource _dataSource;

    public DeviceRepository(IDeviceDataSource dataSource)
        => _dataSource = dataSource;

    public async Task<(IEnumerable<Device> Items, int TotalCount)> GetBorrowedPagedAsync(
        int page, int pageSize,
        string? searchModelName, string? searchIMEI,
        string? searchSerialLab, string? searchSerialNumber,
        string? searchCircuitSerial, string? searchHWVersion,
        string? sortColumn, bool ascending)
    {
        var result = await _dataSource.GetBorrowedPagedAsync(
            page, pageSize,
            searchModelName, searchIMEI,
            searchSerialLab, searchSerialNumber,
            searchCircuitSerial, searchHWVersion,
            sortColumn, ascending);
        return (result.Items, result.TotalCount);
    }

    public async Task<IEnumerable<string>> GetBorrowedHWVersionsAsync()
        => await _dataSource.GetBorrowedHWVersionsAsync();

    public async Task BorrowAsync(Guid modelId, int quantity)
        => await _dataSource.BorrowAsync(modelId, quantity);

    public async Task ReturnAsync(Guid deviceId)
        => await _dataSource.ReturnAsync(deviceId);

    public async Task AddAsync(Device device)
        => await _dataSource.InsertAsync(device);
}
