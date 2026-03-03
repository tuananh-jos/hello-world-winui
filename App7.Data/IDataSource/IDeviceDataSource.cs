using App7.Domain.Entities;

namespace App7.Data.IDataSource;

public interface IDeviceDataSource
{
    Task<(List<Device> Items, int TotalCount)> GetBorrowedPagedAsync(
        int page,
        int pageSize,
        string? searchText,
        string? filterHWVersion,
        string? sortColumn,
        bool ascending);

    Task<List<string>> GetBorrowedHWVersionsAsync();

    Task BorrowAsync(Guid modelId, int quantity);

    Task ReturnAsync(Guid deviceId);

    Task InsertAsync(Device device);
}
