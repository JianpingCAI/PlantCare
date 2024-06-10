using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class WateringHistoryView : ContentPageBase
{
    public WateringHistoryView(WateringHistoryViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}