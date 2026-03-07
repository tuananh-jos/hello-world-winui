using System.Collections.ObjectModel;
using App7.Domain.Constants;
using App7.Domain.Entities;
using App7.Domain.Services;
using App7.Domain.Usecases;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace App7.Presentation.ViewModels;

public partial class MyDevicesViewModel : PagedListViewModelBase
{

    public ObservableCollection<Device> Devices { get; } = new();

    // ── Per-column search ─────────────────────────────────────────────
    [ObservableProperty] private string _searchName         = string.Empty;
    [ObservableProperty] private string _searchModelName    = string.Empty;
    [ObservableProperty] private string _searchIMEI         = string.Empty;
    [ObservableProperty] private string _searchSerialLab    = string.Empty;
    [ObservableProperty] private string _searchSerialNumber = string.Empty;
    [ObservableProperty] private string _searchCircuitSerial= string.Empty;
    [ObservableProperty] private string _searchHWVersion    = string.Empty;

    partial void OnSearchNameChanged(string value)         => DebounceSearch();
    partial void OnSearchModelNameChanged(string value)    => DebounceSearch();
    partial void OnSearchIMEIChanged(string value)         => DebounceSearch();
    partial void OnSearchSerialLabChanged(string value)    => DebounceSearch();
    partial void OnSearchSerialNumberChanged(string value) => DebounceSearch();
    partial void OnSearchCircuitSerialChanged(string value)=> DebounceSearch();
    partial void OnSearchHWVersionChanged(string value)    => DebounceSearch();

    // ── Column visibility ─────────────────────────────────────────────
    public override ObservableCollection<ColumnVisibilityItem> ColumnVisibilities { get; } = new()
    {
        new() { ColumnTag = ColumnTags.NAME,                  DisplayName = ColumnTags.NAME_LABEL,                  IsVisible = true },
        new() { ColumnTag = ColumnTags.MODEL_NAME,            DisplayName = ColumnTags.MODEL_NAME_LABEL,            IsVisible = true },
        new() { ColumnTag = ColumnTags.IMEI,                  DisplayName = ColumnTags.IMEI_LABEL,                  IsVisible = true },
        new() { ColumnTag = ColumnTags.SERIAL_LAB,            DisplayName = ColumnTags.SERIAL_LAB_LABEL,            IsVisible = true },
        new() { ColumnTag = ColumnTags.SERIAL_NUMBER,         DisplayName = ColumnTags.SERIAL_NUMBER_LABEL,         IsVisible = true },
        new() { ColumnTag = ColumnTags.CIRCUIT_SERIAL_NUMBER, DisplayName = ColumnTags.CIRCUIT_SERIAL_NUMBER_LABEL, IsVisible = true },
        new() { ColumnTag = ColumnTags.HW_VERSION,            DisplayName = ColumnTags.HW_VERSION_LABEL,            IsVisible = true },
    };

    private readonly GetBorrowedDevicesUseCase _getDevices;
    private readonly IInstanceSyncService       _syncService;
    private readonly DispatcherQueue            _dispatcher;

    public MyDevicesViewModel(
        GetBorrowedDevicesUseCase getDevices,
        IInstanceSyncService      syncService)
    {
        _getDevices  = getDevices;
        _syncService = syncService;
        _dispatcher  = DispatcherQueue.GetForCurrentThread();

        _syncService.DataChanged += OnExternalDataChanged;
    }

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override async Task LoadDataCoreAsync()
    {
        var (items, total) = await _getDevices.ExecuteAsync(
            page:               CurrentPage,
            pageSize:           SelectedPageSize,
            searchName:         NullIfEmpty(SearchName),
            searchModelName:    NullIfEmpty(SearchModelName),
            searchIMEI:         NullIfEmpty(SearchIMEI),
            searchSerialLab:    NullIfEmpty(SearchSerialLab),
            searchSerialNumber: NullIfEmpty(SearchSerialNumber),
            searchCircuitSerial:NullIfEmpty(SearchCircuitSerial),
            searchHWVersion:    NullIfEmpty(SearchHWVersion),
            sortColumn:         SortColumn,
            ascending:          SortAscending);

        Devices.Clear();
        foreach (var d in items) Devices.Add(d);
        TotalCount = total;
    }

    protected override Task LoadFilterOptionsAsync() => Task.CompletedTask;

    protected override void ClearFilterValues()
    {
        SearchName          = string.Empty;
        SearchModelName     = string.Empty;
        SearchIMEI          = string.Empty;
        SearchSerialLab     = string.Empty;
        SearchSerialNumber  = string.Empty;
        SearchCircuitSerial = string.Empty;
        SearchHWVersion     = string.Empty;
    }

    private static string? NullIfEmpty(string s)
        => string.IsNullOrWhiteSpace(s) ? null : s;

    // ── External sync ─────────────────────────────────────────────────
    private void OnExternalDataChanged()
    {
        _dispatcher.TryEnqueue(async () => await ReloadAsync());
    }
}
