using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantOverviewView : ContentPageBase
{
    public PlantOverviewView(PlantListOverviewViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}