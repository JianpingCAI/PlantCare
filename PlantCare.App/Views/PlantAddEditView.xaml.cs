using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantAddEditView : ContentPage
{
    public PlantAddEditView(PlantAddEditViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}