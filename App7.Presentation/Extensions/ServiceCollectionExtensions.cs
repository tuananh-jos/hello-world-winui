using System.IO;
using App7.Data.DataSource;
using App7.Data.Db;
using App7.Data.IDataSource;
using App7.Data.Repository;
using App7.Domain.IRepository;
using App7.Domain.Usecases;
using App7.Presentation.Activation;
using App7.Presentation.Contracts.Services;
using App7.Presentation.Services;
using App7.Presentation.ViewModels;
using App7.Presentation.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace App7.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // ── Core App Services ──────────────────────────────────────────
        services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();
        services.AddSingleton<IActivationService, ActivationService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<INavigationViewService, NavigationViewService>();

        // ── Database & DbContext ───────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
        {
            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "app.db");
            options.UseSqlite($"Data Source={dbPath}");
            options.LogTo(message => { }, LogLevel.None);
        }, ServiceLifetime.Transient);

        services.AddTransient<DatabaseInitializer>();

        // ── Repositories (Data Layer) ──────────────────────────────────
        services.AddTransient<IDeviceRepository, DeviceRepository>();
        services.AddTransient<IModelRepository, ModelRepository>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();

        // ── DataSources (Data Layer) ───────────────────────────────────
        services.AddTransient<IDeviceDataSource, DeviceDataSource>();
        services.AddTransient<IModelDataSource, ModelDataSource>();

        // ── Sync Service (Presentation Service) ────────────────────────
        services.AddSingleton<IInstanceSyncService>(sp =>
        {
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return new InstanceSyncService(folder);
        });

        // ── UseCases (Domain Layer) ────────────────────────────────────
        services.AddTransient<GetModelsPagedUseCase>();
        services.AddTransient<GetModelFiltersUseCase>();
        services.AddTransient<BorrowDeviceUseCase>();
        services.AddTransient<GetBorrowedDevicesUseCase>();
        services.AddTransient<ReturnDeviceUseCase>();

        // ── Views & ViewModels (Presentation Layer) ────────────────────
        services.AddTransient<ModelListViewModel>();
        services.AddTransient<ModelListPage>();
        services.AddTransient<MyDevicesViewModel>();
        services.AddTransient<MyDevicesPage>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<ShellPage>();

        return services;
    }
}
