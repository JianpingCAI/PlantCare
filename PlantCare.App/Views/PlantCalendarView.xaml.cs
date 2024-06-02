using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantCalendarView : ContentPageBase
{
    public PlantCalendarView(PlantCalendarViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}