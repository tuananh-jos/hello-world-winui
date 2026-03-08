using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App7.Domain.Entities;

namespace App7.Domain.IRepository;

public interface IRepositoryBase<TEntity> where TEntity : class, IEntity
{
    
}
