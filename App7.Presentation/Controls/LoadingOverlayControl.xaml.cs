using App7.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Controls;

public sealed partial class LoadingOverlayControl : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(PagedListViewModelBase),
        typeof(LoadingOverlayControl),
        new PropertyMetadata(null));

    public PagedListViewModelBase? ViewModel
    {
        get => (PagedListViewModelBase?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public LoadingOverlayControl()
    {
        InitializeComponent();
    }
}
