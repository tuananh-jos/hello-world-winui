using App7.Domain.Dtos;
using App7.Domain.Entities;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using Moq;

namespace App7.Tests.Domain.UseCases;

[TestFixture]
public class GetModelsPagedUseCaseTests
{
    private Mock<IModelRepository> _modelRepoMock = null!;
    private GetModelsPagedUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _modelRepoMock = new Mock<IModelRepository>();
        _sut = new GetModelsPagedUseCase(_modelRepoMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_DelegatesCorrectRequest()
    {
        var request = new GetModelsPagedRequest(Page: 2, PageSize: 10, SearchName: "Galaxy");
        _modelRepoMock.Setup(r => r.GetPagedAsync(request))
            .ReturnsAsync((new List<Model>(), 0));

        await _sut.ExecuteAsync(request);

        _modelRepoMock.Verify(r => r.GetPagedAsync(request), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ReturnsRepositoryResult()
    {
        var expectedItems = new List<Model>
        {
            new() { Id = Guid.NewGuid(), Name = "Model A" },
            new() { Id = Guid.NewGuid(), Name = "Model B" }
        };
        var request = new GetModelsPagedRequest(1, 10);
        _modelRepoMock.Setup(r => r.GetPagedAsync(request))
            .ReturnsAsync((expectedItems, 42));

        var result = await _sut.ExecuteAsync(request);

        Assert.That(result.TotalCount, Is.EqualTo(42));
        Assert.That(result.Items, Is.EqualTo(expectedItems));
    }
}
