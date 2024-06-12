using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using System.Diagnostics;

namespace PlantCare.App.Services;

public class NavigationService : INavigationService
{
    public Task GoToPlantDetail(Guid id)
    {
        var parameters = new Dictionary<string, object> { { "PlantId", id } };
        return Shell.Current.GoToAsync("//overview/plant", parameters);
    }

    public Task GoToAddPlant(int plantCount)
    {
        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "PlantCount", plantCount }
        };

        return Shell.Current.GoToAsync("//overview/add", navigationParameter);
    }

    public Task GoToEditPlant(Plant plant)
    {
        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "Plant", plant }
        };

        return Shell.Current.GoToAsync("//overview/edit", navigationParameter);
    }

    public Task GoToPlantsOverview()
    {
        return Shell.Current.GoToAsync("//overview");
    }

    //public async Task GoBack()
    //{
    //    try
    //    {
    //        await Shell.Current.GoToAsync("..");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine(ex.Message);
    //    }
    //}
}