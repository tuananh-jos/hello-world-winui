using System.Collections.Generic;
using System.Reflection.Emit;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace App7.Data.Db;

public class AppDbContext : DbContext
{
    public DbSet<Model> Models
    {
        get; set;
    }

    public DbSet<Device> Devices
    {
        get; set;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        // Lấy Connection từ Options
        var connection = Database.GetDbConnection();

        // Đảm bảo Connection được mở để chạy lệnh PRAGMA
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        using (var command = connection.CreateCommand())
        {
            // Bộ 3 quyền lực giúp SQLite chạy cực nhanh
            command.CommandText = @"
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = OFF;
            PRAGMA temp_store = MEMORY;
            PRAGMA cache_size = -200000; -- Khoảng 100MB cache
        ";
            command.ExecuteNonQueryAsync();
        }
    }
}
