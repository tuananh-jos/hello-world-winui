using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Domain.Constants;
using App7.Domain.Entities;
using App7.Domain.Dtos;
using Microsoft.EntityFrameworkCore;

namespace App7.Data.DataSource;

public class ModelDataSource : DataSourceBase<Model>, IModelDataSource
{
    public ModelDataSource(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Model> Items, int TotalCount)> GetPagedAsync(GetModelsPagedRequest request)
    {
        var query = _context.Models.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchName))
            query = query.Where(m => EF.Functions.Like(m.Name, $"%{request.SearchName}%"));

        if (!string.IsNullOrWhiteSpace(request.SearchManufacturer))
            query = query.Where(m => m.Manufacturer == request.SearchManufacturer);

        // Filter
        if (!string.IsNullOrWhiteSpace(request.FilterCategory))
            query = query.Where(m => m.Category == request.FilterCategory);

        if (!string.IsNullOrWhiteSpace(request.FilterSubCategory))
            query = query.Where(m => m.SubCategory == request.FilterSubCategory);


        // Sort
        query = request.SortColumn switch
        {
            ColumnTags.MANUFACTURER => request.Ascending ? query.OrderBy(m => m.Manufacturer) : query.OrderByDescending(m => m.Manufacturer),
            ColumnTags.CATEGORY     => request.Ascending ? query.OrderBy(m => m.Category)     : query.OrderByDescending(m => m.Category),
            ColumnTags.SUB_CATEGORY  => request.Ascending ? query.OrderBy(m => m.SubCategory)  : query.OrderByDescending(m => m.SubCategory),
            ColumnTags.AVAILABLE    => request.Ascending ? query.OrderBy(m => m.Available)    : query.OrderByDescending(m => m.Available),
            _                       => request.Ascending ? query.OrderBy(m => m.Name)         : query.OrderByDescending(m => m.Name),
        };

        // Pagination
        return await GetPagedInternalAsync(query, request.Page, request.PageSize);
    }

    public async Task<List<string>> GetManufacturersAsync()
    {
        return await _context.Models
            .AsNoTracking()
            .Select(m => m.Manufacturer)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _context.Models
            .AsNoTracking()
            .Select(m => m.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<string>> GetSubCategoriesAsync()
    {
        return await _context.Models
            .AsNoTracking()
            .Select(m => m.SubCategory)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task IncrementAvailableAsync(Guid modelId)
    {
        await _context.Models
            .Where(m => m.Id == modelId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Available, m => m.Available + 1));
    }

    public async Task DecrementAvailableAsync(Guid modelId, int quantity)
    {
        await _context.Models
            .Where(m => m.Id == modelId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Available, m => m.Available - quantity));
    }

}
