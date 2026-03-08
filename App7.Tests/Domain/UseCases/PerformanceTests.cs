using System.Diagnostics;
using App7.Domain.Dtos;
using App7.Domain.Entities;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using Moq;

namespace App7.Tests.Domain.UseCases;

[TestFixture]
public class PerformanceTests
{
    private const int MaxAllowedMs = 1000;

    [Test]
    public async Task BorrowDeviceUseCase_CompletesUnder1Second()
    {
        var uow = CreateMockUnitOfWork();
        var sut = new BorrowDeviceUseCase(uow.Object);

        var sw = Stopwatch.StartNew();
        await sut.ExecuteAsync(new BorrowDeviceRequest(Guid.NewGuid(), 1));
        sw.Stop();

        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(MaxAllowedMs),
            $"BorrowDeviceUseCase took {sw.ElapsedMilliseconds}ms");
    }

    [Test]
    public async Task ReturnDeviceUseCase_CompletesUnder1Second()
    {
        var uow = CreateMockUnitOfWork();
        var sut = new ReturnDeviceUseCase(uow.Object);

        var sw = Stopwatch.StartNew();
        await sut.ExecuteAsync(new ReturnDeviceRequest(Guid.NewGuid(), Guid.NewGuid()));
        sw.Stop();

        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(MaxAllowedMs),
            $"ReturnDeviceUseCase took {sw.ElapsedMilliseconds}ms");
    }

    [Test]
    public async Task GetModelsPagedUseCase_CompletesUnder1Second()
    {
        var repo = new Mock<IModelRepository>();
        repo.Setup(r => r.GetPagedAsync(It.IsAny<GetModelsPagedRequest>()))
            .ReturnsAsync((new List<Model>(), 0));
        var sut = new GetModelsPagedUseCase(repo.Object);

        var sw = Stopwatch.StartNew();
        await sut.ExecuteAsync(new GetModelsPagedRequest(1, 10));
        sw.Stop();

        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(MaxAllowedMs),
            $"GetModelsPagedUseCase took {sw.ElapsedMilliseconds}ms");
    }

    [Test]
    public async Task GetBorrowedDevicesUseCase_CompletesUnder1Second()
    {
        var repo = new Mock<IDeviceRepository>();
        repo.Setup(r => r.GetBorrowedPagedAsync(It.IsAny<GetBorrowedDevicesRequest>()))
            .ReturnsAsync((new List<Device>(), 0));
        var sut = new GetBorrowedDevicesUseCase(repo.Object);

        var sw = Stopwatch.StartNew();
        await sut.ExecuteAsync(new GetBorrowedDevicesRequest(1, 10));
        sw.Stop();

        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(MaxAllowedMs),
            $"GetBorrowedDevicesUseCase took {sw.ElapsedMilliseconds}ms");
    }

    [Test]
    public async Task GetModelFiltersUseCase_CompletesUnder1Second()
    {
        var repo = new Mock<IModelRepository>();
        repo.Setup(r => r.GetManufacturersAsync()).ReturnsAsync(new List<string>());
        repo.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(new List<string>());
        repo.Setup(r => r.GetSubCategoriesAsync()).ReturnsAsync(new List<string>());
        var sut = new GetModelFiltersUseCase(repo.Object);

        var sw = Stopwatch.StartNew();
        await sut.ExecuteAsync();
        sw.Stop();

        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(MaxAllowedMs),
            $"GetModelFiltersUseCase took {sw.ElapsedMilliseconds}ms");
    }

    private static Mock<IUnitOfWork> CreateMockUnitOfWork()
    {
        var uow = new Mock<IUnitOfWork>();
        var devices = new Mock<IDeviceRepository>();
        var models = new Mock<IModelRepository>();
        uow.Setup(u => u.Devices).Returns(devices.Object);
        uow.Setup(u => u.Models).Returns(models.Object);
        return uow;
    }
}
