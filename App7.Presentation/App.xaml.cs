using App7.Data.DataSource;
using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Data.Repository;
using App7.Data.Services;
using App7.Domain.IRepository;
using App7.Domain.Services;
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

public partial class App : Application
{
    public IHost Host { get; }

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();
    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                // Activation
                services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

                // Services
                services.AddTransient<INavigationViewService, NavigationViewService>();
                services.AddSingleton<IActivationService, ActivationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();

                // ── In-Memory Store (Singleton) ─────────────────────────────
                services.AddSingleton<IInMemoryStore, InMemoryStore>();

                // ── InstanceSyncService (Singleton) ─────────────────────────
                services.AddSingleton<IInstanceSyncService>(sp =>
                {
                    var folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                    return new InstanceSyncService(folder);
                });

                // ── UseCases ────────────────────────────────────────────────
                services.AddTransient<LoadAllModelsUseCase>();
                services.AddTransient<LoadAllDevicesUseCase>();
                services.AddTransient<BorrowDeviceUseCase>();
                services.AddTransient<ReturnDeviceUseCase>();
                // Legacy — kept in case needed by future features
                services.AddTransient<GetModelsPagedUseCase>();
                services.AddTransient<GetModelFiltersUseCase>();
                services.AddTransient<GetBorrowedDevicesUseCase>();

                // ── Repositories ────────────────────────────────────────────
                services.AddTransient<IDeviceRepository, DeviceRepository>();
                services.AddTransient<IModelRepository, ModelRepository>();

                // ── DataSources ─────────────────────────────────────────────
                services.AddTransient<IDeviceDataSource, DeviceDataSource>();
                services.AddTransient<IModelDataSource, ModelDataSource>();

                // ── DbContext (Transient) ────────────────────────────────────
                services.AddDbContext<AppDbContext>(options =>
                {
                    var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "app.db");
                    options.UseSqlite($"Data Source={dbPath}");
                }, ServiceLifetime.Transient);

                services.AddTransient<DatabaseInitializer>();

                // ── Views and ViewModels ─────────────────────────────────────
                services.AddTransient<ModelListViewModel>();
                services.AddTransient<ModelListPage>();
                services.AddTransient<MyDevicesViewModel>();
                services.AddTransient<MyDevicesPage>();
                services.AddTransient<ShellPage>();
                services.AddTransient<ShellViewModel>();
            })
            .Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        // ── 1. DB init ─────────────────────────────────────────────────
        using var scope = Host.Services.CreateScope();

        var context  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var fullPath = Path.GetFullPath(context.Database.GetDbConnection().DataSource);
        System.Diagnostics.Debug.WriteLine("FULL DB PATH: " + fullPath);

        await context.Database.EnsureCreatedAsync();

        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();

        // ── 2. Start FileWatcher ───────────────────────────────────────
        App.GetService<IInstanceSyncService>().Start();

        // ── 3. Activate UI (show window immediately — FR36) ───────────
        await App.GetService<IActivationService>().ActivateAsync(args);

        // ── 4. Background chunk loading into IInMemoryStore ───────────
        // Fire-and-forget: loads data while user sees the UI.
        // ViewModels will re-render when StoreChanged fires after MarkLoaded().
        _ = Task.Run(async () =>
        {
            try
            {
                await LoadAllDataIntoStoreAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Background load error: " + ex.Message);
            }
        });
    }

    /// <summary>
    /// Loads all Models then all Devices into IInMemoryStore in 100k-record chunks (FR34, FR35).
    /// Runs on a background thread — UI stays responsive throughout (FR36).
    /// </summary>
    private async Task LoadAllDataIntoStoreAsync()
    {
        var store = App.GetService<IInMemoryStore>();

        // Load Models
        using (var scope = Host.Services.CreateScope())
        {
            var useCase = scope.ServiceProvider.GetRequiredService<LoadAllModelsUseCase>();
            await foreach (var chunk in useCase.ExecuteAsync(chunkSize: 100_000))
            {
                store.AddModelChunk(chunk);
            }
        }

        // Load Devices
        using (var scope = Host.Services.CreateScope())
        {
            var useCase = scope.ServiceProvider.GetRequiredService<LoadAllDevicesUseCase>();
            await foreach (var chunk in useCase.ExecuteAsync(chunkSize: 100_000))
            {
                store.AddDeviceChunk(chunk);
            }
        }

        store.MarkLoaded(); // fires StoreChanged → ViewModels re-render
    }
}
