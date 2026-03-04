using System.Collections.ObjectModel;
using App7.Domain.Entities;
using App7.Domain.Usecases;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App7.Presentation.ViewModels;

public partial class MyDevicesViewModel : PagedListViewModelBase
{
    private readonly GetBorrowedDevicesUseCase _getDevices;

    // ── Data ──────────────────────────────────────────────────────────
    public ObservableCollection<Device> Devices { get; } = new();

    // ── Filters ───────────────────────────────────────────────────────
    [ObservableProperty] private string  _searchText        = string.Empty;
    [ObservableProperty] private string? _selectedHWVersion;

    public ObservableCollection<string> HWVersions { get; } = new();

    // ── Column visibility ─────────────────────────────────────────────
    public override ObservableCollection<ColumnVisibilityItem> ColumnVisibilities { get; } = new()
    {
        new() { ColumnTag = "Name",                DisplayName = "Name",           IsVisible = true },
        new() { ColumnTag = "IMEI",                DisplayName = "IMEI",           IsVisible = true },
        new() { ColumnTag = "SerialLab",           DisplayName = "Serial Lab",     IsVisible = true },
        new() { ColumnTag = "SerialNumber",        DisplayName = "Serial No.",     IsVisible = true },
        new() { ColumnTag = "CircuitSerialNumber", DisplayName = "Circuit Serial", IsVisible = true },
        new() { ColumnTag = "HWVersion",           DisplayName = "HW Version",     IsVisible = true },
    };

    public MyDevicesViewModel(GetBorrowedDevicesUseCase getDevices)
        => _getDevices = getDevices;

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override async Task LoadDataCoreAsync()
    {
        var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

        var (items, total) = await _getDevices.ExecuteAsync(
            page:            CurrentPage,
            pageSize:        SelectedPageSize,
            searchText:      search,
            filterHWVersion: SelectedHWVersion,
            sortColumn:      SortColumn,
            ascending:       SortAscending);

        Devices.Clear();
        foreach (var d in items) Devices.Add(d);
        TotalCount = total;
    }

    protected override async Task LoadFilterOptionsAsync()
    {
        var versions = await _getDevices.GetHWVersionsAsync();
        HWVersions.Clear();
        foreach (var v in versions) HWVersions.Add(v);
    }

    protected override void ClearFilterValues()
    {
        SearchText        = string.Empty;
        SelectedHWVersion = null;
    }
}
