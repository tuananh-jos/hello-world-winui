using App7.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace App7.Presentation.Controls;

public sealed partial class PaginationControl : UserControl
{
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
                oldVm.PropertyChanged -= control.OnViewModelPropertyChanged;

            if (e.NewValue is PagedListViewModelBase newVm)
            {
                newVm.PropertyChanged += control.OnViewModelPropertyChanged;
                control.RebuildAllButtons();
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PagedListViewModelBase.PageNumbers)
                           or nameof(PagedListViewModelBase.CurrentPage))
        {
            RebuildAllButtons();
        }
    }

    private void RebuildAllButtons()
    {
        if (ViewModel == null) return;

        PaginationPanel.Children.Clear();

        // First + Previous
        PaginationPanel.Children.Add(MakeNavButton("First", ViewModel.FirstPageCommand));
        PaginationPanel.Children.Add(MakeNavButton("Previous", ViewModel.PreviousPageCommand));

        // Page number buttons
        foreach (var pageNum in ViewModel.PageNumbers)
        {
            var isActive = pageNum == ViewModel.CurrentPage;
            var btn = new HandButton
            {
                Content = pageNum.ToString(),
                Style = isActive
                    ? (Style)Resources["ActiveNumberButtonStyle"]
                    : (Style)Resources["InactiveNumberButtonStyle"]
            };

            var captured = pageNum;
            btn.Click += async (_, _) => await ViewModel.GoToPageCommand.ExecuteAsync(captured);
            PaginationPanel.Children.Add(btn);
        }

        // Next + Last
        PaginationPanel.Children.Add(MakeNavButton("Next", ViewModel.NextPageCommand));
        PaginationPanel.Children.Add(MakeNavButton("Last", ViewModel.LastPageCommand));
    }

    private static Button MakeNavButton(string label, System.Windows.Input.ICommand command)
    {
        return new HandButton
        {
            Content = label,
            Command = command,
            Style = (Style)Application.Current.Resources["AppNavButtonStyle"]
        };
    }
}
