using System.Collections.ObjectModel;
using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App7.Presentation.ViewModels;

public partial class MyDevicesViewModel : ObservableRecipient, INavigationAware
{
    private readonly GetBorrowedDevicesUseCase _getDevices;

    // ── Data ──────────────────────────────────────────────────────────
    public ObservableCollection<Device> Devices { get; } = new();

    // ── Search & Filter ───────────────────────────────────────────────
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string? _selectedHWVersion;

    public ObservableCollection<string> HWVersions { get; } = new();

    // ── Sort ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _sortColumn = "Name";
    [ObservableProperty] private bool _sortAscending = true;

    // ── Pagination ────────────────────────────────────────────────────
    private const int PageSize = 50;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
    [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    [NotifyPropertyChangedFor(nameof(TotalCountText))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
    private int _totalCount;

    public int TotalPages     => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage     => CurrentPage < TotalPages;
    public string TotalCountText => $"{TotalCount:N0} devices";

    // ── Loading ───────────────────────────────────────────────────────
    [ObservableProperty] private bool _isLoading;

    public MyDevicesViewModel(GetBorrowedDevicesUseCase getDevices)
    {
        _getDevices = getDevices;
    }

    // ── INavigationAware ──────────────────────────────────────────────
    public async void OnNavigatedTo(object parameter)
    {
        await LoadHWVersionsAsync();
        await LoadDevicesAsync();
    }

    public void OnNavigatedFrom() { }

    // ── Commands ──────────────────────────────────────────────────────

    [RelayCommand]
    public async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadDevicesAsync();
    }

    [RelayCommand]
    public async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedHWVersion = null;
        SortColumn = "Name";
        SortAscending = true;
        CurrentPage = 1;
        await LoadDevicesAsync();
    }

    [RelayCommand(CanExecute = nameof(HasPreviousPage))]
    private async Task PreviousPageAsync()
    {
        if (!HasPreviousPage) return;
        CurrentPage--;
        await LoadDevicesAsync();
    }

    [RelayCommand(CanExecute = nameof(HasNextPage))]
    private async Task NextPageAsync()
    {
        if (!HasNextPage) return;
        CurrentPage++;
        await LoadDevicesAsync();
    }

    [RelayCommand]
    public async Task SortByAsync(string column)
    {
        if (SortColumn == column)
            SortAscending = !SortAscending;
        else
        {
            SortColumn = column;
            SortAscending = true;
        }
        CurrentPage = 1;
        await LoadDevicesAsync();
    }

    // ── Public reload (called from ReturnDialog after confirm) ────────
    public async Task ReloadAsync() => await LoadDevicesAsync();

    // ── Private helpers ────────────────────────────────────────────────

    private async Task LoadDevicesAsync()
    {
        IsLoading = true;
        try
        {
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

            var (items, total) = await _getDevices.ExecuteAsync(
                page: CurrentPage,
                pageSize: PageSize,
                searchText: search,
                filterHWVersion: SelectedHWVersion,
                sortColumn: SortColumn,
                ascending: SortAscending);

            Devices.Clear();
            foreach (var d in items)
                Devices.Add(d);

            TotalCount = total;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadHWVersionsAsync()
    {
        var versions = await _getDevices.GetHWVersionsAsync();
        HWVersions.Clear();
        foreach (var v in versions)
            HWVersions.Add(v);
    }
}
