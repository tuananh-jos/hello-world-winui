using App7.Domain.Dtos;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using Moq;

namespace App7.Tests.Domain.UseCases;

[TestFixture]
public class GetModelFiltersUseCaseTests
{
    private Mock<IModelRepository> _modelRepoMock = null!;
    private GetModelFiltersUseCase _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _modelRepoMock = new Mock<IModelRepository>();
        _sut = new GetModelFiltersUseCase(_modelRepoMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_CombinesAllFilterResults()
    {
        var manufacturers = new List<string> { "Samsung", "Apple" };
        var categories = new List<string> { "Phone", "Tablet" };
        var subCategories = new List<string> { "Flagship", "Mid-range" };

        _modelRepoMock.Setup(r => r.GetManufacturersAsync()).ReturnsAsync(manufacturers);
        _modelRepoMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(categories);
        _modelRepoMock.Setup(r => r.GetSubCategoriesAsync()).ReturnsAsync(subCategories);

        var result = await _sut.ExecuteAsync();

        Assert.That(result.Manufacturers, Is.EqualTo(manufacturers));
        Assert.That(result.Categories, Is.EqualTo(categories));
        Assert.That(result.SubCategories, Is.EqualTo(subCategories));
    }

    [Test]
    public async Task ExecuteAsync_CallsAllThreeMethods()
    {
        _modelRepoMock.Setup(r => r.GetManufacturersAsync()).ReturnsAsync(new List<string>());
        _modelRepoMock.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(new List<string>());
        _modelRepoMock.Setup(r => r.GetSubCategoriesAsync()).ReturnsAsync(new List<string>());

        await _sut.ExecuteAsync();

        _modelRepoMock.Verify(r => r.GetManufacturersAsync(), Times.Once);
        _modelRepoMock.Verify(r => r.GetCategoriesAsync(), Times.Once);
        _modelRepoMock.Verify(r => r.GetSubCategoriesAsync(), Times.Once);
    }

    [Test]
    public void ExecuteAsync_OneThrows_PropagatesException()
    {
        _modelRepoMock.Setup(r => r.GetManufacturersAsync()).ReturnsAsync(new List<string>());
        _modelRepoMock.Setup(r => r.GetCategoriesAsync()).ThrowsAsync(new Exception("DB error"));
        _modelRepoMock.Setup(r => r.GetSubCategoriesAsync()).ReturnsAsync(new List<string>());

        Assert.ThrowsAsync<Exception>(() => _sut.ExecuteAsync());
    }
}
