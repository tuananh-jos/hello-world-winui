using App7.Domain.Entities;
using App7.Domain.Usecases;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Views.Dialogs;

public sealed partial class ReturnDialog : ContentDialog
{
    private readonly ReturnDeviceUseCase _returnUseCase;
    private Device? _device;

    public bool Confirmed { get; private set; }

    public ReturnDialog(ReturnDeviceUseCase returnUseCase)
    {
        _returnUseCase = returnUseCase;
        InitializeComponent();
    }

    public void Init(Device device)
    {
        _device              = device;
        DialogTitleText.Text = $"Return Device — {device.ModelName}";
        ModelNameBox.Text    = device.ModelName;
        IMEIBox.Text         = device.IMEI;
        SerialLabBox.Text    = device.SerialLab;
        SerialNumberBox.Text = device.SerialNumber;
        CircuitSerialBox.Text= device.CircuitSerialNumber;
        HWVersionBox.Text    = device.HWVersion;
    }

    private void OnCloseClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Confirmed = false;
        Hide();
    }

    private void OnCancelClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Confirmed = false;
        Hide();
    }

    private async void OnConfirmClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_device == null) return;

        ConfirmBtn.IsEnabled = false;
        CancelBtn.IsEnabled  = false;

        try
        {
            await _returnUseCase.ExecuteAsync(_device.Id, _device.ModelId);
            Confirmed = true;
            Hide();
        }
        catch (Exception ex)
        {
            ConfirmBtn.IsEnabled = true;
            CancelBtn.IsEnabled  = true;

            var errorDialog = new ContentDialog
            {
                Title           = "Return failed",
                Content         = ex.Message,
                CloseButtonText = "OK",
                XamlRoot        = XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
