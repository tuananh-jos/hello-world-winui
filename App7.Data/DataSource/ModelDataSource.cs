using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public async Task<List<Model>> GetAllAsync()
    {
        return await _context.Models
            .AsNoTracking()
            .ToListAsync();
    }

    public Task InsertAsync(Model order) => throw new NotImplementedException();
}
