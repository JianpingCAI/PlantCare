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
        }
    }
}