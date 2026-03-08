using App7.Presentation.Helpers;
using App7.Presentation.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace App7.Presentation.Views;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    private readonly SolidColorBrush _selectedBrush;
    private readonly SolidColorBrush _normalBrush;

    private const double SidebarBreakpoint = 900;

    // ── x:Bind properties for sidebar selection highlight ─────────────
    public Brush NavModelsBg    => GetNavItemBg("ModelList");
    public Brush NavMyDevicesBg => GetNavItemBg("MyDevices");

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;

        _selectedBrush = (SolidColorBrush)Application.Current.Resources["AppSidebarSelectedBrush"];
        _normalBrush   = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

        InitializeComponent();

        // Wire navigation frame
        ViewModel.NavigationService.Frame = NavigationFrame;

        // Re-evaluate nav highlight on every page change
        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.CurrentPageKey))
                Bindings.Update();
        };

        // Custom title bar
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();

        // Auto close/open sidebar based on window width
        App.MainWindow.SizeChanged += OnWindowSizeChanged;
    }

    private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        ViewModel.IsSidebarOpen = args.Size.Width >= SidebarBreakpoint;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    // ── Sidebar selection helper ──────────────────────────────────────
    private Brush GetNavItemBg(string pageKeyFragment)
        => ViewModel.CurrentPageKey.Contains(pageKeyFragment)
            ? _selectedBrush
            : _normalBrush;

    // ── Keyboard back nav ─────────────────────────────────────────────
    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var ka = new KeyboardAccelerator { Key = key };
        if (modifiers.HasValue) ka.Modifiers = modifiers.Value;
        ka.Invoked += OnKeyboardAcceleratorInvoked;
        return ka;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var nav = App.GetService<App7.Presentation.Contracts.Services.INavigationService>();
        args.Handled = nav.GoBack();
    }
}
