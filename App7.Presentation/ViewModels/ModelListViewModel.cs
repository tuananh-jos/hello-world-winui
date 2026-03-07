using System.Collections.ObjectModel;
using App7.Domain.Entities;
using App7.Domain.Services;
using App7.Domain.Usecases;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace App7.Presentation.ViewModels;

public partial class ModelListViewModel : PagedListViewModelBase
{

    public ObservableCollection<Model> Models { get; } = new();

    // ── Filters ───────────────────────────────────────────────────────
    [ObservableProperty] private string  _searchName          = string.Empty;
    [ObservableProperty] private string? _selectedManufacturer;
    [ObservableProperty] private string? _selectedCategory;
    [ObservableProperty] private string? _selectedSubCategory;
    public ObservableCollection<string> Manufacturers { get; } = new();
    public ObservableCollection<string> Categories    { get; } = new();
    public ObservableCollection<string> SubCategories { get; } = new();

    // ── Column visibility ─────────────────────────────────────────────
    public override ObservableCollection<ColumnVisibilityItem> ColumnVisibilities { get; } = new()
    {
        new() { ColumnTag = "Name",         DisplayName = "Name",         IsVisible = true },
        new() { ColumnTag = "Manufacturer", DisplayName = "Manufacturer", IsVisible = true },
        new() { ColumnTag = "Category",     DisplayName = "Category",     IsVisible = true },
        new() { ColumnTag = "SubCategory",  DisplayName = "Sub Category", IsVisible = true },
        new() { ColumnTag = "Available",    DisplayName = "Available",    IsVisible = true },
    };

    private readonly GetModelsPagedUseCase  _getModelsPaged;
    private readonly GetModelFiltersUseCase _getModelFilters;
    private readonly IInstanceSyncService   _syncService;
    private readonly DispatcherQueue        _dispatcher;

    public ModelListViewModel(
        GetModelsPagedUseCase  getModelsPaged,
        GetModelFiltersUseCase getModelFilters,
        IInstanceSyncService   syncService)
    {
        _getModelsPaged  = getModelsPaged;
        _getModelFilters = getModelFilters;
        _syncService     = syncService;
        _dispatcher      = DispatcherQueue.GetForCurrentThread();

        _syncService.DataChanged += OnExternalDataChanged;
    }

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override async Task LoadDataCoreAsync()
    {
        var (items, total) = await _getModelsPaged.ExecuteAsync(
            page:              CurrentPage,
            pageSize:          SelectedPageSize,
            searchName:        NullIfEmpty(SearchName),
            searchManufacturer: SelectedManufacturer,   // exact match from hardcoded list
            filterCategory:    SelectedCategory,
            filterSubCategory: SelectedSubCategory,
            sortColumn:        SortColumn,
            ascending:         SortAscending);

        Models.Clear();
        foreach (var m in items) Models.Add(m);
        TotalCount = total;
    }

    protected override async Task LoadFilterOptionsAsync()
    {
        var mfrs = await _getModelFilters.GetManufacturersAsync();
        Manufacturers.Clear();
        foreach (var m in mfrs) Manufacturers.Add(m);

        var cats = await _getModelFilters.GetCategoriesAsync();
        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);

        var subs = await _getModelFilters.GetSubCategoriesAsync();
        SubCategories.Clear();
        foreach (var s in subs) SubCategories.Add(s);
    }

    protected override void ClearFilterValues()
    {
        SearchName          = string.Empty;
        SelectedManufacturer = null;
        SelectedCategory    = null;
        SelectedSubCategory = null;
    }

    private static string? NullIfEmpty(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s;

    // ── External sync ─────────────────────────────────────────────────
    private void OnExternalDataChanged()
    {
        // Raised on ThreadPool — must dispatch to UI thread
        _dispatcher.TryEnqueue(async () => await ReloadAsync());
    }
}
