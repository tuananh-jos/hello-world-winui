using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.ViewModels;
using App7.Presentation.Views.Dialogs;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace App7.Presentation.Views;

public sealed partial class ModelListPage : Page
{
    public ModelListViewModel ViewModel { get; }
    private readonly BorrowDeviceUseCase _borrowUseCase;

    private static readonly SolidColorBrush ActivePageBrush
        = (SolidColorBrush)Application.Current.Resources["AppOkBrush"];
    private static readonly SolidColorBrush InactivePageBrush = new(Colors.Transparent);
    private static readonly SolidColorBrush InactiveTextBrush = new(Color.FromArgb(255, 0x33, 0x33, 0x33));

    // Hardcoded manufacturer list
    private static readonly string[] AllManufacturers =
    {
        "Alcatel", "Apple", "ASUS", "BBK", "Bkav",
        "BlackBerry", "Canon", "Chary mobile", "Circuit", "Company", "Samsung"
    };

    private string? _selectedManufacturer;

    public ModelListPage()
    {
        ViewModel      = App.GetService<ModelListViewModel>();
        _borrowUseCase = App.GetService<BorrowDeviceUseCase>();
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
            PopulateManufacturerList(string.Empty);
        };
    }

    // ── Sort icons ────────────────────────────────────────────────────
    private void UpdateSortIcons()
    {
        var icons = new (string col, TextBlock tb)[]
        {
            ("Name",         SortIconName),
            ("Manufacturer", SortIconManufacturer),
            ("Category",     SortIconCategory),
            ("SubCategory",  SortIconSubCategory),
            ("Available",    SortIconAvailable),
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

    private async void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
        => await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);

    private async void OnSubCategoryChanged(object sender, SelectionChangedEventArgs e)
        => await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);

    // ── Manufacturer filter popup ─────────────────────────────────────
    private void OnManufacturerFilterClicked(object sender, RoutedEventArgs e)
    {
        // Position popup directly below the filter button
        var transform = ManufacturerFilterBtn.TransformToVisual(PageRoot);
        var pt = transform.TransformPoint(new Point(0, ManufacturerFilterBtn.ActualHeight + 2));

        ManufacturerPopup.HorizontalOffset = pt.X;
        ManufacturerPopup.VerticalOffset   = pt.Y;
        ManufacturerSearchBox.Text = string.Empty;
        PopulateManufacturerList(string.Empty);
        ManufacturerPopup.IsOpen = true;
    }

    private void OnManufacturerSearchChanged(object sender, TextChangedEventArgs e)
        => PopulateManufacturerList(ManufacturerSearchBox.Text.Trim());

    private void PopulateManufacturerList(string filter)
    {
        ManufacturerListPanel.Children.Clear();

        // "All" option at top
        var allBtn = MakeManufacturerItem("All manufacturers", _selectedManufacturer == null);
        allBtn.Click += async (_, _) =>
        {
            _selectedManufacturer = null;
            ViewModel.SelectedManufacturer = null;
            ManufacturerFilterLabel.Text = "All manufacturers";
            ManufacturerFilterLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x88, 0x88, 0x88));
            ManufacturerPopup.IsOpen = false;
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        };
        ManufacturerListPanel.Children.Add(allBtn);

        // Filtered list
        var matches = string.IsNullOrEmpty(filter)
            ? AllManufacturers
            : AllManufacturers.Where(m => m.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();

        foreach (var name in matches)
        {
            var captured = name;
            var btn = MakeManufacturerItem(captured, _selectedManufacturer == captured);
            btn.Click += async (_, _) =>
            {
                _selectedManufacturer = captured;
                ViewModel.SelectedManufacturer = captured;
                ManufacturerFilterLabel.Text = captured;
                ManufacturerFilterLabel.Foreground = new SolidColorBrush(Colors.Black);
                ManufacturerPopup.IsOpen = false;
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            };
            ManufacturerListPanel.Children.Add(btn);
        }
    }

    private static Button MakeManufacturerItem(string text, bool isSelected)
    {
        return new Button
        {
            Content             = text,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background          = isSelected
                                    ? new SolidColorBrush(Color.FromArgb(255, 0xE8, 0xF4, 0xF8))
                                    : new SolidColorBrush(Colors.Transparent),
            BorderThickness     = new Thickness(0),
            Padding             = new Thickness(8, 4, 8, 4),
            FontSize            = 12,
        };
    }

    // ── Borrow ────────────────────────────────────────────────────────
    private async void OnBorrowClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Model model) return;

        var dialog = new BorrowDialog(_borrowUseCase) { XamlRoot = XamlRoot };
        dialog.Init(model);
        await dialog.ShowAsync();

        if (dialog.ViewModel.Confirmed)
        {
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            ShowInfoBar(InfoBarSeverity.Success,
                $"Borrowed {dialog.ViewModel.SelectedQuantity} device(s) from \"{model.Name}\" successfully.");
        }
    }

    // ── InfoBar ───────────────────────────────────────────────────────
    private void ShowInfoBar(InfoBarSeverity severity, string message)
    {
        BorrowInfoBar.Severity = severity;
        BorrowInfoBar.Message  = message;
        BorrowInfoBar.IsOpen   = true;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (_, _) => { BorrowInfoBar.IsOpen = false; timer.Stop(); };
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
    private static readonly (string Tag, ColumnDefinition HdrCol, ColumnDefinition FltCol)[] _colMapCache
        = Array.Empty<(string, ColumnDefinition, ColumnDefinition)>();

    private void SyncColumnVisibility(ColumnVisibilityItem item)
    {
        var visible = item.IsVisible;
        var tag = item.ColumnTag;

        // 1. DataGrid column
        foreach (var col in ModelsGrid.Columns)
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
            hdrColDef.Width = visible ? GetNaturalWidth(tag) : new GridLength(0);

        // 3. Filter ColumnDefinition width
        var fltColDef = GetFilterColDef(tag);
        if (fltColDef != null)
            fltColDef.Width = visible ? GetNaturalWidth(tag) : new GridLength(0);
    }

    private ColumnDefinition? GetHeaderColDef(string tag) => tag switch
    {
        "Name"        => HdrColName,
        "Manufacturer"=> HdrColManufacturer,
        "Category"    => HdrColCategory,
        "SubCategory" => HdrColSubCategory,
        "Available"   => HdrColAvailable,
        "Function"    => HdrColFunction,
        _             => null
    };

    private ColumnDefinition? GetFilterColDef(string tag) => tag switch
    {
        "Name"        => FltColName,
        "Manufacturer"=> FltColManufacturer,
        "Category"    => FltColCategory,
        "SubCategory" => FltColSubCategory,
        "Available"   => FltColAvailable,
        "Function"    => FltColFunction,
        _             => null
    };

    private static GridLength GetNaturalWidth(string tag) => tag switch
    {
        "Name"        => new GridLength(200),
        "Manufacturer"=> new GridLength(160),
        "Category"    => new GridLength(140),
        "SubCategory" => new GridLength(140),
        "Available"   => new GridLength(100),
        "Function"    => new GridLength(120),
        _             => new GridLength(100),
    };
}
