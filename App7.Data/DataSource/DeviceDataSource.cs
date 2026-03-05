using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App7.Data.DataSource;

public class DeviceDataSource : IDeviceDataSource
{
    private readonly AppDbContext _context;

    public DeviceDataSource(AppDbContext context)
        => _context = context;

    public async Task<(List<Device> Items, int TotalCount)> GetBorrowedPagedAsync(
        int page,
        int pageSize,
        string? searchModelName,
        string? searchIMEI,
        string? searchSerialLab,
        string? searchSerialNumber,
        string? searchCircuitSerial,
        string? searchHWVersion,
        string? sortColumn,
        bool ascending)
    {
        // Join Devices with Models to get ModelName
        var query = _context.Devices
            .AsNoTracking()
            .Where(d => d.Status == "Borrowed")
            .Join(_context.Models,
                  d => d.ModelId,
                  m => m.Id,
                  (d, m) => new { Device = d, ModelName = m.Name });

        // Per-column search — each filter is independent
        if (!string.IsNullOrWhiteSpace(searchModelName))
            query = query.Where(x => x.ModelName.ToLower().Contains(searchModelName.ToLower()));

        if (!string.IsNullOrWhiteSpace(searchIMEI))
            query = query.Where(x => x.Device.IMEI.ToLower().Contains(searchIMEI.ToLower()));

        if (!string.IsNullOrWhiteSpace(searchSerialLab))
            query = query.Where(x => x.Device.SerialLab.ToLower().Contains(searchSerialLab.ToLower()));

        if (!string.IsNullOrWhiteSpace(searchSerialNumber))
            query = query.Where(x => x.Device.SerialNumber.ToLower().Contains(searchSerialNumber.ToLower()));

        if (!string.IsNullOrWhiteSpace(searchCircuitSerial))
            query = query.Where(x => x.Device.CircuitSerialNumber.ToLower().Contains(searchCircuitSerial.ToLower()));

        if (!string.IsNullOrWhiteSpace(searchHWVersion))
            query = query.Where(x => x.Device.HWVersion.ToLower().Contains(searchHWVersion.ToLower()));

        // Total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Sort
        query = (sortColumn?.ToLowerInvariant()) switch
        {
            "modelname"          => ascending ? query.OrderBy(x => x.ModelName)               : query.OrderByDescending(x => x.ModelName),
            "imei"               => ascending ? query.OrderBy(x => x.Device.IMEI)             : query.OrderByDescending(x => x.Device.IMEI),
            "seriallab"          => ascending ? query.OrderBy(x => x.Device.SerialLab)        : query.OrderByDescending(x => x.Device.SerialLab),
            "serialnumber"       => ascending ? query.OrderBy(x => x.Device.SerialNumber)     : query.OrderByDescending(x => x.Device.SerialNumber),
            "circuitserialnumber"=> ascending ? query.OrderBy(x => x.Device.CircuitSerialNumber) : query.OrderByDescending(x => x.Device.CircuitSerialNumber),
            "hwversion"          => ascending ? query.OrderBy(x => x.Device.HWVersion)        : query.OrderByDescending(x => x.Device.HWVersion),
            _                    => ascending ? query.OrderBy(x => x.ModelName)               : query.OrderByDescending(x => x.ModelName),
        };

        // Pagination + project into Device with ModelName populated
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Device
            {
                Id                  = x.Device.Id,
                ModelId             = x.Device.ModelId,
                ModelName           = x.ModelName,
                IMEI                = x.Device.IMEI,
                SerialLab           = x.Device.SerialLab,
                SerialNumber        = x.Device.SerialNumber,
                CircuitSerialNumber = x.Device.CircuitSerialNumber,
                HWVersion           = x.Device.HWVersion,
                Status              = x.Device.Status,
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<string>> GetBorrowedHWVersionsAsync()
    {
        return await _context.Devices
            .AsNoTracking()
            .Where(d => d.Status == "Borrowed")
            .Select(d => d.HWVersion)
            .Distinct()
            .OrderBy(v => v)
            .ToListAsync();
    }

    /// <summary>
    /// Atomically borrows `quantity` available devices from the given model.
    /// Uses a transaction to ensure all-or-nothing update (NFR6).
    /// Throws InvalidOperationException if stock is insufficient.
    /// </summary>
    public async Task BorrowAsync(Guid modelId, int quantity)
    {
        var candidateIds = await _context.Devices
            .AsNoTracking()
            .Where(d => d.ModelId == modelId && d.Status == "Available")
            .Select(d => d.Id)
            .Take(quantity)
            .ToListAsync();

        if (candidateIds.Count < quantity)
            throw new InvalidOperationException(
                $"Not enough available devices. Requested: {quantity}, available: {candidateIds.Count}.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var updatedCount = await _context.Devices
                .Where(d => candidateIds.Contains(d.Id) && d.Status == "Available")
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, "Borrowed"));

            if (updatedCount < quantity)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    "Concurrent borrow detected: some devices were borrowed by another operation. Please try again.");
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Atomically returns a borrowed device to "Available" status.
    /// Uses a transaction (NFR6).
    /// </summary>
    public async Task ReturnAsync(Guid deviceId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var updatedCount = await _context.Devices
                .Where(d => d.Id == deviceId && d.Status == "Borrowed")
                .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, "Available"));

            if (updatedCount == 0)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    $"Device {deviceId} is not currently borrowed or does not exist.");
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task InsertAsync(Device device)
    {
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
    }
}
