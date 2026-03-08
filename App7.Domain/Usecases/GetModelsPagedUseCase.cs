using App7.Domain.Entities;
using App7.Domain.IRepository;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class GetModelsPagedUseCase : IUseCase<GetModelsPagedRequest, (IEnumerable<Model> Items, int TotalCount)>
{
    private readonly IModelRepository _modelRepository;

    public GetModelsPagedUseCase(IModelRepository modelRepository) => _modelRepository = modelRepository;

    public async Task<(IEnumerable<Model> Items, int TotalCount)> ExecuteAsync(GetModelsPagedRequest request)
    {
        await Task.Delay(100);
        return await _modelRepository.GetPagedAsync(request);
    }
}
