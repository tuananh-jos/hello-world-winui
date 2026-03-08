using App7.Data.Db;
using App7.Data.DataSource;
using App7.Data.IDataSource;
using App7.Domain.IRepository;
using Microsoft.EntityFrameworkCore.Storage;

namespace App7.Data.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public IDeviceRepository Devices { get; }
    public IModelRepository Models { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        var deviceDs = new DeviceDataSource(context);
        var modelDs = new ModelDataSource(context);
        Devices = new DeviceRepository(deviceDs);
        Models = new ModelRepository(modelDs);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
