using Microsoft.EntityFrameworkCore;
using PlantCare.Data.Repositories;

namespace PlantCare.App
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Apply migrations at startup
            using (var scope = serviceProvider.CreateScope())
            {
                ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            MainPage = new AppShell();
            //MainPage = new NavigationPage(serviceProvider.GetRequiredService<HomeView>());
        }
    }
}