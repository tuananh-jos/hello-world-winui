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
        //modelBuilder.Entity<Model>()
        //    .HasIndex(o => o.OrderID);

        //modelBuilder.Entity<Model>()
        //    .HasIndex(o => o.Status);

        //modelBuilder.Entity<Model>()
        //    .HasIndex(o => o.Company);

        //modelBuilder.Entity<Model>()
        //    .HasIndex(o => o.OrderDate);

        //modelBuilder.Entity<Model>()
        //    .HasIndex(o => new { o.Status, o.OrderDate });
    }

}
