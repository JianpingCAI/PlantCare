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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            viewModel.NavigateBack();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
        }

        return base.OnBackButtonPressed();
    }
}
