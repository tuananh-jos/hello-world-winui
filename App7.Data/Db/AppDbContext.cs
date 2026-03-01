using System.Collections.Generic;
using System.Reflection.Emit;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App7.Data.Db;

public class AppDbContext : DbContext
{
    public DbSet<SampleOrder> SampleOrders
    {
        get; set;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SampleOrder>()
            .HasIndex(o => o.OrderID);

        modelBuilder.Entity<SampleOrder>()
            .HasIndex(o => o.Status);

        modelBuilder.Entity<SampleOrder>()
            .HasIndex(o => o.Company);

        modelBuilder.Entity<SampleOrder>()
            .HasIndex(o => o.OrderDate);

        modelBuilder.Entity<SampleOrder>()
            .HasIndex(o => new { o.Status, o.OrderDate });
    }

}
