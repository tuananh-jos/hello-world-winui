using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Domain.Usecases;
public class GetModelsUseCase
{

    private readonly IModelRepository _modelRepository;

    public GetModelsUseCase(IModelRepository modelRepository)
    {
        _modelRepository = modelRepository;
    }

    public async Task<IEnumerable<Model>> ExecuteAsync()
    {
        return await _modelRepository.GetAllAsync();
    }
}
