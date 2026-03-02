using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Data.IDataSource;
using App7.Domain.Entities;

namespace App7.Data.DataSource;
public class DeviceDataSource : IDeviceDataSource
{
    public Task<List<Device>> GetAllAsync() => throw new NotImplementedException();
    public Task InsertAsync(Device company) => throw new NotImplementedException();
}
