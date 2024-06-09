using Microsoft.EntityFrameworkCore;
using PlantCare.App.Utils;
using PlantCare.Data.Repositories;
using System.Diagnostics;

namespace PlantCare.App
{
    public partial class App : Application
    {
        public static Language AppLanguage { get; private set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            //FileHelper.DeleteDatabaseFile();

            // Apply migrations at startup
            using (var scope = serviceProvider.CreateScope())
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
        }

        private static void LoadLocalizationLanguage()
        {
            string? languageString = SecureStorage.GetAsync("Language").Result;
            try
            {
                if (!string.IsNullOrEmpty(languageString))
                {
                    Language language = (Language)Enum.Parse(typeof(Language), languageString, true);

                    LocalizationManager.Instance.SetLanguage(language);

                    AppLanguage = language;
                }
            }
            catch (Exception)
            {
                LocalizationManager.Instance.SetLanguage(Language.English);

                AppLanguage = Language.English;
            }
        }

        private static void LoadTheme()
        {
            AppTheme appTheme = AppTheme.Unspecified;

            if (Current is null)
                return;
            string? strTheme = SecureStorage.GetAsync("AppTheme").Result;
            if (string.IsNullOrEmpty(strTheme))
                return;

            try
            {
                appTheme = (AppTheme)Enum.Parse(typeof(AppTheme), strTheme, true);
            }
            catch (Exception)
            {
            }

            Current.UserAppTheme = appTheme;
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