using System.Collections.ObjectModel;
using App7.Domain.Constants;
using App7.Domain.Entities;
using App7.Presentation.Contracts.Services;
using App7.Domain.Usecases;
using App7.Domain.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace App7.Presentation.ViewModels;

public partial class MyDevicesViewModel : PagedListViewModelBase<Device>
{

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

    public MyDevicesViewModel(
        GetBorrowedDevicesUseCase getDevices,
        IInstanceSyncService      syncService) : base(syncService)
    {
        _getDevices  = getDevices;
    }

    // ── PagedListViewModelBase contract ────────────────────────────────
    protected override async Task LoadDataCoreAsync()
    {
        var request = new GetBorrowedDevicesRequest(
            Page:               CurrentPage,
            PageSize:           SelectedPageSize,
            SearchName:         NullIfEmpty(SearchName),
            SearchModelName:    NullIfEmpty(SearchModelName),
            SearchIMEI:         NullIfEmpty(SearchIMEI),
            SearchSerialLab:    NullIfEmpty(SearchSerialLab),
            SearchSerialNumber: NullIfEmpty(SearchSerialNumber),
            SearchCircuitSerial:NullIfEmpty(SearchCircuitSerial),
            SearchHWVersion:    NullIfEmpty(SearchHWVersion),
            SortColumn:         SortColumn,
            Ascending:          SortAscending);

        var (items, total) = await _getDevices.ExecuteAsync(request);

        Items.Clear();
        foreach (var d in items) Items.Add(d);
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
}
