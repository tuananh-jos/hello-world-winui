using System.Text.Json;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace App7.Data.Db;
public class DatabaseInitializer
{
    private readonly AppDbContext _context;

    public DatabaseInitializer(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        if (await _context.Models.AnyAsync())
            return;

        var basePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Data");

        var modelPath = Path.Combine(basePath, "models.json");
        var devicePath = Path.Combine(basePath, "devices.json");

        if (!File.Exists(modelPath) || !File.Exists(devicePath))
        {
            await SampleDataGenerator.GenerateAsync();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await SeedModelsAsync(modelPath);
            await SeedDevicesAsync(devicePath);

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }



        await CreateIndexes();

        await _context.Database.ExecuteSqlRawAsync("VACUUM;");
    }

    private async Task CreateIndexes()
    {
        // Model indexes
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Models_Name ON Models (Name);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Models_Manufacturer ON Models (Manufacturer);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Models_Category ON Models (Category);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Models_SubCategory ON Models (SubCategory);");

        // Device composite indexes — (Status, Column) for efficient WHERE Status='Borrowed' + sort/search
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_Status_Name ON Devices (Status, Name);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_Status_ModelId ON Devices (Status, ModelId);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_Status_IMEI ON Devices (Status, IMEI);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_Status_SerialLab ON Devices (Status, SerialLab);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_Status_SerialNumber ON Devices (Status, SerialNumber);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_Status_CircuitSerial ON Devices (Status, CircuitSerialNumber);");
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_Status_HWVersion ON Devices (Status, HWVersion);");

        // Device single-column indexes — for BorrowAsync/ReturnAsync lookups
        await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Devices_ModelId ON Devices (ModelId);");
    }

    private async Task SeedModelsAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        const int batchSize = 50000;
        var batch = new List<Model>(batchSize);

        await foreach (var model in JsonSerializer
            .DeserializeAsyncEnumerable<Model>(stream, options))
        {
            if (model == null)
                continue;

            batch.Add(model);

            if (batch.Count >= batchSize)
            {
                await InsertModelsBatchAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            await InsertModelsBatchAsync(batch);
    }

    private async Task SeedDevicesAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        const int batchSize = 100000;
        var batch = new List<Device>(batchSize);

        await foreach (var device in JsonSerializer
            .DeserializeAsyncEnumerable<Device>(stream, options))
        {
            if (device == null)
                continue;

            device.Status = "Borrowed";

            batch.Add(device);

            if (batch.Count >= batchSize)
            {
                await InsertDevicesBatchAsync(batch);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            await InsertDevicesBatchAsync(batch);
    }

    private async Task InsertModelsBatchAsync(List<Model> batch)
    {
        //_context.Models.AddRange(batch);
        //await _context.SaveChangesAsync();
        //_context.ChangeTracker.Clear();

        await _context.BulkInsertAsync(batch, config => {
            config.BatchSize = batch.Count;
            config.CalculateStats = false;
            config.IncludeGraph = false;
            config.PreserveInsertOrder = false;
        });
    }

    private async Task InsertDevicesBatchAsync(List<Device> batch)
    {
        //_+context.Devices.AddRange(batch);
        //await _context.SaveChangesAsync();
        //_context.ChangeTracker.Clear();

        await _context.BulkInsertAsync(batch, config => {
            config.BatchSize = batch.Count;
            config.CalculateStats = false;
            config.IncludeGraph = false;
            config.PreserveInsertOrder = false;
        });
    }
}

