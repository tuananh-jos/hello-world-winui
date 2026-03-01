using App7.Presentation.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
