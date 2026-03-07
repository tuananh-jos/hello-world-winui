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
            PopulatePageSizeList();
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

    // ── Sort icons ────────────────────────────────────────────────────
    private void UpdateSortIcons()
    {
        var icons = new (string col, TextBlock tb)[]
        {
            ("Name",                SortIconName),
            ("ModelName",           SortIconModelName),
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

    // ── Sync header/filter column widths ──────────────────────────────
    private bool _syncingLayout = false;
    private void OnGridLayoutUpdated(object? sender, object e)
    {
        if (_syncingLayout) return;
        if (DevicesGrid.Columns.Count < 8) return;

        var colWidths = DevicesGrid.Columns.Select(c => c.ActualWidth).ToArray();
        if (colWidths.All(w => w <= 0)) return;

        _syncingLayout = true;
        try
        {
            ColDefName.Width         = new GridLength(colWidths[0]);
            ColDefModelName.Width    = new GridLength(colWidths[1]);
            ColDefIMEI.Width         = new GridLength(colWidths[2]);
            ColDefSerialLab.Width    = new GridLength(colWidths[3]);
            ColDefSerialNumber.Width = new GridLength(colWidths[4]);
            ColDefCircuitSerial.Width= new GridLength(colWidths[5]);
            ColDefHWVersion.Width    = new GridLength(colWidths[6]);
            ColDefFunction.Width     = new GridLength(colWidths[7]);

            FilterColDefName.Width         = new GridLength(colWidths[0]);
            FilterColDefModelName.Width    = new GridLength(colWidths[1]);
            FilterColDefIMEI.Width         = new GridLength(colWidths[2]);
            FilterColDefSerialLab.Width    = new GridLength(colWidths[3]);
            FilterColDefSerialNumber.Width = new GridLength(colWidths[4]);
            FilterColDefCircuitSerial.Width= new GridLength(colWidths[5]);
            FilterColDefHWVersion.Width    = new GridLength(colWidths[6]);
            FilterColDefFunction.Width     = new GridLength(colWidths[7]);
        }
        finally
        {
            _syncingLayout = false;
        }
    }

    // ── PageSize popup ────────────────────────────────────────────────
    private void OnPageSizeFilterClicked(object sender, RoutedEventArgs e)
    {
        PopulatePageSizeList();
        var transform = PageSizeFilterBtn.TransformToVisual(PageRoot);
        var pt = transform.TransformPoint(new Point(0, PageSizeFilterBtn.ActualHeight + 2));
        PageSizePopup.HorizontalOffset = pt.X;
        PageSizePopup.VerticalOffset   = pt.Y;
        PageSizePopup.IsOpen = true;
    }

    private void PopulatePageSizeList()
    {
        PageSizeListPanel.Children.Clear();
        foreach (var size in ViewModel.PageSizeOptions)
        {
            var captured = size;
            var btn = MakeFilterItem(size.ToString(), ViewModel.SelectedPageSize == captured);
            btn.Click += async (_, _) =>
            {
                ViewModel.SelectedPageSize = captured;
                PageSizeFilterLabel.Text = captured.ToString();
                PageSizePopup.IsOpen = false;
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            };
            PageSizeListPanel.Children.Add(btn);
        }
    }

    private static Button MakeFilterItem(string text, bool isSelected) => new()
    {
        Content = text,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Left,
        Background = isSelected
            ? new SolidColorBrush(Color.FromArgb(255, 0xE8, 0xF4, 0xF8))
            : new SolidColorBrush(Colors.Transparent),
        BorderThickness = new Thickness(0),
        Padding = new Thickness(8, 4, 8, 4),
        FontSize = 12,
    };

    // ── Shared search handlers ──────────────────────────────────────
    private DispatcherTimer? _searchDebounceTimer;

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var text = tb.Text;

        switch (tb.Tag?.ToString())
        {
            case "Name":             ViewModel.SearchName        = text; break;
            case "ModelName":        ViewModel.SearchModelName   = text; break;
            case "IMEI":             ViewModel.SearchIMEI        = text; break;
            case "SerialLab":        ViewModel.SearchSerialLab   = text; break;
            case "SerialNumber":     ViewModel.SearchSerialNumber= text; break;
            case "CircuitSerialNumber": ViewModel.SearchCircuitSerial= text; break;
            case "HWVersion":        ViewModel.SearchHWVersion   = text; break;
        }

        if (_searchDebounceTimer == null)
        {
            _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _searchDebounceTimer.Tick += async (_, _) =>
            {
                _searchDebounceTimer.Stop();
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            };
        }
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

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
        var pt = transform.TransformPoint(new Point(0, ColumnsBtn.ActualHeight + 4));
        ColumnsPanel.Margin = new Thickness(pt.X, pt.Y, 0, 0);
        ViewModel.IsColumnsPopupOpen = true;
    }

    private void OnColumnsOverlayPressed(object sender, PointerRoutedEventArgs e)
        => ViewModel.CloseColumnsPopupCommand.Execute(null);

    // ── Column visibility — sync DataGrid column + header + filter ────
    private void SyncColumnVisibility(ColumnVisibilityItem item)
    {
        var visible = item.IsVisible;
        var tag = item.ColumnTag;

        // 1. DataGrid column
        foreach (var col in DevicesGrid.Columns)
        {
            if (col.Tag?.ToString() == tag)
            {
                col.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                break;
            }
        }

        // 2. Header ColumnDefinition width
        var hdrColDef = GetHeaderColDef(tag);
        if (hdrColDef != null)
        {
            hdrColDef.Width = visible ? GetNaturalWidth(tag) : new GridLength(0);
            hdrColDef.MinWidth = visible ? GetNaturalMinWidth(tag) : 0;
            hdrColDef.MaxWidth = visible ? double.PositiveInfinity : 0;
        }

        // 3. Filter ColumnDefinition width
        var fltColDef = GetFilterColDef(tag);
        if (fltColDef != null)
        {
            fltColDef.Width = visible ? GetNaturalWidth(tag) : new GridLength(0);
            fltColDef.MinWidth = visible ? GetNaturalMinWidth(tag) : 0;
            fltColDef.MaxWidth = visible ? double.PositiveInfinity : 0;
        }
    }

    private ColumnDefinition? GetHeaderColDef(string tag) => tag switch
    {
        "Name"               => ColDefName,
        "ModelName"          => ColDefModelName,
        "IMEI"               => ColDefIMEI,
        "SerialLab"          => ColDefSerialLab,
        "SerialNumber"       => ColDefSerialNumber,
        "CircuitSerialNumber"=> ColDefCircuitSerial,
        "HWVersion"          => ColDefHWVersion,
        "Function"           => ColDefFunction,
        _                    => null
    };

    private ColumnDefinition? GetFilterColDef(string tag) => tag switch
    {
        "Name"               => FilterColDefName,
        "ModelName"          => FilterColDefModelName,
        "IMEI"               => FilterColDefIMEI,
        "SerialLab"          => FilterColDefSerialLab,
        "SerialNumber"       => FilterColDefSerialNumber,
        "CircuitSerialNumber"=> FilterColDefCircuitSerial,
        "HWVersion"          => FilterColDefHWVersion,
        "Function"           => FilterColDefFunction,
        _                    => null
    };

    private static GridLength GetNaturalWidth(string tag) => tag switch
    {
        "Function" => new GridLength(120),
        _ => new GridLength(1, GridUnitType.Star)
    };

    private static double GetNaturalMinWidth(string tag) => tag switch
    {
        "Name"               => 100,
        "ModelName"          => 120,
        "IMEI"               => 100,
        "SerialLab"          => 80,
        "SerialNumber"       => 80,
        "CircuitSerialNumber"=> 80,
        "HWVersion"          => 70,
        _                    => 0
    };
}
