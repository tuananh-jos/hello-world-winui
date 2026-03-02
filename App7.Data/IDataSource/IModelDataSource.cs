using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Domain.Entities;

namespace App7.Data.IDataSource;
public interface IModelDataSource
{
    Task<List<Model>> GetAllAsync();
    Task InsertAsync(Model order);

}
