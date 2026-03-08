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
    }

    // ── Filter handlers ───────────────────────────────────────────────

    private void OnGridSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        => ModelsGrid.SelectedItem = null;





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
