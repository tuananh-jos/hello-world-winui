using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App7.Data.DataSource;

public class ModelDataSource : IModelDataSource
{
    private readonly AppDbContext _context;

    public ModelDataSource(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Model> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchName,
        string? searchManufacturer,
        string? filterCategory,
        string? filterSubCategory,
        string? sortColumn,
        bool ascending)
    {
        var query = _context.Models.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(searchName))
            query = query.Where(m => m.Name.ToLower().Contains(searchName.ToLower()));

        if (!string.IsNullOrWhiteSpace(searchManufacturer))
            query = query.Where(m => m.Manufacturer == searchManufacturer);

        // Filter
        if (!string.IsNullOrWhiteSpace(filterCategory))
            query = query.Where(m => m.Category == filterCategory);

        if (!string.IsNullOrWhiteSpace(filterSubCategory))
            query = query.Where(m => m.SubCategory == filterSubCategory);

        // Total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Sort
        query = (sortColumn?.ToLowerInvariant()) switch
        {
            "manufacturer" => ascending ? query.OrderBy(m => m.Manufacturer) : query.OrderByDescending(m => m.Manufacturer),
            "category"     => ascending ? query.OrderBy(m => m.Category)     : query.OrderByDescending(m => m.Category),
            "subcategory"  => ascending ? query.OrderBy(m => m.SubCategory)  : query.OrderByDescending(m => m.SubCategory),
            "available"    => ascending ? query.OrderBy(m => m.Available)    : query.OrderByDescending(m => m.Available),
            _              => ascending ? query.OrderBy(m => m.Name)          : query.OrderByDescending(m => m.Name),
        };

        // Pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
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

    public async Task InsertAsync(Model model)
    {
        _context.Models.Add(model);
        await _context.SaveChangesAsync();
    }
}
