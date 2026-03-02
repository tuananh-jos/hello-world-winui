using System.Text.Json;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        await SeedModelsAsync(modelPath);
        await SeedDevicesAsync(devicePath);
    }

    private async Task SeedModelsAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        const int batchSize = 2000;
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

        const int batchSize = 5000;
        var batch = new List<Device>(batchSize);

        await foreach (var device in JsonSerializer
            .DeserializeAsyncEnumerable<Device>(stream, options))
        {
            if (device == null)
                continue;

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
        _context.Models.AddRange(batch);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    private async Task InsertDevicesBatchAsync(List<Device> batch)
    {
        _context.Devices.AddRange(batch);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }
}

