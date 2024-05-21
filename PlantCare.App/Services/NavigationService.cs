using PlantCare.Data.DbModels;

namespace PlantCare.App.Services;

public class NavigationService : INavigationService
{
    public async Task GoToPlantDetail(Guid id)
    {
        var parameters = new Dictionary<string, object> { { "PlantId", id } };
        await Shell.Current.GoToAsync("plant", parameters);
    }

    public Task GoToAddPlant()
    {
        return Shell.Current.GoToAsync("plant/add");
    }

    public async Task GoToEditPlant(PlantDbModel plant)
    {
        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "Plant", plant }
        };

        await Shell.Current.GoToAsync("plant/edit", navigationParameter);
    }

    public Task GoToPlantsOverview()
    {
        return Shell.Current.GoToAsync("//overview");
    }

    public Task GoBack()
    {
        return Shell.Current.GoToAsync("..");
    }
}