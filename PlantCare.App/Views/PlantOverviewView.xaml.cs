using PlantCare.App.ViewModels;

namespace PlantCare.App.Views;

public partial class PlantOverviewView : ContentPageBase
{
    public PlantOverviewView(PlantListOverviewViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

  private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is PlantListOverviewViewModel viewModel)
        {
            await viewModel.ResetSearchAsync();
        }
    }
}