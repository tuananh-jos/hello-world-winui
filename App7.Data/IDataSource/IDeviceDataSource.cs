using App7.Domain.Entities;

namespace App7.Data.IDataSource;

public interface IDeviceDataSource
{
    /// <summary>Returns a chunk of all devices (with ModelName joined) for initial in-memory load.</summary>
    Task<List<Device>> GetChunkAsync(int offset, int chunkSize);

    /// <summary>Returns the IDs of the first N available devices for a model — used after BorrowAsync to know which deviceIds were borrowed.</summary>
    Task<List<Guid>> GetAvailableDeviceIdsAsync(Guid modelId, int quantity);

    Task<(List<Device> Items, int TotalCount)> GetBorrowedPagedAsync(
        int page,
        int pageSize,
        string? searchModelName,
        string? searchIMEI,
        string? searchSerialLab,
        string? searchSerialNumber,
        string? searchCircuitSerial,
        string? searchHWVersion,
        string? sortColumn,
        bool ascending);

    Task<List<string>> GetBorrowedHWVersionsAsync();

    Task BorrowAsync(Guid modelId, int quantity);

    Task ReturnAsync(Guid deviceId);

    Task InsertAsync(Device device);
}
