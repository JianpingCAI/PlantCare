using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlantCare.Data.Repositories;
using PlantCare.App.Services;
using PlantCare.App.ViewModels;
using PlantCare.App.Views;
using CommunityToolkit.Maui;
using PlantCare.Data;
using PlantCare.App.Utils;
using Plugin.LocalNotification;
using Serilog;
using PlantCare.Data.Repositories.interfaces;
using PlantCare.App.Services.DBService;
using CommunityToolkit.Maui.Storage;
using PlantCare.App.Services.DataExportImport;
using PlantCare.App.Services.Security;
using PlantCare.App.Services.Accessibility;
using SkiaSharp.Views.Maui.Controls.Hosting;
using LiveChartsCore.SkiaSharpView.Maui;

namespace PlantCare.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Configure Serilog before building
        ConfigureSerilog();

        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseLiveCharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            })
            .UseMauiCommunityToolkit()
            .UseLocalNotification();

        // Configure application services
        ConfigureDatabase(builder);
        ConfigureRepositories(builder);
        ConfigureAppServices(builder);
        ConfigureSecurityServices(builder);
        ConfigureAccessibilityServices(builder);
        ConfigureViewsAndViewModels(builder);
        ConfigureNavigation(builder);
        ConfigureDataServices(builder);
        ConfigureLogging(builder);

        return builder.Build();
    }

    private static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.Maui", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.File(
                ConstantValues.LogFilePath,
                rollingInterval: RollingInterval.Month,
                shared: true,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static void ConfigureDatabase(MauiAppBuilder builder)
    {
        string dbPath = Path.Combine(
            Microsoft.Maui.Storage.FileSystem.AppDataDirectory,
            ConstStrings.DatabaseFileName);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseSqlite($"Data Source={dbPath}")
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
        );
    }

    private static void ConfigureRepositories(MauiAppBuilder builder)
    {
        builder.Services.AddScoped<IPlantRepository, PlantRepository>();
        builder.Services.AddScoped<IWateringHistoryRepository, WateringHistoryRepository>();
        builder.Services.AddScoped<IFertilizationHistoryRepository, FertilizationHistoryRepository>();
    }

    private static void ConfigureAppServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IPlantService, PlantService>();
        builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();
        builder.Services.AddSingleton<IImageOptimizationService, ImageOptimizationService>();
    }

    private static void ConfigureSecurityServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
    }

    private static void ConfigureAccessibilityServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IAccessibilityService, AccessibilityService>();
    }

    private static void ConfigureViewsAndViewModels(MauiAppBuilder builder)
    {
        // Singleton Views/ViewModels (long-lived, shared state)
        builder.Services
            .AddSingleton<PlantOverviewView>()
            .AddSingleton<PlantListOverviewViewModel>();

        builder.Services
            .AddSingleton<PlantDetailView>()
            .AddSingleton<PlantDetailViewModel>();

        builder.Services
            .AddSingleton<SettingsView>()
            .AddSingleton<SettingsViewModel>();

        // Transient Views/ViewModels (new instance each time)
        builder.Services
            .AddTransient<PlantAddEditView>()
            .AddTransient<PlantAddEditViewModel>();

        builder.Services
            .AddTransient<PlantCalendarView>()
            .AddTransient<PlantCalendarViewModel>();

        builder.Services
            .AddTransient<CareHistoryView>()
            .AddTransient<CareHistoryViewModel>();

        builder.Services
            .AddTransient<SingePlantCareHistoryView>()
            .AddTransient<SinglePlantCareHistoryViewModel>();

        builder.Services
            .AddTransient<LogViewerPage>()
            .AddTransient<LogViewerViewModel>();

        // Commented out views - uncomment when ready to implement
        // builder.Services
        //     .AddTransient<LoginView>()
        //     .AddTransient<LoginViewModel>();

        // builder.Services
        //     .AddTransient<ReminderView>()
        //     .AddTransient<ReminderViewModel>();

        // builder.Services
        //     .AddTransient<ReminderCalendarView>()
        //     .AddTransient<ReminderCalendarViewModel>();
    }

    private static void ConfigureNavigation(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
    }

    private static void ConfigureDataServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);
        builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(App).Assembly));
        builder.Services.AddTransient<IDataExportService, DataExportService>();
        builder.Services.AddTransient<IDataImportService, DataImportService>();
    }

    private static void ConfigureLogging(MauiAppBuilder builder)
    {
        builder.Logging.ClearProviders();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Logging.AddSerilog(dispose: true);
        builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
    }
}
