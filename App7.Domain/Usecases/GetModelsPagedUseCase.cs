using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Domain.Usecases;

/// <summary>
/// UC1 + UC2: Returns a paginated, filtered, and sorted list of device models.
/// </summary>
public class GetModelsPagedUseCase
{
    private readonly IModelRepository _modelRepository;

    public GetModelsPagedUseCase(IModelRepository modelRepository)
    {
        _modelRepository = modelRepository;
    }

    public async Task<(IEnumerable<Model> Items, int TotalCount)> ExecuteAsync(
        int page,
        int pageSize,
        string? searchName = null,
        string? searchManufacturer = null,
        string? filterCategory = null,
        string? filterSubCategory = null,
        string? sortColumn = null,
        bool ascending = true)
    {
        return await _modelRepository.GetPagedAsync(
            page, pageSize,
            searchName, searchManufacturer,
            filterCategory, filterSubCategory,
            sortColumn, ascending);
    }
}
