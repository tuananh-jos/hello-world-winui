using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.ViewModels;
using App7.Presentation.Views.Dialogs;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using App7.Domain.Constants;
using App7.Presentation.Helpers;

namespace App7.Presentation.Views;

public sealed partial class MyDevicesPage : Page
{
    public MyDevicesViewModel ViewModel { get; }
    private readonly ReturnDeviceUseCase _returnUseCase;
    private readonly DataGridSyncHelper _syncHelper;

    private static readonly SolidColorBrush ActivePageBrush
        = (SolidColorBrush)Application.Current.Resources["AppOkBrush"];
    private static readonly SolidColorBrush InactivePageBrush = new(Colors.Transparent);
    private static readonly SolidColorBrush InactiveTextBrush = new(Color.FromArgb(255, 0x33, 0x33, 0x33));

    public MyDevicesPage()
    {
        ViewModel      = App.GetService<MyDevicesViewModel>();
        _returnUseCase = App.GetService<ReturnDeviceUseCase>();
        InitializeComponent();

        _syncHelper = new DataGridSyncHelper(DevicesGrid, new[]
        {
            new ColumnSyncInfo { Tag = ColumnTags.NAME,                SortIcon = SortIconName,          HeaderColumn = ColDefName,          FilterColumn = FilterColDefName,          NaturalMinWidth = 100 },
            new ColumnSyncInfo { Tag = ColumnTags.MODEL_NAME,          SortIcon = SortIconModelName,     HeaderColumn = ColDefModelName,     FilterColumn = FilterColDefModelName,     NaturalMinWidth = 120 },
            new ColumnSyncInfo { Tag = ColumnTags.IMEI,                SortIcon = SortIconIMEI,          HeaderColumn = ColDefIMEI,          FilterColumn = FilterColDefIMEI,          NaturalMinWidth = 100 },
            new ColumnSyncInfo { Tag = ColumnTags.SERIAL_LAB,          SortIcon = SortIconSerialLab,     HeaderColumn = ColDefSerialLab,     FilterColumn = FilterColDefSerialLab,     NaturalMinWidth = 80 },
            new ColumnSyncInfo { Tag = ColumnTags.SERIAL_NUMBER,       SortIcon = SortIconSerialNumber,  HeaderColumn = ColDefSerialNumber,  FilterColumn = FilterColDefSerialNumber,  NaturalMinWidth = 80 },
            new ColumnSyncInfo { Tag = ColumnTags.CIRCUIT_SERIAL_NUMBER,SortIcon = SortIconCircuitSerial,HeaderColumn = ColDefCircuitSerial, FilterColumn = FilterColDefCircuitSerial, NaturalMinWidth = 80 },
            new ColumnSyncInfo { Tag = ColumnTags.HW_VERSION,          SortIcon = SortIconHWVersion,     HeaderColumn = ColDefHWVersion,     FilterColumn = FilterColDefHWVersion,     NaturalMinWidth = 70 },
            new ColumnSyncInfo { Tag = ColumnTags.FUNCTION,            SortIcon = null,                  HeaderColumn = ColDefFunction,      FilterColumn = FilterColDefFunction,      NaturalWidth = new GridLength(120) }
        });

        ViewModel.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SortColumn) or nameof(ViewModel.SortAscending):
                    _syncHelper.UpdateSortIcons(ViewModel.SortColumn, ViewModel.SortAscending);
                    break;
            }
        };

        foreach (var col in ViewModel.ColumnVisibilities)
            col.PropertyChanged += (_, _) => _syncHelper.SyncColumnVisibility(col);

        Loaded += (_, _) =>
        {
            ContentGrid.MinHeight = MainScroller.ActualHeight;
            MainScroller.SizeChanged += (_, e) => ContentGrid.MinHeight = e.NewSize.Height;
        };

        // Hover effect on DataGrid rows
        DevicesGrid.LoadingRow += (_, e) =>
        {
            var row = e.Row;
            row.PointerEntered += (_, _) =>
                row.Background = new SolidColorBrush(Color.FromArgb(255, 0xEC, 0xF3, 0xF8));
            row.PointerExited += (_, _) =>
                row.Background = new SolidColorBrush(Colors.Transparent);
        };
    }



    // ── Shared search handlers ──────────────────────────────────────

    private void OnGridSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        => DevicesGrid.SelectedItem = null;



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

    // ── Columns popup ─────────────────────────────────────────────────
    private void OnColumnsButtonClicked(object sender, RoutedEventArgs e)
    {
        var transform = ColumnsBtn.TransformToVisual(PageRoot);
        var pt = transform.TransformPoint(new Point(0, ColumnsBtn.ActualHeight + 4));
        ColumnsPanel.Margin = new Thickness(pt.X, pt.Y, 0, 0);
        ViewModel.IsColumnsPopupOpen = true;
    }

    private void OnColumnsOverlayPressed(object sender, PointerRoutedEventArgs e)
        => ViewModel.CloseColumnsPopupCommand.Execute(null);
}
