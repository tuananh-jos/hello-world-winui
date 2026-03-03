using App7.Data.DataSource;
using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Data.Repository;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using App7.Presentation.Activation;
using App7.Presentation.Contracts.Services;
using App7.Presentation.Helpers;
using App7.Presentation.Services;
using App7.Presentation.ViewModels;
using App7.Presentation.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

namespace App7.Presentation;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // UseCases
            services.AddTransient<GetModelsPagedUseCase>();
            services.AddTransient<GetModelFiltersUseCase>();
            services.AddTransient<BorrowDeviceUseCase>();
            services.AddTransient<GetBorrowedDevicesUseCase>();
            services.AddTransient<ReturnDeviceUseCase>();

            // Repository — Transient (matches ViewModel and UseCase lifetimes; single-user desktop app)
            services.AddTransient<IDeviceRepository, DeviceRepository>();
            services.AddTransient<IModelRepository, ModelRepository>();

            // DataSource — Transient
            services.AddTransient<IDeviceDataSource, DeviceDataSource>();
            services.AddTransient<IModelDataSource, ModelDataSource>();

            // DbContext — Transient (each operation gets a fresh context; safe for single-user desktop)
            services.AddDbContext<AppDbContext>(options =>
            {
                var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "app.db");
                options.UseSqlite($"Data Source={dbPath}");
            }, ServiceLifetime.Transient);

            // DB initializer
            services.AddTransient<DatabaseInitializer>();


            // Views and ViewModels
            services.AddTransient<ModelListViewModel>();
            services.AddTransient<ModelListPage>();
            services.AddTransient<MyDevicesViewModel>();
            services.AddTransient<MyDevicesPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        using var scope = Host.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var fullPath = Path.GetFullPath(context.Database.GetDbConnection().DataSource);
        System.Diagnostics.Debug.WriteLine("FULL DB PATH: " + fullPath);

        await context.Database.EnsureCreatedAsync();

        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
