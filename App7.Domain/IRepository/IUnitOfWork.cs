namespace App7.Domain.IRepository;

public interface IUnitOfWork : IDisposable
{
    IDeviceRepository Devices { get; }
    IModelRepository Models { get; }

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
