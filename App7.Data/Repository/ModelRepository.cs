using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Domain.Entities;
using App7.Data.IDataSource;
using App7.Domain.IRepository;

namespace App7.Data.Repository;
public class ModelRepository: IModelRepository
{
    private readonly IModelDataSource _dataSource;

    public ModelRepository(IModelDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IEnumerable<Model>> GetAllAsync()
    {
        return await _dataSource.GetAllAsync();
    }

    public async Task AddAsync(Model order)
    {
        await _dataSource.InsertAsync(order);
    }
}
