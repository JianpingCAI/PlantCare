using PlantCare.App.Utils;
using PlantCare.App.ViewModels;
using PlantCare.App.Views;

namespace PlantCare.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(PageName.Overview, typeof(PlantOverviewView));
            Routing.RegisterRoute(PageName.Plant, typeof(PlantDetailView));
            Routing.RegisterRoute(PageName.Edit, typeof(PlantAddEditView));
            Routing.RegisterRoute(PageName.Add, typeof(PlantAddEditView));

            Routing.RegisterRoute(PageName.Calendar, typeof(PlantCalendarView));

            Routing.RegisterRoute(PageName.SinglePlantCareHistory, typeof(SingePlantCareHistoryView));

            Routing.RegisterRoute(PageName.About, typeof(AboutPage));

            Routing.RegisterRoute(PageName.LogViewer, typeof(LogViewerPage));

            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            pageTitle.Text = Current.CurrentPage.Title;
        }

        protected override void OnDisappearing()
        {
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
            base.OnDisappearing();
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            tabHome.Title = LocalizationManager.Instance["Home"];
            tabCalendar.Title = LocalizationManager.Instance["Calendar"];
            tabSettings.Title = LocalizationManager.Instance["Settings"];
            tabHistory.Title = LocalizationManager.Instance["History"];
        }
    }
}