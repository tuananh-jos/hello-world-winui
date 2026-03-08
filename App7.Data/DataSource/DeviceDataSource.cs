using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Domain.Constants;
using App7.Domain.Entities;
using App7.Domain.Dtos;
using Microsoft.EntityFrameworkCore;

namespace App7.Data.DataSource;

public class DeviceDataSource : DataSourceBase<Device>, IDeviceDataSource
{
    public DeviceDataSource(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Device> Items, int TotalCount)> GetBorrowedPagedAsync(GetBorrowedDevicesRequest request)
    {
        // Join Devices with Models to get ModelName
        var query = _context.Devices
            .AsNoTracking()
            .Where(d => d.Status == "Borrowed")
            .Join(_context.Models,
                  d => d.ModelId,
                  m => m.Id,
                  // Include Device Name and joined ModelName
                  (d, m) => new { Device = d, ModelName = m.Name, DeviceName = d.Name });

        // search
        if (!string.IsNullOrWhiteSpace(request.SearchName))
            query = query.Where(x => x.DeviceName.ToLower().Contains(request.SearchName.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.SearchModelName))
            query = query.Where(x => x.ModelName.ToLower().Contains(request.SearchModelName.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.SearchIMEI))
            query = query.Where(x => x.Device.IMEI.ToLower().Contains(request.SearchIMEI.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.SearchSerialLab))
            query = query.Where(x => x.Device.SerialLab.ToLower().Contains(request.SearchSerialLab.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.SearchSerialNumber))
            query = query.Where(x => x.Device.SerialNumber.ToLower().Contains(request.SearchSerialNumber.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.SearchCircuitSerial))
            query = query.Where(x => x.Device.CircuitSerialNumber.ToLower().Contains(request.SearchCircuitSerial.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.SearchHWVersion))
            query = query.Where(x => x.Device.HWVersion.ToLower().Contains(request.SearchHWVersion.ToLower()));

        // Total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Sort
        query = request.SortColumn switch
        {
            ColumnTags.NAME                => request.Ascending ? query.OrderBy(x => x.DeviceName)              : query.OrderByDescending(x => x.DeviceName),
            ColumnTags.MODEL_NAME           => request.Ascending ? query.OrderBy(x => x.ModelName)               : query.OrderByDescending(x => x.ModelName),
            ColumnTags.IMEI                => request.Ascending ? query.OrderBy(x => x.Device.IMEI)             : query.OrderByDescending(x => x.Device.IMEI),
            ColumnTags.SERIAL_LAB           => request.Ascending ? query.OrderBy(x => x.Device.SerialLab)        : query.OrderByDescending(x => x.Device.SerialLab),
            ColumnTags.SERIAL_NUMBER        => request.Ascending ? query.OrderBy(x => x.Device.SerialNumber)     : query.OrderByDescending(x => x.Device.SerialNumber),
            ColumnTags.CIRCUIT_SERIAL_NUMBER => request.Ascending ? query.OrderBy(x => x.Device.CircuitSerialNumber) : query.OrderByDescending(x => x.Device.CircuitSerialNumber),
            ColumnTags.HW_VERSION           => request.Ascending ? query.OrderBy(x => x.Device.HWVersion)        : query.OrderByDescending(x => x.Device.HWVersion),
            _                              => request.Ascending ? query.OrderBy(x => x.ModelName)               : query.OrderByDescending(x => x.ModelName),
        };

        // Pagination + project into Device with ModelName populated
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new Device
            {
                Id                  = x.Device.Id,
                ModelId             = x.Device.ModelId,
                Name                = x.Device.Name,
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

}
