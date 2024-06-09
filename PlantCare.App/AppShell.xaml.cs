using PlantCare.App.Utils;
using PlantCare.App.Views;

namespace PlantCare.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("overview", typeof(PlantOverviewView));
            Routing.RegisterRoute("plant", typeof(PlantDetailView));
            Routing.RegisterRoute("edit", typeof(PlantAddEditView));
            Routing.RegisterRoute("add", typeof(PlantAddEditView));

            Routing.RegisterRoute("calendar", typeof(PlantCalendarView));

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
        }
    }
}