using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantAddEditView : ContentPage
{
    public PlantAddEditView(PlantAddEditViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        if (BindingContext is PlantAddEditViewModel viewModel)
        {
            viewModel.NavigateBack();
            return true;
        }

        return base.OnBackButtonPressed();
    }
}