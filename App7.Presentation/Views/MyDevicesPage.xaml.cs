using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace App7.Presentation.Views;

public sealed partial class MyDevicesPage : Page
{
    public MyDevicesViewModel ViewModel { get; }
    private readonly ReturnDeviceUseCase _returnUseCase;

    private static readonly SolidColorBrush ActivePageBrush
        = (SolidColorBrush)Application.Current.Resources["AppOkBrush"];
    private static readonly SolidColorBrush InactivePageBrush = new(Colors.Transparent);
    private static readonly SolidColorBrush InactiveTextBrush = new(Color.FromArgb(255, 0x33, 0x33, 0x33));

    public MyDevicesPage()
    {
        ViewModel      = App.GetService<MyDevicesViewModel>();
        _returnUseCase = App.GetService<ReturnDeviceUseCase>();
        InitializeComponent();

        ViewModel.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SortColumn) or nameof(ViewModel.SortAscending):
                    UpdateSortIcons();
                    break;
                case nameof(ViewModel.PageNumbers) or nameof(ViewModel.CurrentPage):
                    RebuildPageNumberButtons();
                    break;
            }
        };

        foreach (var col in ViewModel.ColumnVisibilities)
            col.PropertyChanged += (_, _) => SyncColumnVisibility(col);

        Loaded += (_, _) =>
        {
            ContentGrid.MinHeight = MainScroller.ActualHeight;
            MainScroller.SizeChanged += (_, e) => ContentGrid.MinHeight = e.NewSize.Height;
        };
    }

    // ── Sort icons ────────────────────────────────────────────────────
    private void UpdateSortIcons()
    {
        var icons = new (string col, TextBlock tb)[]
        {
            ("Name",                SortIconName),
            ("IMEI",                SortIconIMEI),
            ("SerialLab",           SortIconSerialLab),
            ("SerialNumber",        SortIconSerialNumber),
            ("CircuitSerialNumber", SortIconCircuitSerial),
            ("HWVersion",           SortIconHWVersion),
        };

        foreach (var (col, tb) in icons)
        {
            if (col == ViewModel.SortColumn)
            {
                tb.Text    = ViewModel.SortAscending ? "\uE70E" : "\uE70D";
                tb.Opacity = 1.0;
            }
            else
            {
                tb.Text    = "\uE70D";
                tb.Opacity = 0.4;
            }
        }
    }

    // ── Filter handlers ───────────────────────────────────────────────
    private async void OnSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
    }

    private async void OnHWVersionChanged(object sender, SelectionChangedEventArgs e)
        => await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);

    // ── Return device ─────────────────────────────────────────────────
    private async void OnReturnClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Device device) return;

        var confirm = new ContentDialog
        {
            Title = "Return Device",
            Content = $"Return {device.Name}?\nIMEI: {device.IMEI}",
            PrimaryButtonText = "Return",
            CloseButtonText   = "Cancel",
            DefaultButton     = ContentDialogButton.Primary,
            XamlRoot          = XamlRoot
        };

        if (await confirm.ShowAsync() != ContentDialogResult.Primary) return;

        try
        {
            await _returnUseCase.ExecuteAsync(device.Id, device.ModelId);
            await ViewModel.ReloadAsync();
            ShowInfoBar(InfoBarSeverity.Success, $"Returned \"{device.Name}\" successfully.");
        }
        catch (Exception ex)
        {
            ShowInfoBar(InfoBarSeverity.Error, $"Return failed: {ex.Message}");
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

    // ── Page-number buttons ───────────────────────────────────────────
    private void RebuildPageNumberButtons()
    {
        PageNumbersPanel.Children.Clear();
        foreach (var pageNum in ViewModel.PageNumbers)
        {
            var isActive = pageNum == ViewModel.CurrentPage;
            var btn = new Button
            {
                Content = pageNum.ToString(),
                Width = 32, Height = 32,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(0),
                Margin = new Thickness(2, 0, 2, 0),
                Background = isActive ? ActivePageBrush : InactivePageBrush,
                Foreground = isActive ? new SolidColorBrush(Colors.White) : InactiveTextBrush,
                BorderThickness = new Thickness(isActive ? 0 : 1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0xDD, 0xDD, 0xDD)),
            };
            var captured = pageNum;
            btn.Click += async (_, _) => await ViewModel.GoToPageCommand.ExecuteAsync(captured);
            PageNumbersPanel.Children.Add(btn);
        }
    }

    // ── Columns popup ─────────────────────────────────────────────────
    private void OnColumnsButtonClicked(object sender, RoutedEventArgs e)
    {
        var transform = ColumnsBtn.TransformToVisual(PageRoot);
        var pt = transform.TransformPoint(new Windows.Foundation.Point(0, ColumnsBtn.ActualHeight + 4));
        ColumnsPanel.Margin = new Thickness(pt.X, pt.Y, 0, 0);
        ViewModel.IsColumnsPopupOpen = true;
    }

    private void OnColumnsOverlayPressed(object sender, PointerRoutedEventArgs e)
        => ViewModel.CloseColumnsPopupCommand.Execute(null);

    // ── Column visibility ─────────────────────────────────────────────
    private void SyncColumnVisibility(ColumnVisibilityItem item)
    {
        foreach (var col in DevicesGrid.Columns)
        {
            if (col.Tag?.ToString() == item.ColumnTag)
            {
                col.Visibility = item.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                break;
            }
        }
    }
}
