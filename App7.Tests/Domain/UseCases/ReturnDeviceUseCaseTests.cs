using App7.Domain.Dtos;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using Moq;

namespace App7.Tests.Domain.UseCases;

[TestFixture]
public class ReturnDeviceUseCaseTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IDeviceRepository> _deviceRepoMock = null!;
    private Mock<IModelRepository> _modelRepoMock = null!;
    private ReturnDeviceUseCase _sut = null!;

    private readonly Guid _deviceId = Guid.NewGuid();
    private readonly Guid _modelId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _deviceRepoMock = new Mock<IDeviceRepository>();
        _modelRepoMock = new Mock<IModelRepository>();

        _unitOfWorkMock.Setup(u => u.Devices).Returns(_deviceRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Models).Returns(_modelRepoMock.Object);

        _sut = new ReturnDeviceUseCase(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_Success_CallsReturnAndIncrement()
    {
        var request = new ReturnDeviceRequest(_deviceId, _modelId);

        await _sut.ExecuteAsync(request);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _deviceRepoMock.Verify(r => r.ReturnAsync(_deviceId), Times.Once);
        _modelRepoMock.Verify(r => r.IncrementAvailableAsync(_modelId), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_Success_CallsInCorrectOrder()
    {
        var callOrder = new List<string>();

        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .Callback(() => callOrder.Add("Begin")).Returns(Task.CompletedTask);
        _deviceRepoMock.Setup(r => r.ReturnAsync(It.IsAny<Guid>()))
            .Callback(() => callOrder.Add("Return")).Returns(Task.CompletedTask);
        _modelRepoMock.Setup(r => r.IncrementAvailableAsync(It.IsAny<Guid>()))
            .Callback(() => callOrder.Add("Increment")).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync())
            .Callback(() => callOrder.Add("Commit")).Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(new ReturnDeviceRequest(_deviceId, _modelId));

        Assert.That(callOrder, Is.EqualTo(new[] { "Begin", "Return", "Increment", "Commit" }));
    }

    [Test]
    public void ExecuteAsync_ReturnThrows_RollsBack()
    {
        _deviceRepoMock.Setup(r => r.ReturnAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Device not borrowed"));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ExecuteAsync(new ReturnDeviceRequest(_deviceId, _modelId)));

        _modelRepoMock.Verify(r => r.IncrementAvailableAsync(It.IsAny<Guid>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Test]
    public void ExecuteAsync_IncrementThrows_RollsBack()
    {
        _modelRepoMock.Setup(r => r.IncrementAvailableAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("DB error"));

        Assert.ThrowsAsync<Exception>(
            () => _sut.ExecuteAsync(new ReturnDeviceRequest(_deviceId, _modelId)));

        _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}
