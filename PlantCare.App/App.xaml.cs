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

        public static void UpdateAppLanguage(Language language)
        {
            AppLanguage = language;
        }

#if DEBUG
        // Set this to true if you want to reset the database on each debug session
        private const bool RESET_DATABASE_ON_DEBUG = false;
#endif

        public App(IServiceProvider serviceProvider, IAppSettingsService settingsService)
        {
            InitializeComponent();

            _settingsService = settingsService;
            _serviceProvider = serviceProvider;

            // Initialize app asynchronously to avoid blocking UI thread
            InitializeAppAsync().SafeFireAndForget(ex =>
            {
                Debug.WriteLine($"[App Initialization] Failed: {ex.Message}");
            });
        }

        /// <summary>
        /// Asynchronously initializes the app without blocking the UI thread
        /// </summary>
        private async Task InitializeAppAsync()
        {
            try
            {
#if DEBUG
                // Optional: Delete database for fresh start during debugging
                if (RESET_DATABASE_ON_DEBUG)
                {
                    await Task.Run(() => FileHelper.DeleteDatabaseFile());
                    Debug.WriteLine("[Database] Database deleted for fresh start (DEBUG mode)");
                }
#endif

                // Apply migrations on background thread
                await Task.Run(() =>
                {
                    using IServiceScope scope = _serviceProvider.CreateScope();
                    ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                    
#if DEBUG
                    // Seed test data for development
                    DevelopmentDataSeeder.SeedTestDataIfNeededAsync(db, 10).GetAwaiter().GetResult();
                    Debug.WriteLine($"[Database] Current plant count: {db.Plants.Count()}");
#endif
                });

                // Load theme asynchronously
                await LoadThemeAsync();

                // Load localization asynchronously
                await LoadLocalizationLanguageAsync();

                // Add navigation event handlers on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    RegisterNavigationEventHandlers();
                });

                // Global exception handling
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
                
                // Validate and regenerate thumbnails on startup (async fire-and-forget)
                _ = ValidateThumbnailsOnStartupAsync();
                
                Debug.WriteLine("[App Initialization] Completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[App Initialization] Error: {ex.Message}");
                Debug.WriteLine($"[App Initialization] Stack Trace: {ex.StackTrace}");
            }
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

                using IServiceScope scope = _serviceProvider.CreateScope();
                IPlantService plantService = scope.ServiceProvider.GetRequiredService<IPlantService>();

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
            return new Window(new AppShell());
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
            MainThread.InvokeOnMainThreadAsync(async () =>
            {
                Shell current = Shell.Current;
                if (current != null)
                {
                    await current.DisplayAlertAsync("Error", $"An unexpected error occurred. Please try again later: {ex.Message}.", "OK");
                }
            });
        }

        private async Task LoadLocalizationLanguageAsync()
        {
            try
            {
                Language language = await _settingsService.GetLanguageAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocalizationManager.Instance.SetLanguage(language);
                    AppLanguage = language;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Localization] Failed to load language: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LocalizationManager.Instance.SetLanguage(Language.English);
                    AppLanguage = Language.English;
                });
            }
        }

        private async Task LoadThemeAsync()
        {
            try
            {
                AppTheme appTheme = await _settingsService.GetThemeSettingAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (Current is not null)
                    {
                        Current.UserAppTheme = appTheme;
                    }
                    else
                    {
                        Current.UserAppTheme = AppTheme.Light;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Theme] Failed to load theme: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (Current is not null)
                    {
                        Current.UserAppTheme = AppTheme.Light;
                    }
                });
            }
        }

        private static void RegisterNavigationEventHandlers()
        {
            Shell current = Shell.Current;
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
