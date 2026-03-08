using App7.Domain.Dtos;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using Moq;

namespace App7.Tests.Domain.UseCases;

[TestFixture]
public class MultiInstanceTests
{
    [Test]
    public async Task ConcurrentBorrow_BothSucceed_WhenEnoughStock()
    {
        // Simulate shared state: available starts at 10
        var available = 10;
        var lockObj = new object();

        Mock<IUnitOfWork> CreateUnitOfWork()
        {
            var uow = new Mock<IUnitOfWork>();
            var devices = new Mock<IDeviceRepository>();
            var models = new Mock<IModelRepository>();

            devices.Setup(d => d.BorrowAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns<Guid, int>((_, qty) =>
                {
                    lock (lockObj)
                    {
                        if (available < qty)
                            throw new InvalidOperationException("Not enough stock");
                        available -= qty;
                    }
                    return Task.CompletedTask;
                });

            models.Setup(m => m.DecrementAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            uow.Setup(u => u.Devices).Returns(devices.Object);
            uow.Setup(u => u.Models).Returns(models.Object);
            return uow;
        }

        var modelId = Guid.NewGuid();

        // Two concurrent borrow of qty=2
        var task1 = Task.Run(async () =>
        {
            var uow = CreateUnitOfWork();
            var useCase = new BorrowDeviceUseCase(uow.Object);
            await useCase.ExecuteAsync(new BorrowDeviceRequest(modelId, 2));
        });

        var task2 = Task.Run(async () =>
        {
            var uow = CreateUnitOfWork();
            var useCase = new BorrowDeviceUseCase(uow.Object);
            await useCase.ExecuteAsync(new BorrowDeviceRequest(modelId, 2));
        });

        await Task.WhenAll(task1, task2);

        // Both succeeded, total borrowed = 4
        Assert.That(available, Is.EqualTo(6));
    }

    [Test]
    public async Task ConcurrentBorrow_OneFailsGracefully_WhenInsufficientStock()
    {
        // Simulate shared state: only 1 available
        var available = 1;
        var lockObj = new object();

        Mock<IUnitOfWork> CreateUnitOfWork()
        {
            var uow = new Mock<IUnitOfWork>();
            var devices = new Mock<IDeviceRepository>();
            var models = new Mock<IModelRepository>();

            devices.Setup(d => d.BorrowAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns<Guid, int>((_, qty) =>
                {
                    lock (lockObj)
                    {
                        if (available < qty)
                            throw new InvalidOperationException("Not enough stock");
                        available -= qty;
                    }
                    return Task.CompletedTask;
                });

            models.Setup(m => m.DecrementAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            uow.Setup(u => u.Devices).Returns(devices.Object);
            uow.Setup(u => u.Models).Returns(models.Object);
            return uow;
        }

        var modelId = Guid.NewGuid();
        var exceptions = new List<Exception>();

        var task1 = Task.Run(async () =>
        {
            try
            {
                var uow = CreateUnitOfWork();
                var useCase = new BorrowDeviceUseCase(uow.Object);
                await useCase.ExecuteAsync(new BorrowDeviceRequest(modelId, 1));
            }
            catch (Exception ex) { lock (exceptions) exceptions.Add(ex); }
        });

        var task2 = Task.Run(async () =>
        {
            try
            {
                var uow = CreateUnitOfWork();
                var useCase = new BorrowDeviceUseCase(uow.Object);
                await useCase.ExecuteAsync(new BorrowDeviceRequest(modelId, 1));
            }
            catch (Exception ex) { lock (exceptions) exceptions.Add(ex); }
        });

        await Task.WhenAll(task1, task2);

        // Exactly one should succeed, one should fail
        Assert.That(exceptions.Count, Is.EqualTo(1),
            "Exactly one concurrent borrow should fail when only 1 device available");
        Assert.That(exceptions[0], Is.TypeOf<InvalidOperationException>());
        Assert.That(available, Is.EqualTo(0));
    }
}
