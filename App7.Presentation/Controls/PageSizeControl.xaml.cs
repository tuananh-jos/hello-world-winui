using App7.Presentation.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace App7.Presentation.Controls;

public sealed partial class PageSizeControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(PagedListViewModelBase),
        typeof(PageSizeControl),
        new PropertyMetadata(null));

    public PagedListViewModelBase? ViewModel
    {
        get => (PagedListViewModelBase?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public PageSizeControl()
    {
        InitializeComponent();
    }

    private void OnPageSizeFilterClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;
        PopulatePageSizeList();
        
        var transform = PageSizeFilterBtn.TransformToVisual(this);
        var pt = transform.TransformPoint(new Point(0, PageSizeFilterBtn.ActualHeight + 2));
        PageSizePopup.HorizontalOffset = pt.X;
        PageSizePopup.VerticalOffset = pt.Y;
        PageSizePopup.IsOpen = true;
    }

    private void PopulatePageSizeList()
    {
        if (ViewModel == null) return;
        PageSizeListPanel.Children.Clear();
        foreach (var size in ViewModel.PageSizeOptions)
        {
            var captured = size;
            var isSelected = ViewModel.SelectedPageSize == captured;
            var btn = new Button
            {
                Content = size.ToString(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Background = isSelected
                    ? new SolidColorBrush(Color.FromArgb(255, 0xE8, 0xF4, 0xF8))
                    : new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(8, 4, 8, 4),
                FontSize = 12,
            };
            btn.Click += async (_, _) =>
            {
                ViewModel.SelectedPageSize = captured;
                PageSizePopup.IsOpen = false;
                if (ViewModel.ApplyFiltersCommand.CanExecute(null))
                    await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            };
            PageSizeListPanel.Children.Add(btn);
        }
    }
}
