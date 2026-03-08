using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App7.Data.DataSource;

public abstract class DataSourceBase<TBaseEntity> : IDataSourceBase<TBaseEntity> where TBaseEntity : class, IEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<TBaseEntity> _dbSet;

    protected DataSourceBase(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TBaseEntity>();
    }

    public virtual async Task InsertAsync(TBaseEntity entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
    }

    protected async Task<(List<TEntity> Items, int TotalCount)> GetPagedInternalAsync<TEntity>(
        IQueryable<TEntity> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
}
