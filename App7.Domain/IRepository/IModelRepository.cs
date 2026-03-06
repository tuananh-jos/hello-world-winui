using App7.Domain.Entities;

namespace App7.Domain.IRepository;

public interface IModelRepository
{
    /// <summary>
    /// Returns a paginated, filtered, and sorted list of models,
    /// along with the total count matching the filter (for pagination UI).
    /// </summary>
    Task<(IEnumerable<Model> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchName,
        string? searchManufacturer,
        string? filterCategory,
        string? filterSubCategory,
        string? sortColumn,
        bool ascending);

    /// <summary>
    /// Returns all distinct Manufacturer values.
    /// </summary>
    Task<IEnumerable<string>> GetManufacturersAsync();

    /// <summary>
    /// Returns all distinct Category values (for filter dropdown).
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync();

    /// <summary>
    /// Returns all distinct SubCategory values for a given category.
    /// </summary>
    Task<IEnumerable<string>> GetSubCategoriesAsync(string category);

    /// <summary>
    /// Increments the Available count for a model (called after a device is returned).
    /// </summary>
    Task IncrementAvailableAsync(Guid modelId);

    /// <summary>
    /// Decrements the Available count for a model (called after a device is borrowed).
    /// </summary>
    Task DecrementAvailableAsync(Guid modelId, int quantity);

    // Legacy — kept for compatibility if needed
    Task AddAsync(Model model);
}
