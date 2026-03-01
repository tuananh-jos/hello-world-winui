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

        if (await _context.SampleOrders.AnyAsync())
            return;

        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Data",
            "orders.json");

        if (!File.Exists(filePath))
            return;

        const int batchSize = 5000;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        using var stream = File.OpenRead(filePath);

        var batch = new List<SampleOrder>(batchSize);

        await foreach (var order in JsonSerializer.DeserializeAsyncEnumerable<SampleOrder>(stream, options))
        {
            if (order == null)
                continue;

            batch.Add(order);

            if (batch.Count >= batchSize)
            {
                await InsertBatchAsync(batch);
                batch.Clear();
            }
        }

        // Insert phần còn lại
        if (batch.Count > 0)
        {
            await InsertBatchAsync(batch);
        }
    }

    private async Task InsertBatchAsync(List<SampleOrder> batch)
    {
        _context.SampleOrders.AddRange(batch);
        await _context.SaveChangesAsync();

        // CỰC KỲ QUAN TRỌNG nếu dữ liệu lớn
        _context.ChangeTracker.Clear();
    }
}

