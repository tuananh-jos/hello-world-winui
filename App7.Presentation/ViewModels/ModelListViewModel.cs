using System.Collections.ObjectModel;
using App7.Domain.Constants;
using App7.Domain.Entities;
using App7.Domain.Services;
using App7.Domain.Usecases;
using App7.Domain.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace App7.Presentation.ViewModels;

public partial class ModelListViewModel : PagedListViewModelBase<Model>
{

    // ── Filters ───────────────────────────────────────────────────────
    [ObservableProperty] private string  _searchName          = string.Empty;

    partial void OnSearchNameChanged(string value) => DebounceSearch();

    [ObservableProperty] private string? _selectedManufacturer;
    [ObservableProperty] private string  _searchManufacturer  = string.Empty;
    [ObservableProperty] private string? _selectedCategory;
    [ObservableProperty] private string  _searchCategory      = string.Empty;
    [ObservableProperty] private string? _selectedSubCategory;
    [ObservableProperty] private string  _searchSubCategory   = string.Empty;
    public ObservableCollection<string> Manufacturers { get; } = new();
    public ObservableCollection<string> Categories    { get; } = new();
    public ObservableCollection<string> SubCategories { get; } = new();

    // ── Column visibility ─────────────────────────────────────────────
    public override ObservableCollection<ColumnVisibilityItem> ColumnVisibilities { get; } = new()
    {
        new() { ColumnTag = ColumnTags.NAME,         DisplayName = ColumnTags.NAME_LABEL,         IsVisible = true },
        new() { ColumnTag = ColumnTags.MANUFACTURER, DisplayName = ColumnTags.MANUFACTURER_LABEL, IsVisible = true },
        new() { ColumnTag = ColumnTags.CATEGORY,     DisplayName = ColumnTags.CATEGORY_LABEL,     IsVisible = true },
        new() { ColumnTag = ColumnTags.SUB_CATEGORY, DisplayName = ColumnTags.SUB_CATEGORY_LABEL, IsVisible = true },
        new() { ColumnTag = ColumnTags.AVAILABLE,    DisplayName = ColumnTags.AVAILABLE_LABEL,    IsVisible = true },
    };

    private readonly GetModelsPagedUseCase  _getModelsPaged;
    private readonly GetModelFiltersUseCase _getModelFilters;

    public ModelListViewModel(
        GetModelsPagedUseCase  getModelsPaged,
        GetModelFiltersUseCase getModelFilters,
        IInstanceSyncService   syncService) : base(syncService)
    {
        _getModelsPaged  = getModelsPaged;
        _getModelFilters = getModelFilters;
    }

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override async Task LoadDataCoreAsync()
    {
        var request = new GetModelsPagedRequest(
            Page:              CurrentPage,
            PageSize:          SelectedPageSize,
            SearchName:        NullIfEmpty(SearchName),
            SearchManufacturer: SelectedManufacturer,
            FilterCategory:    SelectedCategory,
            FilterSubCategory: SelectedSubCategory,
            SortColumn:        SortColumn,
            Ascending:         SortAscending);

        var (items, total) = await _getModelsPaged.ExecuteAsync(request);

        Items.Clear();
        foreach (var m in items) Items.Add(m);
        TotalCount = total;
    }

    protected override async Task LoadFilterOptionsAsync()
    {
        var filters = await _getModelFilters.ExecuteAsync();

        Manufacturers.Clear();
        foreach (var m in filters.Manufacturers) Manufacturers.Add(m);

        Categories.Clear();
        foreach (var c in filters.Categories) Categories.Add(c);

        SubCategories.Clear();
        foreach (var s in filters.SubCategories) SubCategories.Add(s);
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
}
