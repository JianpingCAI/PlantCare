using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantDetailView : ContentPage
{
    public PlantDetailView(PlantDetailViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}