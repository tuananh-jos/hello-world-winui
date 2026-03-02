using System.Collections.Generic;
using System.Reflection.Emit;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Model indexes — support search by Name, Manufacturer, filter by Category/SubCategory
        modelBuilder.Entity<Model>()
            .HasIndex(m => m.Name);

        modelBuilder.Entity<Model>()
            .HasIndex(m => m.Manufacturer);

        modelBuilder.Entity<Model>()
            .HasIndex(m => m.Category);

        modelBuilder.Entity<Model>()
            .HasIndex(m => m.SubCategory);

        // Composite index for combined Category + SubCategory filter
        modelBuilder.Entity<Model>()
            .HasIndex(m => new { m.Category, m.SubCategory });

        // Device indexes — support multi-field search + HWVersion filter
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.ModelId);

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.Name);

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.IMEI);

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.SerialLab);

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.SerialNumber);

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.CircuitSerialNumber);

        modelBuilder.Entity<Device>()
            .HasIndex(d => d.HWVersion);

        // Status index — support "My Devices" query (WHERE Status = 'Borrowed')
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.Status);

        // Status default value — so JSON import rows without Status are 'Available'
        modelBuilder.Entity<Device>()
            .Property(d => d.Status)
            .HasDefaultValue("Available");
    }

}
