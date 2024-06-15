using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class SingePlantCareHistoryView : ContentPage
{
    public SingePlantCareHistoryView(SinglePlantCareHistoryViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}