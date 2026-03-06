using App7.Domain.IRepository;

namespace App7.Domain.Usecases;

/// <summary>
/// UC2 helper: provides distinct Category and SubCategory lists for filter dropdowns.
/// </summary>
public class GetModelFiltersUseCase
{
    private readonly IModelRepository _modelRepository;

    public GetModelFiltersUseCase(IModelRepository modelRepository)
    {
        _modelRepository = modelRepository;
    }

    public async Task<IEnumerable<string>> GetManufacturersAsync()
        => await _modelRepository.GetManufacturersAsync();

    public async Task<IEnumerable<string>> GetCategoriesAsync()
        => await _modelRepository.GetCategoriesAsync();

    public async Task<IEnumerable<string>> GetSubCategoriesAsync(string category)
        => await _modelRepository.GetSubCategoriesAsync(category);
}
