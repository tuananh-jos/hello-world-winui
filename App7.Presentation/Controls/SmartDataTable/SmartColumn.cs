using Microsoft.UI.Xaml;
using System.Windows.Input;

namespace App7.Presentation.Controls;

public enum SmartFilterType
{
    None,
    Text,
    Dropdown,
    ClearButton,
    Placeholder
}

public class SmartColumn : DependencyObject
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(SmartColumn), new PropertyMetadata(string.Empty));
    public string Header { get => (string)GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }

    public static readonly DependencyProperty TagProperty = DependencyProperty.Register(
        nameof(Tag), typeof(string), typeof(SmartColumn), new PropertyMetadata(string.Empty));
    public string Tag { get => (string)GetValue(TagProperty); set => SetValue(TagProperty, value); }

    public static readonly DependencyProperty BindingPathProperty = DependencyProperty.Register(
        nameof(BindingPath), typeof(string), typeof(SmartColumn), new PropertyMetadata(string.Empty));
    public string BindingPath { get => (string)GetValue(BindingPathProperty); set => SetValue(BindingPathProperty, value); }

    public static readonly DependencyProperty IsSortableProperty = DependencyProperty.Register(
        nameof(IsSortable), typeof(bool), typeof(SmartColumn), new PropertyMetadata(true));
    public bool IsSortable { get => (bool)GetValue(IsSortableProperty); set => SetValue(IsSortableProperty, value); }

    public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
        nameof(MinWidth), typeof(double), typeof(SmartColumn), new PropertyMetadata(0.0));
    public double MinWidth { get => (double)GetValue(MinWidthProperty); set => SetValue(MinWidthProperty, value); }

    public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
        nameof(Width), typeof(GridLength), typeof(SmartColumn), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));
    public GridLength Width { get => (GridLength)GetValue(WidthProperty); set => SetValue(WidthProperty, value); }

    // -- Filter Configuration --
    public static readonly DependencyProperty FilterTypeProperty = DependencyProperty.Register(
        nameof(FilterType), typeof(SmartFilterType), typeof(SmartColumn), new PropertyMetadata(SmartFilterType.None));
    public SmartFilterType FilterType { get => (SmartFilterType)GetValue(FilterTypeProperty); set => SetValue(FilterTypeProperty, value); }

    public static readonly DependencyProperty FilterPlaceholderProperty = DependencyProperty.Register(
        nameof(FilterPlaceholder), typeof(string), typeof(SmartColumn), new PropertyMetadata(string.Empty));
    public string FilterPlaceholder { get => (string)GetValue(FilterPlaceholderProperty); set => SetValue(FilterPlaceholderProperty, value); }

    // Used for standard TextBox Text TwoWay binding
    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText), typeof(string), typeof(SmartColumn), new PropertyMetadata(string.Empty));
    public string SearchText { get => (string)GetValue(SearchTextProperty); set => SetValue(SearchTextProperty, value); }

    // -- Dropdown Filter Configuration --
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(object), typeof(SmartColumn), new PropertyMetadata(null));
    public object ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(SmartColumn), new PropertyMetadata(null));
    public object SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

    public static readonly DependencyProperty SearchTextDropdownProperty = DependencyProperty.Register(
        nameof(SearchTextDropdown), typeof(string), typeof(SmartColumn), new PropertyMetadata(string.Empty));
    public string SearchTextDropdown { get => (string)GetValue(SearchTextDropdownProperty); set => SetValue(SearchTextDropdownProperty, value); }

    public static readonly DependencyProperty DropdownMinWidthProperty = DependencyProperty.Register(
        nameof(DropdownMinWidth), typeof(double), typeof(SmartColumn), new PropertyMetadata(160.0));
    public double DropdownMinWidth { get => (double)GetValue(DropdownMinWidthProperty); set => SetValue(DropdownMinWidthProperty, value); }

    public static readonly DependencyProperty AllOptionLabelProperty = DependencyProperty.Register(
        nameof(AllOptionLabel), typeof(string), typeof(SmartColumn), new PropertyMetadata("All"));
    public string AllOptionLabel { get => (string)GetValue(AllOptionLabelProperty); set => SetValue(AllOptionLabelProperty, value); }

    public static readonly DependencyProperty DropdownSelectionChangedCommandProperty = DependencyProperty.Register(
        nameof(DropdownSelectionChangedCommand), typeof(ICommand), typeof(SmartColumn), new PropertyMetadata(null));
    public ICommand DropdownSelectionChangedCommand { get => (ICommand)GetValue(DropdownSelectionChangedCommandProperty); set => SetValue(DropdownSelectionChangedCommandProperty, value); }

    // -- DataGrid Configuration --
    public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register(
        nameof(CellTemplate), typeof(DataTemplate), typeof(SmartColumn), new PropertyMetadata(null));
    public DataTemplate CellTemplate { get => (DataTemplate)GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
}
