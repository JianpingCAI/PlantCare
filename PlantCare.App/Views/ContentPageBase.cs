using PlantCare.App.ViewModels.Base;

namespace PlantCare.App.Views;

public class ContentPageBase : ContentPage
{
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is not IViewModelBase viewModel)
        {
            return;
        }

        await viewModel.OnViewAppearingCommand.ExecuteAsync(null);
    }
}