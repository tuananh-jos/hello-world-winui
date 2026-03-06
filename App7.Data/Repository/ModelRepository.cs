using App7.Data.IDataSource;
using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Data.Repository;

public class ModelRepository : IModelRepository
{
    private readonly IModelDataSource _dataSource;

    public ModelRepository(IModelDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<(IEnumerable<Model> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? searchName, string? searchManufacturer,
        string? filterCategory, string? filterSubCategory,
        string? sortColumn, bool ascending)
    {
        var result = await _dataSource.GetPagedAsync(
            page, pageSize,
            searchName, searchManufacturer,
            filterCategory, filterSubCategory,
            sortColumn, ascending);
        return (result.Items, result.TotalCount);
    }

    public async Task<IEnumerable<string>> GetManufacturersAsync()
        => await _dataSource.GetManufacturersAsync();

    public async Task<IEnumerable<string>> GetCategoriesAsync()
        => await _dataSource.GetCategoriesAsync();

    public async Task<IEnumerable<string>> GetSubCategoriesAsync(string category)
        => await _dataSource.GetSubCategoriesAsync(category);

    public async Task IncrementAvailableAsync(Guid modelId)
        => await _dataSource.IncrementAvailableAsync(modelId);

    public async Task DecrementAvailableAsync(Guid modelId, int quantity)
        => await _dataSource.DecrementAvailableAsync(modelId, quantity);

    public async Task AddAsync(Model model)
        => await _dataSource.InsertAsync(model);
}
