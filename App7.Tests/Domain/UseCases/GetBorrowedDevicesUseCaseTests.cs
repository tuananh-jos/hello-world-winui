using App7.Domain.Dtos;
using App7.Domain.Entities;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using Moq;

namespace App7.Tests.Domain.UseCases;

[TestFixture]
public class GetBorrowedDevicesUseCaseTests
{
    private Mock<IDeviceRepository> _deviceRepoMock = null!;
    private GetBorrowedDevicesUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _deviceRepoMock = new Mock<IDeviceRepository>();
        _sut = new GetBorrowedDevicesUseCase(_deviceRepoMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_DelegatesCorrectRequest()
    {
        var request = new GetBorrowedDevicesRequest(Page: 1, PageSize: 10, SearchName: "test");
        _deviceRepoMock.Setup(r => r.GetBorrowedPagedAsync(request))
            .ReturnsAsync((new List<Device>(), 0));

        await _sut.ExecuteAsync(request);

        _deviceRepoMock.Verify(r => r.GetBorrowedPagedAsync(request), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ReturnsRepositoryResult()
    {
        var expectedItems = new List<Device>
        {
            new() { Id = Guid.NewGuid(), Name = "Device 1" },
            new() { Id = Guid.NewGuid(), Name = "Device 2" }
        };
        var request = new GetBorrowedDevicesRequest(1, 10);
        _deviceRepoMock.Setup(r => r.GetBorrowedPagedAsync(request))
            .ReturnsAsync((expectedItems, 10));

        var result = await _sut.ExecuteAsync(request);

        Assert.That(result.TotalCount, Is.EqualTo(10));
        Assert.That(result.Items, Is.EqualTo(expectedItems));
    }
}
