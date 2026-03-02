using System.Collections.ObjectModel;

using App7.Presentation.Contracts.ViewModels;
using App7.Domain.Usecases;
using App7.Domain.Entities;

using CommunityToolkit.Mvvm.ComponentModel;

namespace App7.Presentation.ViewModels;

public partial class DataGridViewModel : ObservableRecipient, INavigationAware
{
    //private readonly ISampleDataService _sampleDataService;
    private readonly GetModelsUseCase _useCase;

    public ObservableCollection<Model> Source { get; } = new ObservableCollection<Model>();

    public DataGridViewModel(GetModelsUseCase useCase)
    {
        _useCase = useCase;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        //var data = await _sampleDataService.GetGridDataAsync();
        var data = await _useCase.ExecuteAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
