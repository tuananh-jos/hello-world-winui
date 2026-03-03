using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Domain.Usecases;

/// <summary>
/// Legacy use case kept for backward compatibility with DataGridViewModel.
/// New code should use GetModelsPagedUseCase instead.
/// </summary>
public class GetModelsUseCase
{
    private readonly IModelRepository _modelRepository;

    public GetModelsUseCase(IModelRepository modelRepository)
    {
        _modelRepository = modelRepository;
    }

    /// <summary>
    /// Returns the first page of models (up to 50) with no filters applied.
    /// </summary>
    public async Task<IEnumerable<Model>> ExecuteAsync()
    {
        var (items, _) = await _modelRepository.GetPagedAsync(
            page: 1,
            pageSize: 50,
            searchName: null,
            searchManufacturer: null,
            filterCategory: null,
            filterSubCategory: null,
            sortColumn: null,
            ascending: true);

        return items;
    }
}
