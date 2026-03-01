using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Domain.Usecases;
public class GetGridDataUseCase
{

    private readonly ISampleOrderRepository _sampleRepository;

    public GetGridDataUseCase(ISampleOrderRepository sampleRepository)
    {
        _sampleRepository = sampleRepository;
    }

    public async Task<IEnumerable<SampleOrder>> ExecuteAsync()
    {
        return await _sampleRepository.GetAllAsync();
    }
}
