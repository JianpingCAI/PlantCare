using PlantCare.App.Views;

namespace PlantCare.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("plant", typeof(PlantDetailView));
        }
    }
}