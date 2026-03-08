using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using CommunityToolkit.WinUI.UI.Controls;

namespace App7.Presentation.Controls;

[Microsoft.UI.Xaml.Markup.ContentProperty(Name = "Columns")]
public sealed partial class SmartDataTable : UserControl
{
    // -- Properties --
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(SmartDataTable), new PropertyMetadata(null));
    public IEnumerable ItemsSource { get => (IEnumerable)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly DependencyProperty EmptyVisibilityProperty = DependencyProperty.Register(
        nameof(EmptyVisibility), typeof(Visibility), typeof(SmartDataTable), new PropertyMetadata(Visibility.Collapsed));
    public Visibility EmptyVisibility { get => (Visibility)GetValue(EmptyVisibilityProperty); set => SetValue(EmptyVisibilityProperty, value); }

    public static readonly DependencyProperty SortCommandProperty = DependencyProperty.Register(
        nameof(SortCommand), typeof(ICommand), typeof(SmartDataTable), new PropertyMetadata(null));
    public ICommand SortCommand { get => (ICommand)GetValue(SortCommandProperty); set => SetValue(SortCommandProperty, value); }

    public static readonly DependencyProperty ClearFiltersCommandProperty = DependencyProperty.Register(
        nameof(ClearFiltersCommand), typeof(ICommand), typeof(SmartDataTable), new PropertyMetadata(null));
    public ICommand ClearFiltersCommand { get => (ICommand)GetValue(ClearFiltersCommandProperty); set => SetValue(ClearFiltersCommandProperty, value); }

    public static readonly DependencyProperty SortColumnProperty = DependencyProperty.Register(
        nameof(SortColumn), typeof(string), typeof(SmartDataTable), new PropertyMetadata(string.Empty, OnSortColumnChanged));
    public string SortColumn { get => (string)GetValue(SortColumnProperty); set => SetValue(SortColumnProperty, value); }

    public static readonly DependencyProperty SortAscendingProperty = DependencyProperty.Register(
        nameof(SortAscending), typeof(bool), typeof(SmartDataTable), new PropertyMetadata(true, OnSortColumnChanged));
    public bool SortAscending { get => (bool)GetValue(SortAscendingProperty); set => SetValue(SortAscendingProperty, value); }

    public IList<SmartColumn> Columns { get; } = new List<SmartColumn>();

    private readonly Dictionary<string, SmartColumnSyncInfo> _syncInfo = new();
    private bool _syncingLayout;
    private bool _hasBuilt = false;

    public SmartDataTable()
    {
        this.InitializeComponent();

        Loaded += (_, _) => BuildTable();
        InnerDataGrid.LayoutUpdated += OnInnerDataGridLayoutUpdated;
        
        InnerDataGrid.LoadingRow += (_, e) =>
        {
            var row = e.Row;
            row.PointerEntered += (_, _) =>
                row.Background = new SolidColorBrush(Color.FromArgb(255, 0xEC, 0xF3, 0xF8));
            row.PointerExited += (_, _) =>
                row.Background = new SolidColorBrush(Colors.Transparent);
        };
    }

    private void BuildTable()
    {
        if (_hasBuilt) return;
        _hasBuilt = true;
        _syncingLayout = true; // Block sync while building columns

        HeaderGrid.ColumnDefinitions.Clear();
        FilterGrid.ColumnDefinitions.Clear();
        InnerDataGrid.Columns.Clear();
        _syncInfo.Clear();

        HeaderGrid.Children.Clear();
        FilterGrid.Children.Clear();

        for (int i = 0; i < Columns.Count; i++)
        {
            var col = Columns[i];

            // Setup Layout Columns
            var headerColDef = new ColumnDefinition { Width = col.Width, MinWidth = col.MinWidth };
            var filterColDef = new ColumnDefinition { Width = col.Width, MinWidth = col.MinWidth };
            HeaderGrid.ColumnDefinitions.Add(headerColDef);
            FilterGrid.ColumnDefinitions.Add(filterColDef);

            // Setup Header Sort Button or TextBlock
            TextBlock? sortIcon = null;
            if (col.IsSortable && !string.IsNullOrEmpty(col.Tag))
            {
                var btn = new HandButton
                {
                    Command = SortCommand,
                    CommandParameter = col.Tag,
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(0x4A, 0xFF, 0xFF, 0xFF)),
                    Height = 40,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Padding = new Thickness(8, 0, 8, 0)
                };

                // Forward DataContext so commands bind correctly
                btn.SetBinding(Button.CommandProperty, new Binding { Source = this, Path = new PropertyPath("SortCommand") });

                var grid = new Grid();
                var label = new TextBlock
                {
                    Text = col.Header,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 0x33, 0x7F, 0x94)),
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                sortIcon = new TextBlock
                {
                    Text = "\uE8CB", // Sort neutral icon
                    FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"],
                    FontSize = 10,
                    Opacity = 0.4,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                grid.Children.Add(label);
                grid.Children.Add(sortIcon);
                btn.Content = grid;

                Grid.SetColumn(btn, i);
                HeaderGrid.Children.Add(btn);
            }
            else
            {
                var border = new Border { Padding = new Thickness(8, 0, 8, 0), BorderThickness = new Thickness(0) };
                var text = new TextBlock
                {
                    Text = col.Header,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 0x33, 0x7F, 0x94)),
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center
                };
                border.Child = text;
                Grid.SetColumn(border, i);
                HeaderGrid.Children.Add(border);
            }

            // Setup Filter
            FrameworkElement? filterElement = null;
            if (col.FilterType == SmartFilterType.Text)
            {
                var tb = new TextBox
                {
                    PlaceholderText = col.FilterPlaceholder,
                    Margin = new Thickness(4, 0, 4, 0),
                    FontSize = 12
                };
                tb.SetBinding(TextBox.TextProperty, new Binding
                {
                    Source = col,
                    Path = new PropertyPath(nameof(SmartColumn.SearchText)),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                filterElement = tb;
            }
            else if (col.FilterType == SmartFilterType.Dropdown)
            {
                var picker = new SmartDropdownPicker
                {
                    Margin = new Thickness(4, 0, 4, 0),
                    ShowSearchBox = true,
                    ShowAllOption = true,
                    AllOptionLabel = col.AllOptionLabel,
                    DropdownMinWidth = col.DropdownMinWidth
                };
                picker.SetBinding(SmartDropdownPicker.ItemsSourceProperty, new Binding { Source = col, Path = new PropertyPath(nameof(SmartColumn.ItemsSource)), Mode = BindingMode.OneWay });
                picker.SetBinding(SmartDropdownPicker.SelectedItemProperty, new Binding { Source = col, Path = new PropertyPath(nameof(SmartColumn.SelectedItem)), Mode = BindingMode.TwoWay });
                picker.SetBinding(SmartDropdownPicker.SearchTextProperty, new Binding { Source = col, Path = new PropertyPath(nameof(SmartColumn.SearchTextDropdown)), Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                picker.SetBinding(SmartDropdownPicker.SelectionChangedCommandProperty, new Binding { Source = col, Path = new PropertyPath(nameof(SmartColumn.DropdownSelectionChangedCommand)), Mode = BindingMode.OneWay });
                filterElement = picker;
            }
            else if (col.FilterType == SmartFilterType.ClearButton)
            {
                var btn = new HandButton
                {
                    Content = "Clear Filter",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(4, 0, 4, 0)
                };
                
                if (Application.Current.Resources.TryGetValue("AppClearFilterButtonStyle", out var styleObj) && styleObj is Style style)
                    btn.Style = style;

                btn.SetBinding(Button.CommandProperty, new Binding { Source = this, Path = new PropertyPath("ClearFiltersCommand") });
                filterElement = btn;
            }

            if (filterElement != null)
            {
                Grid.SetColumn(filterElement, i);
                FilterGrid.Children.Add(filterElement);
            }

            // Setup DataGrid Column
            DataGridColumn dgCol;
            var dgWidth = col.Width.IsStar
                ? new DataGridLength(col.Width.Value, DataGridLengthUnitType.Star)
                : new DataGridLength(col.Width.Value, DataGridLengthUnitType.Pixel);

            if (col.CellTemplate != null)
            {
                dgCol = new DataGridTemplateColumn
                {
                    CellTemplate = col.CellTemplate,
                    Tag = col.Tag,
                    Width = dgWidth,
                    MinWidth = col.MinWidth,
                    IsReadOnly = true
                };
            }
            else
            {
                dgCol = new DataGridTextColumn
                {
                    Binding = new Binding { Path = new PropertyPath(col.BindingPath) },
                    Tag = col.Tag,
                    Width = dgWidth,
                    MinWidth = col.MinWidth,
                    IsReadOnly = true
                };
            }
            InnerDataGrid.Columns.Add(dgCol);

            // Sync Info
            if (!string.IsNullOrEmpty(col.Tag))
            {
                _syncInfo[col.Tag] = new SmartColumnSyncInfo
                {
                    Tag = col.Tag,
                    SortIcon = sortIcon,
                    HeaderColumn = headerColDef,
                    FilterColumn = filterColDef,
                    NaturalWidth = col.Width,
                    NaturalMinWidth = col.MinWidth
                };
            }
        }

        UpdateSortIcons();
        _syncingLayout = false; // Re-enable sync after all columns are built
    }

    private void OnInnerDataGridLayoutUpdated(object? sender, object e)
    {
        if (_syncingLayout) return;

        var renderedColumns = InnerDataGrid.Columns.Where(c => c.Visibility == Visibility.Visible).ToList();
        if (renderedColumns.Count == 0) return;

        var colWidths = InnerDataGrid.Columns.Select(c => c.ActualWidth).ToArray();
        if (colWidths.All(w => w <= 0)) return;

        _syncingLayout = true;
        try
        {
            for (int i = 0; i < InnerDataGrid.Columns.Count; i++)
            {
                var tag = InnerDataGrid.Columns[i].Tag?.ToString();
                if (tag != null && _syncInfo.TryGetValue(tag, out var info))
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

    private static void OnSortColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SmartDataTable)d).UpdateSortIcons();

    private void UpdateSortIcons()
    {
        foreach (var info in _syncInfo.Values)
        {
            if (info.SortIcon == null) continue;

            if (info.Tag == SortColumn)
            {
                info.SortIcon.Text = SortAscending ? "\uE70E" : "\uE70D";
                info.SortIcon.Opacity = 1.0;
            }
            else
            {
                info.SortIcon.Text = "\uE8CB"; // Neutral sort icon
                info.SortIcon.Opacity = 0.4;
            }
        }
    }

    private void OnInnerDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
        => InnerDataGrid.SelectedItem = null;

    // A helper method for setting ColumnVisibility dynamically
    public void SyncColumnVisibility(string tag, bool visible)
    {
        // 1. DataGrid column
        foreach (var col in InnerDataGrid.Columns)
        {
            if (col.Tag?.ToString() == tag)
            {
                col.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                break;
            }
        }

        // 2. Header and Filter ColumnDefinitions
        if (_syncInfo.TryGetValue(tag, out var info))
        {
            var width = visible ? info.NaturalWidth : new GridLength(0);
            var minW = visible ? info.NaturalMinWidth : 0;
            var maxW = visible ? double.PositiveInfinity : 0;

            if (info.HeaderColumn != null)
            {
                info.HeaderColumn.Width = width;
                info.HeaderColumn.MinWidth = minW;
                info.HeaderColumn.MaxWidth = maxW;
            }
            if (info.FilterColumn != null)
            {
                info.FilterColumn.Width = width;
                info.FilterColumn.MinWidth = minW;
                info.FilterColumn.MaxWidth = maxW;
            }
        }
    }
}

public class SmartColumnSyncInfo
{
    public string Tag { get; set; } = string.Empty;
    public TextBlock? SortIcon { get; set; }
    public ColumnDefinition? HeaderColumn { get; set; }
    public ColumnDefinition? FilterColumn { get; set; }
    public GridLength NaturalWidth { get; set; } = new GridLength(1, GridUnitType.Star);
    public double NaturalMinWidth { get; set; } = 0;
}
