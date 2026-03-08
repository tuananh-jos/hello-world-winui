using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Windows.Input;
using Windows.UI;

namespace App7.Presentation.Controls;

public sealed partial class SmartDropdownPicker : UserControl
{
    private static readonly Brush DefaultLabelFg = new SolidColorBrush(Color.FromArgb(255, 0x88, 0x88, 0x88));
    private static readonly Brush ActiveLabelFg = new SolidColorBrush(Colors.Black);

    public static readonly DependencyProperty ButtonLabelProperty = DependencyProperty.Register(
        nameof(ButtonLabel), typeof(string), typeof(SmartDropdownPicker), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ButtonLabelForegroundProperty = DependencyProperty.Register(
        nameof(ButtonLabelForeground), typeof(Brush), typeof(SmartDropdownPicker), new PropertyMetadata(DefaultLabelFg));

    public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
        nameof(ButtonStyle), typeof(Style), typeof(SmartDropdownPicker), new PropertyMetadata(null, OnButtonStyleChanged));

    public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register(
        nameof(ShowSearchBox), typeof(bool), typeof(SmartDropdownPicker), new PropertyMetadata(false, OnShowSearchBoxChanged));

    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText), typeof(string), typeof(SmartDropdownPicker), new PropertyMetadata(string.Empty, OnSearchTextChanged));

    public static readonly DependencyProperty ShowAllOptionProperty = DependencyProperty.Register(
        nameof(ShowAllOption), typeof(bool), typeof(SmartDropdownPicker), new PropertyMetadata(false, OnShowAllOptionChanged));

    public static readonly DependencyProperty AllOptionLabelProperty = DependencyProperty.Register(
        nameof(AllOptionLabel), typeof(string), typeof(SmartDropdownPicker), new PropertyMetadata("All"));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(object), typeof(SmartDropdownPicker), new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(SmartDropdownPicker), new PropertyMetadata(null, OnItemTemplateChanged));

    public static readonly DependencyProperty CloseOnSelectionProperty = DependencyProperty.Register(
        nameof(CloseOnSelection), typeof(bool), typeof(SmartDropdownPicker), new PropertyMetadata(true));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(SmartDropdownPicker), new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register(
        nameof(SelectionChangedCommand), typeof(ICommand), typeof(SmartDropdownPicker), new PropertyMetadata(null));

    public static readonly DependencyProperty AutoUpdateLabelProperty = DependencyProperty.Register(
        nameof(AutoUpdateLabel), typeof(bool), typeof(SmartDropdownPicker), new PropertyMetadata(true, OnSelectedItemChanged));

    public static readonly DependencyProperty ShowDropdownIconProperty = DependencyProperty.Register(
        nameof(ShowDropdownIcon), typeof(bool), typeof(SmartDropdownPicker), new PropertyMetadata(true, OnShowDropdownIconChanged));

    public static readonly DependencyProperty PickerOverlayModeProperty = DependencyProperty.Register(
        nameof(PickerOverlayMode), typeof(LightDismissOverlayMode), typeof(SmartDropdownPicker), new PropertyMetadata(LightDismissOverlayMode.Auto));

    public static readonly DependencyProperty DropdownMinWidthProperty = DependencyProperty.Register(
        nameof(DropdownMinWidth), typeof(double), typeof(SmartDropdownPicker), new PropertyMetadata(160.0));

    public static readonly DependencyProperty MatchButtonWidthProperty = DependencyProperty.Register(
        nameof(MatchButtonWidth), typeof(bool), typeof(SmartDropdownPicker), new PropertyMetadata(true));

    public string ButtonLabel { get => (string)GetValue(ButtonLabelProperty); set => SetValue(ButtonLabelProperty, value); }
    public Brush ButtonLabelForeground { get => (Brush)GetValue(ButtonLabelForegroundProperty); set => SetValue(ButtonLabelForegroundProperty, value); }
    public Style ButtonStyle { get => (Style)GetValue(ButtonStyleProperty); set => SetValue(ButtonStyleProperty, value); }
    public bool ShowSearchBox { get => (bool)GetValue(ShowSearchBoxProperty); set => SetValue(ShowSearchBoxProperty, value); }
    public string SearchText { get => (string)GetValue(SearchTextProperty); set => SetValue(SearchTextProperty, value); }
    public bool ShowAllOption { get => (bool)GetValue(ShowAllOptionProperty); set => SetValue(ShowAllOptionProperty, value); }
    public string AllOptionLabel { get => (string)GetValue(AllOptionLabelProperty); set => SetValue(AllOptionLabelProperty, value); }
    public object ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public DataTemplate ItemTemplate { get => (DataTemplate)GetValue(ItemTemplateProperty); set => SetValue(ItemTemplateProperty, value); }
    public bool CloseOnSelection { get => (bool)GetValue(CloseOnSelectionProperty); set => SetValue(CloseOnSelectionProperty, value); }
    public object SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
    public ICommand SelectionChangedCommand { get => (ICommand)GetValue(SelectionChangedCommandProperty); set => SetValue(SelectionChangedCommandProperty, value); }
    public bool AutoUpdateLabel { get => (bool)GetValue(AutoUpdateLabelProperty); set => SetValue(AutoUpdateLabelProperty, value); }
    public bool ShowDropdownIcon { get => (bool)GetValue(ShowDropdownIconProperty); set => SetValue(ShowDropdownIconProperty, value); }
    public LightDismissOverlayMode PickerOverlayMode { get => (LightDismissOverlayMode)GetValue(PickerOverlayModeProperty); set => SetValue(PickerOverlayModeProperty, value); }
    public double DropdownMinWidth { get => (double)GetValue(DropdownMinWidthProperty); set => SetValue(DropdownMinWidthProperty, value); }
    public bool MatchButtonWidth { get => (bool)GetValue(MatchButtonWidthProperty); set => SetValue(MatchButtonWidthProperty, value); }

    public Visibility SearchBoxVisibility => ShowSearchBox ? Visibility.Visible : Visibility.Collapsed;
    public Visibility AllOptionVisibility => ShowAllOption ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DropdownIconVisibility => ShowDropdownIcon ? Visibility.Visible : Visibility.Collapsed;

    public SmartDropdownPicker()
    {
        this.InitializeComponent();
        Loaded += (_, _) => 
        {
            UpdateLabel();
            UpdateItemTemplate();
        };
    }

    private static void OnButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SmartDropdownPicker picker && e.NewValue is Style style)
            picker.MainButton.Style = style;
    }

    private static void OnShowSearchBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SmartDropdownPicker)d).Bindings.Update();

    private static void OnShowAllOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SmartDropdownPicker)d).Bindings.Update();

    private static void OnShowDropdownIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((SmartDropdownPicker)d).Bindings.Update();

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SmartDropdownPicker picker)
            picker.ApplySearchFilter();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SmartDropdownPicker picker)
            picker.ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        if (ItemsSource is System.Collections.IEnumerable enumerable)
        {
            var filter = SearchText?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(filter))
            {
                ItemsList.ItemsSource = enumerable;
            }
            else
            {
                var filtered = new System.Collections.Generic.List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null && item.ToString()!.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
                    {
                        filtered.Add(item);
                    }
                }
                ItemsList.ItemsSource = filtered;
            }
        }
        else
        {
            ItemsList.ItemsSource = null;
        }
    }

    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SmartDropdownPicker picker)
            picker.UpdateItemTemplate();
    }

    private void UpdateItemTemplate()
    {
        if (ItemTemplate != null)
        {
            ItemsList.ItemTemplate = ItemTemplate;
        }
        else if (Resources.TryGetValue("DefaultButtonItemTemplate", out var defaultTemplate) && defaultTemplate is DataTemplate template)
        {
            ItemsList.ItemTemplate = template;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SmartDropdownPicker picker)
            picker.UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (!AutoUpdateLabel) return;

        if (SelectedItem != null)
        {
            ButtonLabel = SelectedItem.ToString() ?? AllOptionLabel;
            ButtonLabelForeground = ActiveLabelFg;
        }
        else
        {
            ButtonLabel = AllOptionLabel;
            ButtonLabelForeground = DefaultLabelFg;
        }
    }

    private void OnAllOptionClicked(object sender, RoutedEventArgs e)
    {
        SelectedItem = null;
        if (CloseOnSelection) MainFlyout.Hide();
        SelectionChangedCommand?.Execute(null);
    }

    private void OnDefaultItemButtonClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && CloseOnSelection)
        {
            SelectedItem = btn.DataContext;
            MainFlyout.Hide();
            SelectionChangedCommand?.Execute(null);
        }
    }

    private void OnMainFlyoutOpened(object sender, object e)
    {
        var presenter = FindParent<FlyoutPresenter>(PopupRootGrid);
        if (presenter != null)
        {
            if (MatchButtonWidth)
            {
                presenter.MinWidth = MainButton.ActualWidth;
                presenter.MaxWidth = MainButton.ActualWidth;
            }
            else
            {
                presenter.MinWidth = DropdownMinWidth;
                presenter.ClearValue(FrameworkElement.MaxWidthProperty);
            }
        }
    }

    private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
    {
        var parent = child;
        while (parent != null)
        {
            if (parent is T t) return t;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }
}
