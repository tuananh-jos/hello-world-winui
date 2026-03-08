using App7.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Controls;

[Microsoft.UI.Xaml.Markup.ContentProperty(Name = "TableContent")]
public sealed partial class PageLayoutTemplate : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(PageLayoutTemplate), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(PagedListViewModelBase), typeof(PageLayoutTemplate), new PropertyMetadata(null));

    public static readonly DependencyProperty TableContentProperty = DependencyProperty.Register(
        nameof(TableContent), typeof(object), typeof(PageLayoutTemplate), new PropertyMetadata(null));

    public static readonly DependencyProperty OverlaysProperty = DependencyProperty.Register(
        nameof(Overlays), typeof(object), typeof(PageLayoutTemplate), new PropertyMetadata(null));

    public static readonly DependencyProperty TableMinWidthProperty = DependencyProperty.Register(
        nameof(TableMinWidth), typeof(double), typeof(PageLayoutTemplate), new PropertyMetadata(800.0));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public PagedListViewModelBase ViewModel
    {
        get => (PagedListViewModelBase)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public object TableContent
    {
        get => GetValue(TableContentProperty);
        set => SetValue(TableContentProperty, value);
    }

    public object Overlays
    {
        get => GetValue(OverlaysProperty);
        set => SetValue(OverlaysProperty, value);
    }

    public double TableMinWidth
    {
        get => (double)GetValue(TableMinWidthProperty);
        set => SetValue(TableMinWidthProperty, value);
    }

    public PageLayoutTemplate()
    {
        this.InitializeComponent();
    }
}
