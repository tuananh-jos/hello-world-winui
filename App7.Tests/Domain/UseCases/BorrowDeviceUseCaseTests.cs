using App7.Domain.Dtos;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using Moq;

namespace App7.Tests.Domain.UseCases;

[TestFixture]
public class BorrowDeviceUseCaseTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IDeviceRepository> _deviceRepoMock = null!;
    private Mock<IModelRepository> _modelRepoMock = null!;
    private BorrowDeviceUseCase _sut = null!;

    private readonly Guid _modelId = Guid.NewGuid();
    private const int Quantity = 2;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _deviceRepoMock = new Mock<IDeviceRepository>();
        _modelRepoMock = new Mock<IModelRepository>();

        _unitOfWorkMock.Setup(u => u.Devices).Returns(_deviceRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Models).Returns(_modelRepoMock.Object);

        _sut = new BorrowDeviceUseCase(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_Success_CallsBorrowAndDecrement()
    {
        var request = new BorrowDeviceRequest(_modelId, Quantity);

        await _sut.ExecuteAsync(request);

        _deviceRepoMock.Verify(r => r.BorrowAsync(_modelId, Quantity), Times.Once);
        _modelRepoMock.Verify(r => r.DecrementAvailableAsync(_modelId, Quantity), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_Success_CallsInCorrectOrder()
    {
        var callOrder = new List<string>();

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .Callback(() => callOrder.Add("Begin")).Returns(Task.CompletedTask);
        _deviceRepoMock.Setup(r => r.BorrowAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .Callback(() => callOrder.Add("Borrow")).Returns(Task.CompletedTask);
        _modelRepoMock.Setup(r => r.DecrementAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .Callback(() => callOrder.Add("Decrement")).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .Callback(() => callOrder.Add("Commit")).Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(new BorrowDeviceRequest(_modelId, Quantity));

        Assert.That(callOrder, Is.EqualTo(new[] { "Begin", "Borrow", "Decrement", "Commit" }));
    }

    [Test]
    public void ExecuteAsync_BorrowThrows_RollsBackAndRethrows()
    {
        _deviceRepoMock.Setup(r => r.BorrowAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Not enough stock"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ExecuteAsync(new BorrowDeviceRequest(_modelId, Quantity)));

        _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Test]
    public void ExecuteAsync_DecrementThrows_RollsBackAndRethrows()
    {
        _modelRepoMock.Setup(r => r.DecrementAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("DB error"));

        Assert.ThrowsAsync<Exception>(
            () => _sut.ExecuteAsync(new BorrowDeviceRequest(_modelId, Quantity)));

        _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}
