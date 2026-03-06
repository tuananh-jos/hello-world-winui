using App7.Domain.Entities;

namespace App7.Data.IDataSource;

public interface IModelDataSource
{
    /// <summary>Returns a chunk of all models for initial in-memory load. offset = 0-based row offset.</summary>
    Task<List<Model>> GetChunkAsync(int offset, int chunkSize);

    Task<(List<Model> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchName,
        string? searchManufacturer,
        string? filterCategory,
        string? filterSubCategory,
        string? sortColumn,
        bool ascending);

    Task<List<string>> GetCategoriesAsync();

    Task<List<string>> GetSubCategoriesAsync(string category);

    Task IncrementAvailableAsync(Guid modelId);

    Task DecrementAvailableAsync(Guid modelId, int quantity);

    Task InsertAsync(Model model);
}
