using App7.Domain.IRepository;
using App7.Domain.Dtos;

namespace App7.Domain.Usecases;

public class GetModelFiltersUseCase : IUseCaseWithOutput<ModelFiltersResponse>
{
    private readonly IModelRepository _modelRepository;

    public GetModelFiltersUseCase(IModelRepository modelRepository) => _modelRepository = modelRepository;

    public async Task<ModelFiltersResponse> ExecuteAsync()
    {
        var mfrsTask = _modelRepository.GetManufacturersAsync();
        var catsTask = _modelRepository.GetCategoriesAsync();
        var subsTask = _modelRepository.GetSubCategoriesAsync();

        await Task.WhenAll(mfrsTask, catsTask, subsTask);

        return new ModelFiltersResponse(
            await mfrsTask,
            await catsTask,
            await subsTask
        );
    }
}
