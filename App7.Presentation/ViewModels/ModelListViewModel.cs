using System.Collections.ObjectModel;
using App7.Domain.Entities;
using App7.Domain.Usecases;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App7.Presentation.ViewModels;

public partial class ModelListViewModel : PagedListViewModelBase
{
    private readonly GetModelsPagedUseCase  _getModelsPaged;
    private readonly GetModelFiltersUseCase _getModelFilters;

    // ── Data ──────────────────────────────────────────────────────────
    public ObservableCollection<Model> Models { get; } = new();

    // ── Filters ───────────────────────────────────────────────────────
    [ObservableProperty] private string  _searchText        = string.Empty;
    [ObservableProperty] private string? _selectedCategory;
    [ObservableProperty] private string? _selectedSubCategory;

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

    public ModelListViewModel(
        GetModelsPagedUseCase  getModelsPaged,
        GetModelFiltersUseCase getModelFilters)
    {
        _getModelsPaged  = getModelsPaged;
        _getModelFilters = getModelFilters;
    }

    // reload subcategories when category changes
    partial void OnSelectedCategoryChanged(string? value)
    {
        SelectedSubCategory = null;
        SubCategories.Clear();
        if (!string.IsNullOrEmpty(value))
            _ = LoadSubCategoriesAsync(value);
    }

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override async Task LoadDataCoreAsync()
    {
        var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

        var (items, total) = await _getModelsPaged.ExecuteAsync(
            page:              CurrentPage,
            pageSize:          SelectedPageSize,
            searchName:        search,
            searchManufacturer: search,
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
        var cats = await _getModelFilters.GetCategoriesAsync();
        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);
    }

    protected override void ClearFilterValues()
    {
        SearchText          = string.Empty;
        SelectedCategory    = null;
        SelectedSubCategory = null;
        SubCategories.Clear();
    }

    // ── Private ───────────────────────────────────────────────────────
    private async Task LoadSubCategoriesAsync(string category)
    {
        var subs = await _getModelFilters.GetSubCategoriesAsync(category);
        SubCategories.Clear();
        foreach (var s in subs) SubCategories.Add(s);
    }
}
