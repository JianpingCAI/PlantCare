using Microsoft.EntityFrameworkCore;
using PlantCare.App.Services;
using PlantCare.App.Utils;
using PlantCare.Data.Repositories;
using System.Diagnostics;

namespace PlantCare.App
{
    public partial class App : Application
    {
        private readonly IAppSettingsService _settingsService;

        public static Language AppLanguage { get; private set; }

        public App(IServiceProvider serviceProvider, IAppSettingsService settingsService)
        {
            InitializeComponent();

            _settingsService = settingsService;

            //FileHelper.DeleteDatabaseFile();

            // Apply migrations at startup
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            LoadTheme();

            LoadLocalizationLanguage();

            MainPage = new AppShell();
            //MainPage = new NavigationPage(serviceProvider.GetRequiredService<HomeView>());

            // Add navigation event handlers
            RegisterNavigationEventHandlers();

            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            LogException(e.Exception);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            LogException(exception);
        }

        private void LogException(Exception ex)
        {
            // Log exception details here, e.g., to a file or remote server
            Debug.WriteLine($"Unhandled exception: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            // Optionally, display an alert to the user
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                //await Current.MainPage.DisplayAlert("Error", $"An unexpected error occurred. Please try again later: {ex.Message}.", "OK");
                await Shell.Current.DisplayAlert("Error", $"An unexpected error occurred. Please try again later: {ex.Message}.", "OK");
            });
        }

        private void LoadLocalizationLanguage()
        {
            try
            {
                Language language = _settingsService.GetLanguageAsync().Result;
                LocalizationManager.Instance.SetLanguage(language);
                AppLanguage = language;
            }
            catch (Exception)
            {
                LocalizationManager.Instance.SetLanguage(Language.English);
                AppLanguage = Language.English;
            }
        }

        private void LoadTheme()
        {
            try
            {
                AppTheme appTheme = _settingsService.GetThemeSettingAsync().Result;
                if (Current is not null)
                {
                    Current.UserAppTheme = appTheme;
                }
            }
            catch (Exception ex)
            {
                if (Current is not null)
                    Current.UserAppTheme = AppTheme.Unspecified;
                Debug.WriteLine($"?????????? Exception occurs: {ex.Message}");
            }
        }

        private static void RegisterNavigationEventHandlers()
        {
            if (Shell.Current != null)
            {
                Shell.Current.Navigating += (sender, args) =>
                {
                    Debug.WriteLine("?????????????Navigating to: " + args.Target.Location.ToString());
                };

                Shell.Current.Navigated += (sender, args) =>
                {
                    Debug.WriteLine("?????????????Navigated to: " + args.Current.Location.ToString());
                };
            }
        }
    }
}