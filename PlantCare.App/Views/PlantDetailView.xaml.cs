using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantDetailView : ContentPageBase
{
    public PlantDetailView(PlantDetailViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is PlantDetailViewModel viewModel)
        {
            viewModel.NavidateBack();
            return true;
        }
        return base.OnBackButtonPressed();
    }
}