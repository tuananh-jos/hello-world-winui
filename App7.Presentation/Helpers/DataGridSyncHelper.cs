using System;
using System.Collections.Generic;
using System.Linq;
using App7.Presentation.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Helpers;

public class ColumnSyncInfo
{
    public string Tag { get; set; } = string.Empty;
    public TextBlock? SortIcon { get; set; }
    public ColumnDefinition? HeaderColumn { get; set; }
    public ColumnDefinition? FilterColumn { get; set; }
    public GridLength NaturalWidth { get; set; } = new GridLength(1, GridUnitType.Star);
    public double NaturalMinWidth { get; set; } = 0;
}

public class DataGridSyncHelper
{
    private readonly DataGrid _dataGrid;
    private readonly Dictionary<string, ColumnSyncInfo> _columns;
    private bool _syncingLayout;

    public DataGridSyncHelper(DataGrid dataGrid, IEnumerable<ColumnSyncInfo> columns)
    {
        _dataGrid = dataGrid;
        _columns = columns.ToDictionary(c => c.Tag);

        _dataGrid.LayoutUpdated += OnGridLayoutUpdated;
    }

    public void UpdateSortIcons(string? sortColumn, bool sortAscending)
    {
        foreach (var col in _columns.Values)
        {
            if (col.SortIcon == null) continue;

            if (col.Tag == sortColumn)
            {
                col.SortIcon.Text    = sortAscending ? "\uE70E" : "\uE70D";
                col.SortIcon.Opacity = 1.0;
            }
            else
            {
                col.SortIcon.Text    = "\uE70D";
                col.SortIcon.Opacity = 0.4;
            }
        }
    }

    private void OnGridLayoutUpdated(object? sender, object e)
    {
        if (_syncingLayout) return;

        // Ensure we have rendered columns
        var renderedColumns = _dataGrid.Columns.Where(c => c.Visibility == Visibility.Visible).ToList();
        if (renderedColumns.Count == 0) return;

        var colWidths = _dataGrid.Columns.Select(c => c.ActualWidth).ToArray();
        if (colWidths.All(w => w <= 0)) return;

        _syncingLayout = true;
        try
        {
            for (int i = 0; i < _dataGrid.Columns.Count; i++)
            {
                var tag = _dataGrid.Columns[i].Tag?.ToString();
                if (tag != null && _columns.TryGetValue(tag, out var info))
                {
                    var width = new GridLength(colWidths[i]);
                    if (info.HeaderColumn != null) info.HeaderColumn.Width = width;
                    if (info.FilterColumn != null) info.FilterColumn.Width = width;
                }
            }
        }
        finally
        {
            _syncingLayout = false;
        }
    }

    public void SyncColumnVisibility(ColumnVisibilityItem item)
    {
        var visible = item.IsVisible;
        var tag     = item.ColumnTag;

        // 1. DataGrid column
        foreach (var col in _dataGrid.Columns)
        {
            if (col.Tag?.ToString() == tag)
            {
                col.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                break;
            }
        }

        // 2. Header and Filter ColumnDefinitions
        if (_columns.TryGetValue(tag, out var info))
        {
            var width = visible ? info.NaturalWidth : new GridLength(0);
            var minW  = visible ? info.NaturalMinWidth : 0;
            var maxW  = visible ? double.PositiveInfinity : 0;

            if (info.HeaderColumn != null)
            {
                info.HeaderColumn.Width    = width;
                info.HeaderColumn.MinWidth = minW;
                info.HeaderColumn.MaxWidth = maxW;
            }
            if (info.FilterColumn != null)
            {
                info.FilterColumn.Width    = width;
                info.FilterColumn.MinWidth = minW;
                info.FilterColumn.MaxWidth = maxW;
            }
        }
    }
}
