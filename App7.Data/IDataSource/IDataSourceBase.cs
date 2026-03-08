using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App7.Domain.Entities;

namespace App7.Data.IDataSource;

public interface IDataSourceBase<TBaseEntity> where TBaseEntity : class, IEntity
{

}
