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
        // 1. Base query — single table, no JOIN yet
        var deviceQuery = _context.Devices.AsNoTracking().Where(d => d.Status == "Borrowed");

        // 2. Search filters — EF.Functions.Like is case-insensitive on SQLite by default
        //    Avoids LOWER() function call that prevents index usage
        if (!string.IsNullOrWhiteSpace(request.SearchName))
            deviceQuery = deviceQuery.Where(d => EF.Functions.Like(d.Name, $"%{request.SearchName}%"));

        if (!string.IsNullOrWhiteSpace(request.SearchIMEI))
            deviceQuery = deviceQuery.Where(d => EF.Functions.Like(d.IMEI, $"%{request.SearchIMEI}%"));

        if (!string.IsNullOrWhiteSpace(request.SearchSerialLab))
            deviceQuery = deviceQuery.Where(d => EF.Functions.Like(d.SerialLab, $"%{request.SearchSerialLab}%"));

        if (!string.IsNullOrWhiteSpace(request.SearchSerialNumber))
            deviceQuery = deviceQuery.Where(d => EF.Functions.Like(d.SerialNumber, $"%{request.SearchSerialNumber}%"));

        if (!string.IsNullOrWhiteSpace(request.SearchCircuitSerial))
            deviceQuery = deviceQuery.Where(d => EF.Functions.Like(d.CircuitSerialNumber, $"%{request.SearchCircuitSerial}%"));

        if (!string.IsNullOrWhiteSpace(request.SearchHWVersion))
            deviceQuery = deviceQuery.Where(d => EF.Functions.Like(d.HWVersion, $"%{request.SearchHWVersion}%"));

        // SearchModelName — 2-phase: find matching ModelIds first, then filter Devices
        if (!string.IsNullOrWhiteSpace(request.SearchModelName))
        {
            var matchingModelIds = await _context.Models
                .AsNoTracking()
                .Where(m => EF.Functions.Like(m.Name, $"%{request.SearchModelName}%"))
                .Select(m => m.Id)
                .ToListAsync();
            deviceQuery = deviceQuery.Where(d => matchingModelIds.Contains(d.ModelId));
        }

        // 3. Count — on single table (fast with Status index)
        var totalCount = await deviceQuery.CountAsync();

        // 4. Sort + paginate — get IDs only
        var sortedQuery = request.SortColumn switch
        {
            ColumnTags.NAME                  => request.Ascending ? deviceQuery.OrderBy(d => d.Name)                : deviceQuery.OrderByDescending(d => d.Name),
            ColumnTags.IMEI                  => request.Ascending ? deviceQuery.OrderBy(d => d.IMEI)                : deviceQuery.OrderByDescending(d => d.IMEI),
            ColumnTags.SERIAL_LAB            => request.Ascending ? deviceQuery.OrderBy(d => d.SerialLab)           : deviceQuery.OrderByDescending(d => d.SerialLab),
            ColumnTags.SERIAL_NUMBER         => request.Ascending ? deviceQuery.OrderBy(d => d.SerialNumber)        : deviceQuery.OrderByDescending(d => d.SerialNumber),
            ColumnTags.CIRCUIT_SERIAL_NUMBER  => request.Ascending ? deviceQuery.OrderBy(d => d.CircuitSerialNumber) : deviceQuery.OrderByDescending(d => d.CircuitSerialNumber),
            ColumnTags.HW_VERSION            => request.Ascending ? deviceQuery.OrderBy(d => d.HWVersion)           : deviceQuery.OrderByDescending(d => d.HWVersion),
            _                                => request.Ascending ? deviceQuery.OrderBy(d => d.Name)                : deviceQuery.OrderByDescending(d => d.Name),
        };

        var pagedIds = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => d.Id)
            .ToListAsync();

        // 5. Late JOIN — only join the paged rows (e.g. 20) with Models for ModelName
        var items = await _context.Devices
            .AsNoTracking()
            .Where(d => pagedIds.Contains(d.Id))
            .Join(_context.Models,
                d => d.ModelId,
                m => m.Id,
                (d, m) => new Device
                {
                    Id                  = d.Id,
                    ModelId             = d.ModelId,
                    Name                = d.Name,
                    ModelName           = m.Name,
                    IMEI                = d.IMEI,
                    SerialLab           = d.SerialLab,
                    SerialNumber        = d.SerialNumber,
                    CircuitSerialNumber = d.CircuitSerialNumber,
                    HWVersion           = d.HWVersion,
                    Status              = d.Status,
                })
            .ToListAsync();

        // 6. Re-order in memory to match sort from step 4 (only ~20 items, instant)
        var sortedItems = pagedIds
            .Select(id => items.First(x => x.Id == id))
            .ToList();

        return (sortedItems, totalCount);
    }

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

        var updatedCount = await _context.Devices
            .Where(d => candidateIds.Contains(d.Id) && d.Status == "Available")
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, "Borrowed"));

        if (updatedCount < quantity)
            throw new InvalidOperationException(
                "Concurrent borrow detected: some devices were borrowed by another operation. Please try again.");
    }

    public async Task ReturnAsync(Guid deviceId)
    {
        var updatedCount = await _context.Devices
            .Where(d => d.Id == deviceId && d.Status == "Borrowed")
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.Status, "Available"));

        if (updatedCount == 0)
            throw new InvalidOperationException(
                $"Device {deviceId} is not currently borrowed or does not exist.");
    }

}
