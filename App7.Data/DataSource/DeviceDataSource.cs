using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App7.Data.DataSource;

public class DeviceDataSource : IDeviceDataSource
{
    private readonly AppDbContext _context;

    public DeviceDataSource(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Device> Items, int TotalCount)> GetBorrowedPagedAsync(
        int page,
        int pageSize,
        string? searchText,
        string? filterHWVersion,
        string? sortColumn,
        bool ascending)
    {
        var query = _context.Devices
            .AsNoTracking()
            .Where(d => d.Status == "Borrowed");

        // Multi-field search
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var t = searchText;
            query = query.Where(d =>
                d.Name.Contains(t) ||
                d.IMEI.Contains(t) ||
                d.SerialLab.Contains(t) ||
                d.SerialNumber.Contains(t) ||
                d.CircuitSerialNumber.Contains(t) ||
                d.HWVersion.Contains(t) ||
                d.ModelId.ToString().Contains(t));
        }

        // Filter by HWVersion
        if (!string.IsNullOrWhiteSpace(filterHWVersion))
            query = query.Where(d => d.HWVersion == filterHWVersion);

        // Total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Sort
        query = (sortColumn?.ToLowerInvariant()) switch
        {
            "imei"               => ascending ? query.OrderBy(d => d.IMEI)               : query.OrderByDescending(d => d.IMEI),
            "seriallab"          => ascending ? query.OrderBy(d => d.SerialLab)          : query.OrderByDescending(d => d.SerialLab),
            "serialnumber"       => ascending ? query.OrderBy(d => d.SerialNumber)       : query.OrderByDescending(d => d.SerialNumber),
            "circuitserialnumber"=> ascending ? query.OrderBy(d => d.CircuitSerialNumber): query.OrderByDescending(d => d.CircuitSerialNumber),
            "hwversion"          => ascending ? query.OrderBy(d => d.HWVersion)          : query.OrderByDescending(d => d.HWVersion),
            _                    => ascending ? query.OrderBy(d => d.Name)               : query.OrderByDescending(d => d.Name),
        };

        // Pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
        // Get the IDs of available devices first (read phase — no lock yet)
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
            // Bulk update: mark selected devices as Borrowed (NFR6 — atomic)
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
