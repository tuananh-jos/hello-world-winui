using System.Collections.ObjectModel;
using App7.Domain.Entities;
using App7.Domain.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace App7.Presentation.ViewModels;

public partial class MyDevicesViewModel : PagedListViewModelBase
{
    public ObservableCollection<Device> Devices { get; } = new();

    // ── Per-column search ─────────────────────────────────────────────
    [ObservableProperty] private string _searchModelName     = string.Empty;
    [ObservableProperty] private string _searchIMEI          = string.Empty;
    [ObservableProperty] private string _searchSerialLab     = string.Empty;
    [ObservableProperty] private string _searchSerialNumber  = string.Empty;
    [ObservableProperty] private string _searchCircuitSerial = string.Empty;
    [ObservableProperty] private string _searchHWVersion     = string.Empty;

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

    private readonly IInMemoryStore      _store;
    private readonly IInstanceSyncService _syncService;
    private readonly DispatcherQueue     _dispatcher;

    public MyDevicesViewModel(
        IInMemoryStore       store,
        IInstanceSyncService syncService)
    {
        _store       = store;
        _syncService = syncService;
        _dispatcher  = DispatcherQueue.GetForCurrentThread();

        _store.StoreChanged      += OnStoreChanged;
        _syncService.EventReceived += OnEventReceived;
    }

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override Task LoadDataCoreAsync()
    {
        ApplyInMemoryFilter();
        return Task.CompletedTask;
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

    // ── In-memory filter + pagination ─────────────────────────────────
    private void ApplyInMemoryFilter()
    {
        var all = _store.GetAllDevices()
            .Where(d => d.Status == "Borrowed")
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchModelName))
            all = all.Where(d => d.ModelName.Contains(SearchModelName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SearchIMEI))
            all = all.Where(d => d.IMEI.Contains(SearchIMEI, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SearchSerialLab))
            all = all.Where(d => d.SerialLab.Contains(SearchSerialLab, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SearchSerialNumber))
            all = all.Where(d => d.SerialNumber.Contains(SearchSerialNumber, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SearchCircuitSerial))
            all = all.Where(d => d.CircuitSerialNumber.Contains(SearchCircuitSerial, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SearchHWVersion))
            all = all.Where(d => d.HWVersion.Contains(SearchHWVersion, StringComparison.OrdinalIgnoreCase));

        // Sort
        all = (SortColumn?.ToLowerInvariant()) switch
        {
            "modelname"           => SortAscending ? all.OrderBy(d => d.ModelName)               : all.OrderByDescending(d => d.ModelName),
            "imei"                => SortAscending ? all.OrderBy(d => d.IMEI)                    : all.OrderByDescending(d => d.IMEI),
            "seriallab"           => SortAscending ? all.OrderBy(d => d.SerialLab)               : all.OrderByDescending(d => d.SerialLab),
            "serialnumber"        => SortAscending ? all.OrderBy(d => d.SerialNumber)            : all.OrderByDescending(d => d.SerialNumber),
            "circuitserialnumber" => SortAscending ? all.OrderBy(d => d.CircuitSerialNumber)     : all.OrderByDescending(d => d.CircuitSerialNumber),
            "hwversion"           => SortAscending ? all.OrderBy(d => d.HWVersion)               : all.OrderByDescending(d => d.HWVersion),
            _                     => SortAscending ? all.OrderBy(d => d.ModelName)               : all.OrderByDescending(d => d.ModelName),
        };

        var filtered = all.ToList();
        TotalCount = filtered.Count;

        var page = filtered
            .Skip((CurrentPage - 1) * SelectedPageSize)
            .Take(SelectedPageSize)
            .ToList();

        Devices.Clear();
        foreach (var d in page) Devices.Add(d);
    }

    // ── Store / sync event handlers ───────────────────────────────────

    private void OnStoreChanged()
    {
        _dispatcher.TryEnqueue(ApplyInMemoryFilter);
    }

    private void OnEventReceived(Domain.Services.SyncEvent evt)
    {
        _store.ApplyEvent(evt);
        _dispatcher.TryEnqueue(ApplyInMemoryFilter);
    }
}
