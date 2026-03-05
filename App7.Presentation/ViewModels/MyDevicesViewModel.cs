using System.Collections.ObjectModel;
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
    [ObservableProperty] private string _searchModelName    = string.Empty;
    [ObservableProperty] private string _searchIMEI         = string.Empty;
    [ObservableProperty] private string _searchSerialLab    = string.Empty;
    [ObservableProperty] private string _searchSerialNumber = string.Empty;
    [ObservableProperty] private string _searchCircuitSerial= string.Empty;
    [ObservableProperty] private string _searchHWVersion    = string.Empty;

    // ── Column visibility ─────────────────────────────────────────────
    public override ObservableCollection<ColumnVisibilityItem> ColumnVisibilities { get; } = new()
    {
        new() { ColumnTag = "ModelName",           DisplayName = "Model Name",     IsVisible = true },
        new() { ColumnTag = "IMEI",                DisplayName = "IMEI",           IsVisible = true },
        new() { ColumnTag = "SerialLab",           DisplayName = "Serial Lab",     IsVisible = true },
        new() { ColumnTag = "SerialNumber",        DisplayName = "Serial No.",     IsVisible = true },
        new() { ColumnTag = "CircuitSerialNumber", DisplayName = "Circuit Serial", IsVisible = true },
        new() { ColumnTag = "HWVersion",           DisplayName = "HW Version",     IsVisible = true },
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
