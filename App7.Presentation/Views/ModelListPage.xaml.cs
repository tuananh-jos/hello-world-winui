using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.ViewModels;
using App7.Presentation.Views.Dialogs;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Views;

public sealed partial class ModelListPage : Page
{
    public ModelListViewModel ViewModel { get; }

    private readonly BorrowDeviceUseCase _borrowUseCase;

    public ModelListPage()
    {
        ViewModel = App.GetService<ModelListViewModel>();
        _borrowUseCase = App.GetService<BorrowDeviceUseCase>();
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

    // ── Borrow button ─────────────────────────────────────────────────
    private async void OnBorrowClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Model model) return;

        var dialog = new BorrowDialog(_borrowUseCase)
        {
            XamlRoot = XamlRoot
        };
        dialog.Init(model);

        await dialog.ShowAsync();

        // If the user confirmed, reload the list so Available count refreshes
        if (dialog.ViewModel.Confirmed)
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
    }
}
