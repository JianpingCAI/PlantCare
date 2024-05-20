using PlantCare.App.ViewModels;

namespace PlantCare.App.Services;

public class NavigationService : INavigationService
{
    public async Task GotoPlantDetail(Guid id)
    {
        var parameters = new Dictionary<string, object> { { "PlantId", id } };
        await Shell.Current.GoToAsync("plant", parameters);
    }

    public Task GotoAddPlant()
    {
        return Shell.Current.GoToAsync("plant/add");
    }

    public async Task GoToEditPlant(PlantDetailViewModel detailModel)
    {
        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "Plant", detailModel }
        };

        await Shell.Current.GoToAsync("plant/edit", navigationParameter);
    }

    public Task GoToOverview()
    {
        return Shell.Current.GoToAsync("//overview");
    }

    public Task GoBack()
    {
        return Shell.Current.GoToAsync("..");
    }
}