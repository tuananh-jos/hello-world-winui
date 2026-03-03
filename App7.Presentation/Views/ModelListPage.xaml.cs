using App7.Presentation.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Views;

public sealed partial class ModelListPage : Page
{
    public ModelListViewModel ViewModel { get; }

    public ModelListPage()
    {
        ViewModel = App.GetService<ModelListViewModel>();
        InitializeComponent();
    }

    private void OnSorting(object sender, DataGridColumnEventArgs e)
    {
        var columnName = e.Column.Tag?.ToString();
        if (string.IsNullOrEmpty(columnName)) return;

        _ = ViewModel.SortByCommand.ExecuteAsync(columnName);

        // Update sort direction glyph for all columns
        var grid = (DataGrid)sender;
        foreach (var col in grid.Columns)
            col.SortDirection = null;

        e.Column.SortDirection = ViewModel.SortAscending
            ? DataGridSortDirection.Ascending
            : DataGridSortDirection.Descending;
    }
}
