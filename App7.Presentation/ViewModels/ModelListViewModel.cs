using System.Collections.ObjectModel;
using App7.Domain.Entities;
using App7.Domain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace App7.Presentation.ViewModels;

public partial class ModelListViewModel : PagedListViewModelBase
{
    public ObservableCollection<Model> Models { get; } = new();

    // ── Filters ───────────────────────────────────────────────────────
    [ObservableProperty] private string  _searchName           = string.Empty;
    [ObservableProperty] private string? _selectedManufacturer;
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

    private readonly IInMemoryStore  _store;
    private readonly IInstanceSyncService _syncService;
    private readonly DispatcherQueue _dispatcher;

    public ModelListViewModel(
        IInMemoryStore       store,
        IInstanceSyncService syncService)
    {
        _store       = store;
        _syncService = syncService;
        _dispatcher  = DispatcherQueue.GetForCurrentThread();

        // Subscribe to store changes (fired after borrow/return or external sync event)
        _store.StoreChanged += OnStoreChanged;

        // Subscribe to sync events from other instances
        _syncService.EventReceived += OnEventReceived;
    }

    // reload subcategories in-memory when category changes
    partial void OnSelectedCategoryChanged(string? value)
    {
        SelectedSubCategory = null;
        SubCategories.Clear();
        if (!string.IsNullOrEmpty(value))
            LoadSubCategories(value);
    }

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override Task LoadDataCoreAsync()
    {
        ApplyInMemoryFilter();
        return Task.CompletedTask;
    }

    protected override Task LoadFilterOptionsAsync()
    {
        // Build category list from in-memory store
        var cats = _store.GetAllModels()
            .Select(m => m.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);

        return Task.CompletedTask;
    }

    protected override void ClearFilterValues()
    {
        SearchName           = string.Empty;
        SelectedManufacturer = null;
        SelectedCategory     = null;
        SelectedSubCategory  = null;
        SubCategories.Clear();
    }

    // ── In-memory filter + pagination ─────────────────────────────────
    private void ApplyInMemoryFilter()
    {
        var all = _store.GetAllModels().AsEnumerable();

        // Search
        if (!string.IsNullOrWhiteSpace(SearchName))
            all = all.Where(m => m.Name.Contains(SearchName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SelectedManufacturer))
            all = all.Where(m => m.Manufacturer == SelectedManufacturer);

        // Filter
        if (!string.IsNullOrWhiteSpace(SelectedCategory))
            all = all.Where(m => m.Category == SelectedCategory);

        if (!string.IsNullOrWhiteSpace(SelectedSubCategory))
            all = all.Where(m => m.SubCategory == SelectedSubCategory);

        // Sort
        all = (SortColumn?.ToLowerInvariant()) switch
        {
            "manufacturer" => SortAscending ? all.OrderBy(m => m.Manufacturer) : all.OrderByDescending(m => m.Manufacturer),
            "category"     => SortAscending ? all.OrderBy(m => m.Category)     : all.OrderByDescending(m => m.Category),
            "subcategory"  => SortAscending ? all.OrderBy(m => m.SubCategory)  : all.OrderByDescending(m => m.SubCategory),
            "available"    => SortAscending ? all.OrderBy(m => m.Available)    : all.OrderByDescending(m => m.Available),
            _              => SortAscending ? all.OrderBy(m => m.Name)         : all.OrderByDescending(m => m.Name),
        };

        var filtered = all.ToList();
        TotalCount = filtered.Count;

        // Client-side pagination
        var page = filtered
            .Skip((CurrentPage - 1) * SelectedPageSize)
            .Take(SelectedPageSize)
            .ToList();

        Models.Clear();
        foreach (var m in page) Models.Add(m);
    }

    private void LoadSubCategories(string category)
    {
        var subs = _store.GetAllModels()
            .Where(m => m.Category == category)
            .Select(m => m.SubCategory)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        SubCategories.Clear();
        foreach (var s in subs) SubCategories.Add(s);
    }

    // ── Store / sync event handlers ───────────────────────────────────

    private void OnStoreChanged()
    {
        // Called after local borrow/return — store already updated, just re-filter
        _dispatcher.TryEnqueue(() =>
        {
            LoadFilterOptionsAsync();
            ApplyInMemoryFilter();
        });
    }

    private void OnEventReceived(Domain.Services.SyncEvent evt)
    {
        // Called on ThreadPool from FileWatcher — apply event to store, then re-filter
        _store.ApplyEvent(evt);
        _dispatcher.TryEnqueue(() =>
        {
            LoadFilterOptionsAsync();
            ApplyInMemoryFilter();
        });
    }
}
