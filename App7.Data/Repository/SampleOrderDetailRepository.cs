using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Data.IDataSource;
using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Data.Repository;
public class SampleOrderDetailRepository :ISampleOrderDetailRepository
{
    private readonly ISampleOrderDetailDataSource _dataSource;

    public SampleOrderDetailRepository(ISampleOrderDetailDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IEnumerable<SampleOrderDetail>> GetAllAsync()
    {
        return await _dataSource.GetAllAsync();
    }

    public async Task AddAsync(SampleOrderDetail orderDetail)
    {
        await _dataSource.InsertAsync(orderDetail);
    }
}
