using Microsoft.EntityFrameworkCore;
using PlantCare.Data.Repositories;
using System.Diagnostics;

namespace PlantCare.App
{
    public partial class App : Application
    {
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

            MainPage = new AppShell();
            //MainPage = new NavigationPage(serviceProvider.GetRequiredService<HomeView>());

            // Add navigation event handlers
            RegisterNavigationEventHandlers();
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