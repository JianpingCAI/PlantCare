﻿using PlantCare.App.Views;
using System.Diagnostics;

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
            Routing.RegisterRoute("reminders", typeof(ReminderView));

            Routing.RegisterRoute("calendar", typeof(ReminderCalendarView));

        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            pageTitle.Text = Current.CurrentPage.Title;
        }
    }
}