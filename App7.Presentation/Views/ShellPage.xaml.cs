using App7.Presentation.Helpers;
using App7.Presentation.ViewModels;
using Microsoft.UI.Input;
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

        // Custom title bar: Row 0 (AppTitleBar) is the drag region.
        // The header bar (Row 1) is separate, so hamburger clicks work correctly.
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    // ── Hamburger cursor ──────────────────────────────────────────────
    // ProtectedCursor is a protected member of UIElement — must be set on
    // 'this' (the Page), not on the button instance directly.
    private void HamburgerBtn_PointerEntered(object sender, PointerRoutedEventArgs e)
        => ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);

    private void HamburgerBtn_PointerExited(object sender, PointerRoutedEventArgs e)
        => ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

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
