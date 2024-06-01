using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class ReminderCalendarView : ContentPageBase
{
    public ReminderCalendarView(ReminderCalendarViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}