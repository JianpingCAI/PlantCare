using Microsoft.EntityFrameworkCore;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.Data.Repositories;
using System.Diagnostics;

namespace PlantCare.App
{
    public partial class App : Application
    {
        private readonly IAppSettingsService _settingsService;
        private readonly IServiceProvider _serviceProvider;

        public static Language AppLanguage { get; private set; }

        public App(IServiceProvider serviceProvider, IAppSettingsService settingsService)
        {
            InitializeComponent();

            _settingsService = settingsService;
            _serviceProvider = serviceProvider;

            //FileHelper.DeleteDatabaseFile();

            // Apply migrations at startup
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            LoadTheme();

            LoadLocalizationLanguage();

            // Initialize main page
            MainPage = new AppShell();

            // Add navigation event handlers
            RegisterNavigationEventHandlers();

            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            
            // Validate and regenerate thumbnails on startup (async fire-and-forget)
            _ = ValidateThumbnailsOnStartupAsync();
        }

        /// <summary>
        /// Validates thumbnails on app startup and regenerates missing ones.
        /// This runs asynchronously without blocking app startup.
        /// </summary>
        private async Task ValidateThumbnailsOnStartupAsync()
        {
            try
            {
                // Small delay to let app finish initializing
                await Task.Delay(2000);

                using var scope = _serviceProvider.CreateScope();
                var plantService = scope.ServiceProvider.GetRequiredService<IPlantService>();

                int regeneratedCount = await plantService.ValidateAndRegenerateThumbnailsAsync();

                if (regeneratedCount > 0)
                {
                    Debug.WriteLine($"[Thumbnail Health Check] Regenerated {regeneratedCount} missing thumbnails");
                }
                else
                {
                    Debug.WriteLine("[Thumbnail Health Check] All thumbnails are valid");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Thumbnail Health Check] Failed: {ex.Message}");
                // Don't crash the app if thumbnail validation fails
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            if (window?.Page == null)
            {
                window!.Page = new AppShell();
            }
            return window;
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
                var current = Shell.Current;
                if (current != null)
                {
                    await current.DisplayAlert("Error", $"An unexpected error occurred. Please try again later: {ex.Message}.", "OK");
                }
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
                else
                {
                    Current.UserAppTheme = AppTheme.Light;
                }
            }
            catch (Exception ex)
            {
                if (Current is not null)
                    Current.UserAppTheme = AppTheme.Light;
                Debug.WriteLine($"Exception occurs: {ex.Message}");
            }
        }

        private static void RegisterNavigationEventHandlers()
        {
            var current = Shell.Current;
            if (current != null)
            {
                current.Navigating += (sender, args) =>
                {
                    Debug.WriteLine("Navigating to: " + args.Target.Location.ToString());
                };

                current.Navigated += (sender, args) =>
                {
                    Debug.WriteLine("Navigated to: " + args.Current.Location.ToString());
                };
            }
        }
    }
}
