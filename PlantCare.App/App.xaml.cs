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

            MainPage = new AppShell();
            //MainPage = new NavigationPage(serviceProvider.GetRequiredService<HomeView>());

            // Add navigation event handlers
            RegisterNavigationEventHandlers();
        }

        private void RegisterNavigationEventHandlers()
        {
            if (Shell.Current != null)
            {
                Shell.Current.Navigating += (sender, args) => {
                    Debug.WriteLine("?????????????Navigating to: " + args.Target.Location.ToString());
                };

                Shell.Current.Navigated += (sender, args) => {
                    Debug.WriteLine("?????????????Navigated to: " + args.Current.Location.ToString());
                };
            }
        }
    }
}