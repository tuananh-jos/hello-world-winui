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
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using App7.Domain.Constants;
using App7.Presentation.Helpers;

namespace App7.Presentation.Views;

public sealed partial class ModelListPage : Page
{
    public ModelListViewModel ViewModel { get; }
    private readonly BorrowDeviceUseCase _borrowUseCase;
    private readonly DataGridSyncHelper _syncHelper;


    private string? _selectedManufacturer;
    private string? _selectedCategory;
    private string? _selectedSubCategory;

    public ModelListPage()
    {
        ViewModel      = App.GetService<ModelListViewModel>();
        _borrowUseCase = App.GetService<BorrowDeviceUseCase>();
        InitializeComponent();

        _syncHelper = new DataGridSyncHelper(ModelsGrid, new[]
        {
            new ColumnSyncInfo { Tag = ColumnTags.NAME,         SortIcon = SortIconName,         HeaderColumn = HdrColName,         FilterColumn = FltColName,         NaturalMinWidth = 120 },
            new ColumnSyncInfo { Tag = ColumnTags.MANUFACTURER, SortIcon = SortIconManufacturer, HeaderColumn = HdrColManufacturer, FilterColumn = FltColManufacturer, NaturalMinWidth = 100 },
            new ColumnSyncInfo { Tag = ColumnTags.CATEGORY,     SortIcon = SortIconCategory,     HeaderColumn = HdrColCategory,     FilterColumn = FltColCategory,     NaturalMinWidth = 90 },
            new ColumnSyncInfo { Tag = ColumnTags.SUB_CATEGORY,  SortIcon = SortIconSubCategory,  HeaderColumn = HdrColSubCategory,  FilterColumn = FltColSubCategory,  NaturalMinWidth = 90 },
            new ColumnSyncInfo { Tag = ColumnTags.AVAILABLE,    SortIcon = SortIconAvailable,    HeaderColumn = HdrColAvailable,    FilterColumn = FltColAvailable,    NaturalWidth = new GridLength(100) },
            new ColumnSyncInfo { Tag = ColumnTags.FUNCTION,     SortIcon = null,                 HeaderColumn = HdrColFunction,     FilterColumn = FltColFunction,     NaturalWidth = new GridLength(120) }
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
            PopulateManufacturerList(string.Empty);
            PopulateCategoryList(string.Empty);
            PopulateSubCategoryList(string.Empty);
        };

        // Hover effect on DataGrid rows
        ModelsGrid.LoadingRow += (_, e) =>
        {
            var row = e.Row;
            row.PointerEntered += (_, _) =>
                row.Background = new SolidColorBrush(Color.FromArgb(255, 0xEC, 0xF3, 0xF8));
            row.PointerExited += (_, _) =>
                row.Background = new SolidColorBrush(Colors.Transparent);
        };

        // Reset all filter labels when ClearFilters is called
        ViewModel.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedCategory):
                    // If category was cleared (by ClearFilters), reset label too
                    if (ViewModel.SelectedCategory == null)
                    {
                        _selectedCategory = null;
                        CategoryFilterLabel.Text = "All categories";
                        CategoryFilterLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x88, 0x88, 0x88));
                    }
                    break;
                case nameof(ViewModel.SelectedSubCategory):
                    if (ViewModel.SelectedSubCategory == null)
                    {
                        _selectedSubCategory = null;
                        SubCategoryFilterLabel.Text = "All sub-categories";
                        SubCategoryFilterLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x88, 0x88, 0x88));
                    }
                    break;
                case nameof(ViewModel.SelectedManufacturer):
                    if (ViewModel.SelectedManufacturer == null)
                    {
                        _selectedManufacturer = null;
                        ManufacturerFilterLabel.Text = "All manufacturers";
                        ManufacturerFilterLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x88, 0x88, 0x88));
                    }
                    break;
                case nameof(ViewModel.SearchName):
                    if (string.IsNullOrEmpty(ViewModel.SearchName))
                        SearchNameBox.Text = string.Empty;
                    break;
            }
        };
    }

    // ── Filter handlers ───────────────────────────────────────────────

    private void OnGridSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        => ModelsGrid.SelectedItem = null;

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
        var allBtn = MakeFilterItem("All manufacturers", _selectedManufacturer == null);
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
            ? (IEnumerable<string>)ViewModel.Manufacturers
            : ViewModel.Manufacturers.Where(m => m.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();

        foreach (var name in matches)
        {
            var captured = name;
            var btn = MakeFilterItem(captured, _selectedManufacturer == captured);
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

    private static Button MakeFilterItem(string text, bool isSelected)
    {
        return new Button
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
    }

    // ── Category filter popup ─────────────────────────────────────────
    private void OnCategoryFilterClicked(object sender, RoutedEventArgs e)
    {
        CategorySearchBox.Text = string.Empty;
        PopulateCategoryList(string.Empty);
        var transform = CategoryFilterBtn.TransformToVisual(PageRoot);
        var pt = transform.TransformPoint(new Point(0, CategoryFilterBtn.ActualHeight + 2));
        CategoryPopup.HorizontalOffset = pt.X;
        CategoryPopup.VerticalOffset   = pt.Y;
        CategoryPopup.IsOpen = true;
    }

    private void OnCategorySearchChanged(object sender, TextChangedEventArgs e)
        => PopulateCategoryList(CategorySearchBox.Text.Trim());

    private void PopulateCategoryList(string filter)
    {
        CategoryListPanel.Children.Clear();

        var allBtn = MakeFilterItem("All categories", _selectedCategory == null);
        allBtn.Click += async (_, _) =>
        {
            _selectedCategory = null;
            ViewModel.SelectedCategory = null;
            CategoryFilterLabel.Text = "All categories";
            CategoryFilterLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x88, 0x88, 0x88));
            CategoryPopup.IsOpen = false;
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        };
        CategoryListPanel.Children.Add(allBtn);

        var matches = string.IsNullOrEmpty(filter)
            ? ViewModel.Categories
            : ViewModel.Categories.Where(c => c.Contains(filter, StringComparison.OrdinalIgnoreCase));

        foreach (var cat in matches)
        {
            var captured = cat;
            var btn = MakeFilterItem(captured, _selectedCategory == captured);
            btn.Click += async (_, _) =>
            {
                _selectedCategory = captured;
                ViewModel.SelectedCategory = captured;
                CategoryFilterLabel.Text = captured;
                CategoryFilterLabel.Foreground = new SolidColorBrush(Colors.Black);
                CategoryPopup.IsOpen = false;
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            };
            CategoryListPanel.Children.Add(btn);
        }
    }

    // ── SubCategory filter popup ──────────────────────────────────────
    private void OnSubCategoryFilterClicked(object sender, RoutedEventArgs e)
    {
        SubCategorySearchBox.Text = string.Empty;
        PopulateSubCategoryList(string.Empty);
        var transform = SubCategoryFilterBtn.TransformToVisual(PageRoot);
        var pt = transform.TransformPoint(new Point(0, SubCategoryFilterBtn.ActualHeight + 2));
        SubCategoryPopup.HorizontalOffset = pt.X;
        SubCategoryPopup.VerticalOffset   = pt.Y;
        SubCategoryPopup.IsOpen = true;
    }

    private void OnSubCategorySearchChanged(object sender, TextChangedEventArgs e)
        => PopulateSubCategoryList(SubCategorySearchBox.Text.Trim());

    private void PopulateSubCategoryList(string filter = "")
    {
        SubCategoryListPanel.Children.Clear();

        var allBtn = MakeFilterItem("All sub-categories", _selectedSubCategory == null);
        allBtn.Click += async (_, _) =>
        {
            _selectedSubCategory = null;
            ViewModel.SelectedSubCategory = null;
            SubCategoryFilterLabel.Text = "All sub-categories";
            SubCategoryFilterLabel.Foreground = new SolidColorBrush(Color.FromArgb(255, 0x88, 0x88, 0x88));
            SubCategoryPopup.IsOpen = false;
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        };
        SubCategoryListPanel.Children.Add(allBtn);

        var matches = string.IsNullOrEmpty(filter)
            ? ViewModel.SubCategories
            : ViewModel.SubCategories.Where(s => s.Contains(filter, StringComparison.OrdinalIgnoreCase));

        foreach (var sub in matches)
        {
            var captured = sub;
            var btn = MakeFilterItem(captured, _selectedSubCategory == captured);
            btn.Click += async (_, _) =>
            {
                _selectedSubCategory = captured;
                ViewModel.SelectedSubCategory = captured;
                SubCategoryFilterLabel.Text = captured;
                SubCategoryFilterLabel.Foreground = new SolidColorBrush(Colors.Black);
                SubCategoryPopup.IsOpen = false;
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            };
            SubCategoryListPanel.Children.Add(btn);
        }
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

}
