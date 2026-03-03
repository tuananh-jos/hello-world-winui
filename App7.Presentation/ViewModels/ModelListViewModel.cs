using System.Collections.ObjectModel;
using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.Contracts.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace App7.Presentation.ViewModels;

public partial class ModelListViewModel : ObservableRecipient, INavigationAware
{
    private readonly GetModelsPagedUseCase _getModelsPaged;
    private readonly GetModelFiltersUseCase _getModelFilters;

    // ── Data ──────────────────────────────────────────────────────────────
    public ObservableCollection<Model> Models { get; } = new();

    // ── Search & Filter ───────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyFiltersCommand))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private string? _selectedSubCategory;

    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<string> SubCategories { get; } = new();

    // ── Sort ──────────────────────────────────────────────────────────────
    [ObservableProperty] private string _sortColumn = "Name";
    [ObservableProperty] private bool _sortAscending = true;

    // ── Pagination ────────────────────────────────────────────────────────
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

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public string TotalCountText => $"{TotalCount:N0} models";

    // ── Loading state ─────────────────────────────────────────────────────
    [ObservableProperty] private bool _isLoading;

    public ModelListViewModel(
        GetModelsPagedUseCase getModelsPaged,
        GetModelFiltersUseCase getModelFilters)
    {
        _getModelsPaged = getModelsPaged;
        _getModelFilters = getModelFilters;
    }

    // ── INavigationAware ──────────────────────────────────────────────────
    public async void OnNavigatedTo(object parameter)
    {
        await LoadCategoriesAsync();
        await LoadModelsAsync();
    }

    public void OnNavigatedFrom() { }

    // ── Commands ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadModelsAsync();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedCategory = null;
        SelectedSubCategory = null;
        SubCategories.Clear();
        SortColumn = "Name";
        SortAscending = true;
        CurrentPage = 1;
        await LoadModelsAsync();
    }

    [RelayCommand(CanExecute = nameof(HasPreviousPage))]
    private async Task PreviousPageAsync()
    {
        if (!HasPreviousPage) return;
        CurrentPage--;
        await LoadModelsAsync();
    }

    [RelayCommand(CanExecute = nameof(HasNextPage))]
    private async Task NextPageAsync()
    {
        if (!HasNextPage) return;
        CurrentPage++;
        await LoadModelsAsync();
    }

    [RelayCommand]
    private async Task SortByAsync(string column)
    {
        if (SortColumn == column)
            SortAscending = !SortAscending;
        else
        {
            SortColumn = column;
            SortAscending = true;
        }
        CurrentPage = 1;
        await LoadModelsAsync();
    }

    // ── Partial property callbacks ─────────────────────────────────────────

    partial void OnSelectedCategoryChanged(string? value)
    {
        SelectedSubCategory = null;
        SubCategories.Clear();
        if (!string.IsNullOrEmpty(value))
            _ = LoadSubCategoriesAsync(value);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private async Task LoadModelsAsync()
    {
        IsLoading = true;
        try
        {
            var (items, total) = await _getModelsPaged.ExecuteAsync(
                page: CurrentPage,
                pageSize: PageSize,
                searchName: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                searchManufacturer: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                filterCategory: SelectedCategory,
                filterSubCategory: SelectedSubCategory,
                sortColumn: SortColumn,
                ascending: SortAscending);

            Models.Clear();
            foreach (var m in items)
                Models.Add(m);

            TotalCount = total;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _getModelFilters.GetCategoriesAsync();
        Categories.Clear();
        foreach (var c in cats)
            Categories.Add(c);
    }

    private async Task LoadSubCategoriesAsync(string category)
    {
        var subs = await _getModelFilters.GetSubCategoriesAsync(category);
        SubCategories.Clear();
        foreach (var s in subs)
            SubCategories.Add(s);
    }
}
