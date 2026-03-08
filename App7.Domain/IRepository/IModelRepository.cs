using App7.Domain.Entities;
using App7.Domain.Dtos;

namespace App7.Domain.IRepository;

public interface IModelRepository : IRepositoryBase<Model>
{

    Task<(IEnumerable<Model> Items, int TotalCount)> GetPagedAsync(GetModelsPagedRequest request);

    Task<IEnumerable<string>> GetManufacturersAsync();

    Task<IEnumerable<string>> GetCategoriesAsync();

    Task<IEnumerable<string>> GetSubCategoriesAsync();

    Task IncrementAvailableAsync(Guid modelId);

    Task DecrementAvailableAsync(Guid modelId, int quantity);

}
