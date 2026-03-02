using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Domain.Entities;

namespace App7.Domain.IRepository;
public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetAllAsync();
    Task AddAsync(Device company);
}
