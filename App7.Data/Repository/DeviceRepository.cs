using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Data.IDataSource;
using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Data.Repository;
public class DeviceRepository: IDeviceRepository
{
    private readonly IDeviceDataSource _dataSource;

    public DeviceRepository(IDeviceDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IEnumerable<Device>> GetAllAsync()
    {
        return await _dataSource.GetAllAsync();
    }

    public async Task AddAsync(Device company)
    {
        await _dataSource.InsertAsync(company);
    }
}
