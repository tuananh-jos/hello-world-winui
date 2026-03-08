using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Presentation.ViewModels;
using App7.Presentation.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Views;

public sealed partial class ModelListPage : Page
{
    public ModelListViewModel ViewModel { get; }
    private readonly BorrowDeviceUseCase _borrowUseCase;

    public ModelListPage()
    {
        ViewModel      = App.GetService<ModelListViewModel>();
        _borrowUseCase = App.GetService<BorrowDeviceUseCase>();
        InitializeComponent();

        foreach (var col in ViewModel.ColumnVisibilities)
            col.PropertyChanged += (_, _) => ModelsTable.SyncColumnVisibility(col.ColumnTag, col.IsVisible);
    }

    // ── Borrow ────────────────────────────────────────────────────────
    private async void OnBorrowClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Model model) return;

        var dialog = new BorrowDialog(_borrowUseCase) { XamlRoot = XamlRoot };
        dialog.Init(model);
        await dialog.ShowAsync();

        if (dialog.ViewModel.Confirmed)
        {
            ShowInfoBar(InfoBarSeverity.Success,
                $"Borrowed {dialog.ViewModel.SelectedQuantity} device(s) from \"{model.Name}\" successfully.");
        }
    }

    // ── InfoBar ───────────────────────────────────────────────────────
    private void ShowInfoBar(InfoBarSeverity severity, string message)
    {
        BorrowInfoBar.Severity = severity;
        BorrowInfoBar.Message  = message;
        BorrowInfoBar.IsOpen   = true;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (_, _) => { BorrowInfoBar.IsOpen = false; timer.Stop(); };
        timer.Start();
    }

}
