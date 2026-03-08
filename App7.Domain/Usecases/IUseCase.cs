using System.Threading.Tasks;

namespace App7.Domain.Usecases;

/// <summary>
/// Base interface for UseCases with an input and output.
/// </summary>
public interface IUseCase<in TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input);
}

/// <summary>
/// Base interface for UseCases that do not return a value.
/// </summary>
public interface IUseCase<in TInput>
{
    Task ExecuteAsync(TInput input);
}

/// <summary>
/// Base interface for UseCases that do not require input.
/// </summary>
public interface IUseCaseWithOutput<TOutput>
{
    Task<TOutput> ExecuteAsync();
}
