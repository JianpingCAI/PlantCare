using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlantCare.Data.Repositories;
using PlantCare.App.Services;
using PlantCare.App.ViewModels;
using PlantCare.App.Views;
using CommunityToolkit.Maui;
using PlantCare.Data;
using PlantCare.App.Utils;

namespace PlantCare.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // Initialize the .NET MAUI Community Toolkit by adding the below line of code
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular");
            });

        // Configure services

        // Database context
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, Consts.DatabaseFileName);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Repository registrations
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IPlantRepository, PlantRepository>();
        builder.Services.AddScoped<IReminderRepository, ReminderRepository>();

        // Service registrations
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IPlantService, PlantService>();
        builder.Services.AddSingleton<IReminderService, ReminderService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();

        // Register Views and ViewModels
        registerViewWithViewModels(builder);

        // Register the navigation service
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // Register the dialog service
        builder.Services.AddSingleton<IDialogService, DialogService>();

        // Register AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void registerViewWithViewModels(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<PlantOverviewView>();
        builder.Services.AddSingleton<PlantListOverviewViewModel>();

        builder.Services.AddTransient<PlantDetailView>();
        builder.Services.AddTransient<PlantDetailViewModel>();

        builder.Services.AddTransient<PlantAddEditView>();
        builder.Services.AddTransient<PlantAddEditViewModel>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ReminderViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
    }
}