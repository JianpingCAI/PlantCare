using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class ReminderView : ContentPageBase
{
    public ReminderView(ReminderViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}