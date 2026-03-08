using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.ViewModels;
using App7.Presentation.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace App7.Presentation.Views;

public sealed partial class MyDevicesPage : Page
{
    public MyDevicesViewModel ViewModel { get; }
    private readonly ReturnDeviceUseCase _returnUseCase;

    public MyDevicesPage()
    {
        ViewModel      = App.GetService<MyDevicesViewModel>();
        _returnUseCase = App.GetService<ReturnDeviceUseCase>();
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Required;

        foreach (var col in ViewModel.ColumnVisibilities)
            col.PropertyChanged += (_, _) => DevicesTable.SyncColumnVisibility(col.ColumnTag, col.IsVisible);
    }

    // ── Return device ─────────────────────────────────────────────────
    private async void OnReturnClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Device device) return;

        var dialog = new ReturnDialog(_returnUseCase) { XamlRoot = XamlRoot };
        dialog.Init(device);
        await dialog.ShowAsync();

        if (dialog.Confirmed)
        {
            await ViewModel.ReloadAsync();
            ShowInfoBar(InfoBarSeverity.Success,
                $"Returned \"{device.ModelName}\" (IMEI: {device.IMEI}) successfully.");
        }
    }

    // ── InfoBar ───────────────────────────────────────────────────────
    private void ShowInfoBar(InfoBarSeverity severity, string message)
    {
        ReturnInfoBar.Severity = severity;
        ReturnInfoBar.Message  = message;
        ReturnInfoBar.IsOpen   = true;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (_, _) => { ReturnInfoBar.IsOpen = false; timer.Stop(); };
        timer.Start();
    }


}
