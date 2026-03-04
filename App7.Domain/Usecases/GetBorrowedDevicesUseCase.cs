using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Domain.Usecases;

/// <summary>
/// UC4 + UC5: Returns a paginated, filtered, and sorted list of currently borrowed devices.
/// </summary>
public class GetBorrowedDevicesUseCase
{
    private readonly IDeviceRepository _deviceRepository;

    public GetBorrowedDevicesUseCase(IDeviceRepository deviceRepository)
        => _deviceRepository = deviceRepository;

    public async Task<(IEnumerable<Device> Items, int TotalCount)> ExecuteAsync(
        int page,
        int pageSize,
        string? searchModelName  = null,
        string? searchIMEI       = null,
        string? searchSerialLab  = null,
        string? searchSerialNumber  = null,
        string? searchCircuitSerial = null,
        string? searchHWVersion  = null,
        string? sortColumn       = null,
        bool    ascending        = true)
    {
        return await _deviceRepository.GetBorrowedPagedAsync(
            page, pageSize,
            searchModelName, searchIMEI,
            searchSerialLab, searchSerialNumber,
            searchCircuitSerial, searchHWVersion,
            sortColumn, ascending);
    }

    public async Task<IEnumerable<string>> GetHWVersionsAsync()
        => await _deviceRepository.GetBorrowedHWVersionsAsync();
}
