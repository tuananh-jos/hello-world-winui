using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Domain.Entities;
using App7.Data.IDataSource;
using App7.Domain.IRepository;

namespace App7.Data.Repository;
public class SampleOrderRepository: ISampleOrderRepository
{
    private readonly ISampleOrderDataSource _dataSource;

    public SampleOrderRepository(ISampleOrderDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IEnumerable<SampleOrder>> GetAllAsync()
    {
        return await _dataSource.GetAllAsync();
    }

    public async Task AddAsync(SampleOrder order)
    {
        await _dataSource.InsertAsync(order);
    }
}
