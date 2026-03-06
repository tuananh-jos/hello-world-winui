using App7.Domain.Entities;

namespace App7.Domain.IRepository;

public interface IDeviceRepository
{
    /// <summary>Returns a chunk of all devices (with ModelName joined) for initial in-memory load (FR35).</summary>
    Task<IReadOnlyList<Device>> GetChunkAsync(int offset, int chunkSize);

    /// <summary>Returns the IDs of the first N available devices for a model — called before BorrowAsync to know which deviceIds will be borrowed.</summary>
    Task<IReadOnlyList<Guid>> GetAvailableDeviceIdsAsync(Guid modelId, int quantity);

    /// <summary>
    /// Returns a paginated, filtered, and sorted list of borrowed devices (Status = "Borrowed").
    /// Each result has ModelName populated via join.
    /// </summary>
    Task<(IEnumerable<Device> Items, int TotalCount)> GetBorrowedPagedAsync(
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

    /// <summary>Returns all distinct HWVersion values present in borrowed devices (for filter dropdown).</summary>
    Task<IEnumerable<string>> GetBorrowedHWVersionsAsync();

    /// <summary>
    /// Borrows `quantity` available devices from the given model.
    /// Picks the first N devices with Status = "Available" and marks them "Borrowed".
    /// The operation is atomic (single transaction).
    /// Throws InvalidOperationException if available stock is insufficient.
    /// </summary>
    Task BorrowAsync(Guid modelId, int quantity);

    /// <summary>
    /// Returns a single borrowed device, resetting its Status to "Available".
    /// The operation is atomic (single transaction).
    /// </summary>
    Task ReturnAsync(Guid deviceId);

    // Legacy — kept for compatibility
    Task AddAsync(Device device);
}
