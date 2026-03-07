using App7.Presentation.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using Windows.UI;

namespace App7.Presentation.Controls;

public sealed partial class PaginationControl : UserControl
{
    private static readonly SolidColorBrush ActivePageBrush
        = (SolidColorBrush)Application.Current.Resources["AppOkBrush"];
    private static readonly SolidColorBrush InactivePageBrush = new(Colors.Transparent);
    private static readonly SolidColorBrush InactiveTextBrush = new(Color.FromArgb(255, 0x33, 0x33, 0x33));

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(PagedListViewModelBase),
        typeof(PaginationControl),
        new PropertyMetadata(null, OnViewModelChanged));

    public PagedListViewModelBase? ViewModel
    {
        get => (PagedListViewModelBase?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public PaginationControl()
    {
        InitializeComponent();
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            if (e.OldValue is PagedListViewModelBase oldVm)
            {
                oldVm.PropertyChanged -= control.OnViewModelPropertyChanged;
            }
            if (e.NewValue is PagedListViewModelBase newVm)
            {
                newVm.PropertyChanged += control.OnViewModelPropertyChanged;
                control.RebuildPageNumberButtons();
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PagedListViewModelBase.PageNumbers) or nameof(PagedListViewModelBase.CurrentPage))
        {
            RebuildPageNumberButtons();
        }
    }

    private void RebuildPageNumberButtons()
    {
        if (ViewModel == null) return;

        PageNumbersPanel.Children.Clear();
        foreach (var pageNum in ViewModel.PageNumbers)
        {
            var isActive = pageNum == ViewModel.CurrentPage;
            var btn = new Button
            {
                Content = pageNum.ToString(),
                MinWidth = 32, Height = 32,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(8, 0, 8, 0),
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
}
