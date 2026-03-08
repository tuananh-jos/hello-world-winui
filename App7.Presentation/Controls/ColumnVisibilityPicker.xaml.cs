using App7.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace App7.Presentation.Controls;

public sealed partial class ColumnVisibilityPicker : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(PagedListViewModelBase), typeof(ColumnVisibilityPicker), new PropertyMetadata(null));

    public PagedListViewModelBase ViewModel
    {
        get => (PagedListViewModelBase)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public ColumnVisibilityPicker()
    {
        this.InitializeComponent();
    }
}
