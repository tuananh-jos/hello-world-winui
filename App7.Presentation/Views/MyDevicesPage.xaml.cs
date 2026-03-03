using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Views;

public sealed partial class MyDevicesPage : Page
{
    public MyDevicesViewModel ViewModel { get; }

    private readonly ReturnDeviceUseCase _returnUseCase;

    public MyDevicesPage()
    {
        ViewModel = App.GetService<MyDevicesViewModel>();
        _returnUseCase = App.GetService<ReturnDeviceUseCase>();
        InitializeComponent();
    }

    // ── DataGrid sort ─────────────────────────────────────────────────
    private void OnSorting(object sender, DataGridColumnEventArgs e)
    {
        var columnName = e.Column.Tag?.ToString();
        if (string.IsNullOrEmpty(columnName)) return;

        _ = ViewModel.SortByCommand.ExecuteAsync(columnName);

        var grid = (DataGrid)sender;
        foreach (var col in grid.Columns)
            col.SortDirection = null;

        e.Column.SortDirection = ViewModel.SortAscending
            ? DataGridSortDirection.Ascending
            : DataGridSortDirection.Descending;
    }

    // ── Return button (UC6) ───────────────────────────────────────────
    private async void OnReturnClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Device device) return;

        // Simple confirmation dialog
        var confirm = new ContentDialog
        {
            Title = "Return Device",
            Content = $"Return {device.Name}?\nIMEI: {device.IMEI}",
            PrimaryButtonText = "Return",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await confirm.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        try
        {
            await _returnUseCase.ExecuteAsync(device.Id, device.ModelId);
            await ViewModel.ReloadAsync();
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Return failed",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
