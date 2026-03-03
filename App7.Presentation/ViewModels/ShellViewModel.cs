using App7.Presentation.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;

namespace App7.Presentation.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool _isBackEnabled;

    [ObservableProperty]
    private bool _isSidebarOpen = true;

    // Tracks which page is currently active (for sidebar highlight)
    [ObservableProperty]
    private string _currentPageKey = string.Empty;

    public INavigationService NavigationService { get; }

    public ShellViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
    }

    // ── Sidebar toggle ────────────────────────────────────────────────
    [RelayCommand]
    private void ToggleSidebar() => IsSidebarOpen = !IsSidebarOpen;

    // ── Nav commands ──────────────────────────────────────────────────
    [RelayCommand]
    private void NavigateToModels()
        => NavigationService.NavigateTo(typeof(ModelListViewModel).FullName!);

    [RelayCommand]
    private void NavigateToMyDevices()
        => NavigationService.NavigateTo(typeof(MyDevicesViewModel).FullName!);

    // ── Back ──────────────────────────────────────────────────────────
    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        // Update current page key for sidebar selection highlight
        CurrentPageKey = e.SourcePageType?.FullName ?? string.Empty;
    }
}
