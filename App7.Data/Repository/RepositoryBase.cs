using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App7.Data.IDataSource;
using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Data.Repository;

public abstract class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class, IEntity
{
    protected readonly IDataSourceBase<TEntity> _dataSource;

    protected RepositoryBase(IDataSourceBase<TEntity> dataSource)
    {
        _dataSource = dataSource;
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await _dataSource.InsertAsync(entity);
    }
}
