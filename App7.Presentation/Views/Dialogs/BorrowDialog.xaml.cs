using App7.Domain.Entities;
using App7.Domain.Usecases;
using App7.Domain.Dtos;
using App7.Presentation.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Views.Dialogs;

public sealed partial class BorrowDialog : ContentDialog
{
    public BorrowDialogViewModel ViewModel { get; }

    private readonly BorrowDeviceUseCase _borrowUseCase;

    public BorrowDialog(BorrowDeviceUseCase borrowUseCase)
    {
        _borrowUseCase = borrowUseCase;
        ViewModel = new BorrowDialogViewModel();
        InitializeComponent();
    }

    /// <summary>
    /// Call this before showing the dialog to bind a specific model.
    /// </summary>
    public void Init(Model model)
    {
        ViewModel.Init(model);
        DialogTitleText.Text = $"Borrow Model {model.Name}";
    }

    // ── Button handlers ───────────────────────────────────────────────

    private void OnCloseClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.Cancel();
        Hide();
    }

    private void OnCancelClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.Cancel();
        Hide();
    }

    private async void OnOkClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        OkBtn.IsEnabled = false;
        CancelBtn.IsEnabled = false;

        try
        {
            await _borrowUseCase.ExecuteAsync(new BorrowDeviceRequest(ViewModel.ModelId, ViewModel.SelectedQuantity));
            ViewModel.Confirm();
            Hide();
        }
        catch (Exception ex)
        {
            // Show inline error — restore buttons so user can retry or cancel
            OkBtn.IsEnabled = true;
            CancelBtn.IsEnabled = true;

            var errorDialog = new ContentDialog
            {
                Title = "Borrow failed",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
