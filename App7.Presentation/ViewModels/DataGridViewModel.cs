using System.Collections.ObjectModel;

using App7.Presentation.Contracts.ViewModels;
using App7.Domain.Usecases;
using App7.Domain.Entities;

using CommunityToolkit.Mvvm.ComponentModel;

namespace App7.Presentation.ViewModels;

public partial class DataGridViewModel : ObservableRecipient, INavigationAware
{
    //private readonly ISampleDataService _sampleDataService;
    private readonly GetGridDataUseCase _getGridDataUseCase;

    public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

    public DataGridViewModel(GetGridDataUseCase getGridDataUseCase)
    {
        _getGridDataUseCase = getGridDataUseCase;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        //var data = await _sampleDataService.GetGridDataAsync();
        var data = await _getGridDataUseCase.ExecuteAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
