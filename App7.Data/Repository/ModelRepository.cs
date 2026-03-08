using App7.Data.IDataSource;
using App7.Domain.Entities;
using App7.Domain.IRepository;
using App7.Domain.Dtos;

namespace App7.Data.Repository;

public class ModelRepository : RepositoryBase<Model>, IModelRepository
{
    private IModelDataSource ModelDataSource => (IModelDataSource)_dataSource;

    public ModelRepository(IModelDataSource dataSource) : base(dataSource)
    {
    }

    public async Task<(IEnumerable<Model> Items, int TotalCount)> GetPagedAsync(GetModelsPagedRequest request)
    {
        var result = await ModelDataSource.GetPagedAsync(request);
        return (result.Items, result.TotalCount);
    }

    public async Task<IEnumerable<string>> GetManufacturersAsync()
        => await ModelDataSource.GetManufacturersAsync();

    public async Task<IEnumerable<string>> GetCategoriesAsync()
        => await ModelDataSource.GetCategoriesAsync();

    public async Task<IEnumerable<string>> GetSubCategoriesAsync()
        => await ModelDataSource.GetSubCategoriesAsync();

    public async Task IncrementAvailableAsync(Guid modelId)
        => await ModelDataSource.IncrementAvailableAsync(modelId);

    public async Task DecrementAvailableAsync(Guid modelId, int quantity)
        => await ModelDataSource.DecrementAvailableAsync(modelId, quantity);
}
