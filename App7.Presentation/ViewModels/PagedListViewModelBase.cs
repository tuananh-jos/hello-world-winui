using System.Collections.ObjectModel;
using App7.Presentation.Contracts.ViewModels;
using App7.Domain.Services;
using App7.Domain.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace App7.Presentation.ViewModels;

/// <summary>
/// Shared base for list pages: pagination, page-size, overlay loading,
/// sort, column-popup toggle. Subclass implements entity-specific loading.
/// </summary>
public abstract partial class PagedListViewModelBase : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcher;

    protected PagedListViewModelBase(IInstanceSyncService syncService)
    {
        _dispatcher = DispatcherQueue.GetForCurrentThread();
        syncService.DataChanged += OnExternalDataChanged;
    }

    private void OnExternalDataChanged()
    {
        _dispatcher.TryEnqueue(async () => await ReloadAsync());
    }
    // ── Page size ─────────────────────────────────────────────────────
    public IReadOnlyList<int> PageSizeOptions { get; } = new[] { 10, 25, 50, 100 };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowingText), nameof(PageNumbers), nameof(TotalPages))]
    private int _selectedPageSize = 10;

    partial void OnSelectedPageSizeChanged(int value)
        => _ = OverlayLoadAsync(resetPage: true);

    // ── Loading ───────────────────────────────────────────────────────
    /// <summary>True only during OnNavigatedTo first load → drives the ProgressBar.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InitialLoadingVisibility))]
    private bool _isInitialLoading;

    /// <summary>True for every subsequent async op → drives the dimmed overlay.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverlayVisibility))]
    private bool _isOverlayVisible;

    public Visibility InitialLoadingVisibility
        => IsInitialLoading ? Visibility.Visible : Visibility.Collapsed;

    public Visibility OverlayVisibility
        => IsOverlayVisible ? Visibility.Visible : Visibility.Collapsed;

    // ── Pagination ────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPreviousPage), nameof(HasNextPage), nameof(ShowingText), nameof(PageNumbers))]
    [NotifyCanExecuteChangedFor(nameof(FirstPageCommand), nameof(PreviousPageCommand),
                                nameof(NextPageCommand), nameof(LastPageCommand))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPages), nameof(HasNextPage), nameof(ShowingText),
        nameof(PageNumbers), nameof(PaginationVisibility), nameof(ShowingTextVisibility),
        nameof(PageNavVisibility), nameof(EmptyVisibility))]
    [NotifyCanExecuteChangedFor(nameof(NextPageCommand), nameof(LastPageCommand))]
    private int _totalCount;

    public int TotalPages
        => Math.Max(0, (int)Math.Ceiling((double)TotalCount / SelectedPageSize));

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage     => CurrentPage < TotalPages;

    public string ShowingText
    {
        get
        {
            if (TotalCount == 0) return "Showing 0 results";
            var from = (CurrentPage - 1) * SelectedPageSize + 1;
            var to   = Math.Min(CurrentPage * SelectedPageSize, TotalCount);
            return $"Showing {from} to {to} of {TotalCount:N0} entries";
        }
    }

    /// <summary>
    /// Sliding window, max 5 buttons.
    ///   TotalPages = 0   → empty (hide all buttons)
    ///   TotalPages &lt;= 5 → show exactly TotalPages buttons
    ///   TotalPages &gt; 5  → show 5, centred on CurrentPage
    /// </summary>
    public IReadOnlyList<int> PageNumbers
    {
        get
        {
            if (TotalPages <= 0) return Array.Empty<int>();
            var count = Math.Min(TotalPages, 5);
            var start = Math.Max(1, Math.Min(CurrentPage - 2, TotalPages - count + 1));
            return Enumerable.Range(start, count).ToList();
        }
    }

    /// <summary>Always Visible — footer is always shown.</summary>
    public Visibility ShowingTextVisibility
        => Visibility.Visible;

    /// <summary>Shows page nav buttons only when there are multiple pages.</summary>
    public Visibility PageNavVisibility
        => TotalPages > 1 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Shows "No matching records" when TotalCount == 0.</summary>
    public Visibility EmptyVisibility
        => TotalCount == 0 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Legacy: kept for compat.</summary>
    public Visibility PaginationVisibility
        => TotalCount > 0 && TotalPages > 1 ? Visibility.Visible : Visibility.Collapsed;

    // ── Sort ──────────────────────────────────────────────────────────
    [ObservableProperty] private string? _sortColumn;
    [ObservableProperty] private bool   _sortAscending = true;

    // ── Columns popup ─────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColumnsOverlayVisibility))]
    private bool _isColumnsPopupOpen;

    public Visibility ColumnsOverlayVisibility
        => IsColumnsPopupOpen ? Visibility.Visible : Visibility.Collapsed;

    public abstract ObservableCollection<ColumnVisibilityItem> ColumnVisibilities { get; }

    // ── INavigationAware ──────────────────────────────────────────────
    private bool _firstLoad = true;

    public async void OnNavigatedTo(object parameter)
    {
        await LoadFilterOptionsAsync();

        if (_firstLoad)
        {
            _firstLoad = false;
            IsInitialLoading = true;
            try   { await LoadDataCoreAsync(); }
            finally { IsInitialLoading = false; }
        }
        else
        {
            await OverlayLoadAsync(resetPage: false);
        }
    }

    public void OnNavigatedFrom() { }

    // ── Shared commands ───────────────────────────────────────────────

    [RelayCommand]
    public async Task ApplyFiltersAsync() => await OverlayLoadAsync(resetPage: true);

    [RelayCommand]
    public async Task ClearFiltersAsync()
    {
        ClearFilterValues();
        SortColumn = null;
        SortAscending = true;
        await OverlayLoadAsync(resetPage: true);
    }

    [RelayCommand(CanExecute = nameof(HasPreviousPage))]
    private async Task FirstPageAsync()
    {
        CurrentPage = 1;
        await OverlayLoadAsync();
    }

    [RelayCommand(CanExecute = nameof(HasPreviousPage))]
    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage) CurrentPage--;
        await OverlayLoadAsync();
    }

    [RelayCommand]
    public async Task GoToPageAsync(int page)
    {
        CurrentPage = page;
        await OverlayLoadAsync();
    }

    [RelayCommand(CanExecute = nameof(HasNextPage))]
    private async Task NextPageAsync()
    {
        if (HasNextPage) CurrentPage++;
        await OverlayLoadAsync();
    }

    [RelayCommand(CanExecute = nameof(HasNextPage))]
    private async Task LastPageAsync()
    {
        CurrentPage = TotalPages;
        await OverlayLoadAsync();
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
        await OverlayLoadAsync(resetPage: true);
    }

    [RelayCommand]
    private void ToggleColumnsPopup() => IsColumnsPopupOpen = !IsColumnsPopupOpen;

    [RelayCommand]
    public void CloseColumnsPopup() => IsColumnsPopupOpen = false;

    // ── Abstract contract ─────────────────────────────────────────────
    protected abstract Task LoadDataCoreAsync();
    protected abstract Task LoadFilterOptionsAsync();
    protected abstract void ClearFilterValues();

    // ── Internal helper ───────────────────────────────────────────────
    protected async Task OverlayLoadAsync(bool resetPage = false)
    {
        if (resetPage) CurrentPage = 1;
        IsOverlayVisible = true;
        try
        {
            await LoadDataCoreAsync();
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
                await LoadDataCoreAsync();
            }
        }
        finally { IsOverlayVisible = false; }
    }

    private DispatcherTimer? _searchDebounceTimer;

    /// <summary>Call this from property setters to debounce the search by 500ms.</summary>
    protected void DebounceSearch()
    {
        if (_searchDebounceTimer == null)
        {
            _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _searchDebounceTimer.Tick += async (_, _) =>
            {
                _searchDebounceTimer.Stop();
                await ApplyFiltersCommand.ExecuteAsync(null);
            };
        }
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    /// <summary>Called by MyDevicesPage after return — public so code-behind can invoke.</summary>
    public async Task ReloadAsync() => await OverlayLoadAsync();
}

public abstract partial class PagedListViewModelBase<TEntity> : PagedListViewModelBase
{
    public ObservableCollection<TEntity> Items { get; } = new();

    protected PagedListViewModelBase(IInstanceSyncService syncService) : base(syncService)
    {
    }
}
