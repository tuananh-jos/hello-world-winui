using App7.Domain.Entities;
using App7.Domain.IRepository;

namespace App7.Domain.Usecases;

/// <summary>
/// Loads ALL models from DB in chunks of chunkSize (default 100k).
/// Yields each chunk so the caller (App.xaml.cs) can push to IInMemoryStore progressively.
/// Story 1.4 (FR34).
/// </summary>
public class LoadAllModelsUseCase
{
    private readonly IModelRepository _repo;

    public LoadAllModelsUseCase(IModelRepository repo) => _repo = repo;

    public async IAsyncEnumerable<IReadOnlyList<Model>> ExecuteAsync(int chunkSize = 100_000)
    {
        var offset = 0;
        while (true)
        {
            var chunk = await _repo.GetChunkAsync(offset, chunkSize);
            if (chunk.Count == 0) yield break;
            yield return chunk;
            if (chunk.Count < chunkSize) yield break;
            offset += chunkSize;
        }
    }
}
