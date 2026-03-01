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
public class SampleOrderDataSource : ISampleOrderDataSource
{
    private readonly AppDbContext _context;

    public SampleOrderDataSource(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<SampleOrder>> GetAllAsync()
    {
        return _context.SampleOrders.ToListAsync();
    }

    public Task InsertAsync(SampleOrder order) => throw new NotImplementedException();
}
