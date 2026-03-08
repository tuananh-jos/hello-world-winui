using App7.Domain.Entities;
using App7.Domain.Dtos;

namespace App7.Data.IDataSource;

public interface IModelDataSource : IDataSourceBase<Model>
{
    Task<(List<Model> Items, int TotalCount)> GetPagedAsync(GetModelsPagedRequest request);

    Task<List<string>> GetManufacturersAsync();

    Task<List<string>> GetCategoriesAsync();

    Task<List<string>> GetSubCategoriesAsync();

    Task IncrementAvailableAsync(Guid modelId);

    Task DecrementAvailableAsync(Guid modelId, int quantity);

}
