using System.Diagnostics;
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
using Microsoft.Extensions.Logging;
using App7.Presentation.Extensions;

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
            services.AddAppServices();
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

        // print db path
        var fullPath = Path.GetFullPath(context.Database.GetDbConnection().DataSource);
        System.Diagnostics.Debug.WriteLine("FULL DB PATH: " + fullPath);

        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        var needsSetup = await initializer.NeedsInitializationAsync();

        SetupWindow? setupWindow = null;

        if (needsSetup)
        {
            // Show splash window during first-time data import
            setupWindow = new SetupWindow();
            setupWindow.Activate();
        }

        // import data if required
        var watch = Stopwatch.StartNew();
        await initializer.InitializeAsync();
        watch.Stop();
        Debug.WriteLine($"Import xong trong: {watch.ElapsedMilliseconds} ms");

        // Start sync watcher after DB is ready
        App.GetService<IInstanceSyncService>().Start();

        // Activate MainWindow FIRST — must have a visible window before closing SetupWindow
        // otherwise WinUI sees 0 windows and terminates the process
        await App.GetService<IActivationService>().ActivateAsync(args);

        if (setupWindow != null)
        {
            setupWindow.Close();
        }
    }
}
