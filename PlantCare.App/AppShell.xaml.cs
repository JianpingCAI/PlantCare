using PlantCare.App.Views;

namespace PlantCare.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("plant", typeof(PlantDetailView));
            Routing.RegisterRoute("plant/edit", typeof(PlantAddEditView));
            Routing.RegisterRoute("plant/add", typeof(PlantAddEditView));
        }
    }
}