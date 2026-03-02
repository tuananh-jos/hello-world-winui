using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Domain.Entities;

namespace App7.Domain.IRepository;
public interface IModelRepository
{
    Task<IEnumerable<Model>> GetAllAsync();
    Task AddAsync(Model order);
}
