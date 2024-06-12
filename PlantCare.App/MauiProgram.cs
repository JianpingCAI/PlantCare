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
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace PlantCare.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "app.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder
            .UseSkiaSharp(registerRenderers: true)
            .UseMauiApp<App>()
            // Initialize the .NET MAUI Community Toolkit by adding the below line of code
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            })
            .UseLocalNotification();

        // Configure services

        // Database context
        string dbPath = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, ConstStrings.DatabaseFileName);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}")
                   .UseLazyLoadingProxies();
        });

        // Repository registrations
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPlantRepository, PlantRepository>();
        builder.Services.AddScoped<IWateringHistoryRepository, WateringHistoryRepository>();
        builder.Services.AddScoped<IFertilizationHistoryRepository, FertilizationHistoryRepository>();

        // Service registrations
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IPlantService, PlantService>();
        builder.Services.AddSingleton<IReminderService, ReminderService>();
        builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();

        // Register Views and ViewModels
        RegisterViewWithViewModels(builder);

        // Register the navigation service
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // Register the dialog service
        builder.Services.AddSingleton<IDialogService, DialogService>();

        // Register AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // Register background service
        //builder.Services.AddHostedService<PlantStateCheckingService>();

        //builder.Services.AddHostedService<TestBackGroundService>();

#if DEBUG
        builder.Logging.AddDebug();

        //LocalNotificationCenter.LogLevel = LogLevel.Debug;
        //builder.Logging.AddConsole();
#endif
        builder.Logging.AddSerilog(dispose: true);
        builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

        return builder.Build();
    }

    private static void RegisterViewWithViewModels(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<PlantOverviewView>();
        builder.Services.AddSingleton<PlantListOverviewViewModel>();

        builder.Services.AddSingleton<PlantDetailView>();
        builder.Services.AddSingleton<PlantDetailViewModel>();

        builder.Services.AddTransient<PlantAddEditView>();
        builder.Services.AddTransient<PlantAddEditViewModel>();

        //builder.Services.AddTransient<LoginViewModel>();

        //builder.Services.AddTransient<ReminderView>();
        //builder.Services.AddTransient<ReminderViewModel>();

        //builder.Services.AddTransient<ReminderCalendarView>();
        //builder.Services.AddTransient<ReminderCalendarViewModel>();

        builder.Services.AddTransient<PlantCalendarView>();
        builder.Services.AddTransient<PlantCalendarViewModel>();

        builder.Services.AddSingleton<SettingsView>();
        builder.Services.AddSingleton<SettingsViewModel>();

        builder.Services.AddTransient<CareHistoryView>();
        builder.Services.AddTransient<CareHistoryViewModel>();
    }
}