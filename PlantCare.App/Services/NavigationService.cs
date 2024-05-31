using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using System.Diagnostics;

namespace PlantCare.App.Services;

public class NavigationService : INavigationService
{
    public async Task GoToPlantDetail(Guid id)
    {
        var parameters = new Dictionary<string, object> { { "PlantId", id } };
        await Shell.Current.GoToAsync("//overview/plant", parameters);
    }

    public async Task GoToAddPlant(int plantCount)
    {
        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "PlantCount", plantCount }
        };

        await Shell.Current.GoToAsync("//overview/add", navigationParameter);
    }

    public async Task GoToEditPlant(PlantDbModel plant)
    {
        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "Plant", plant }
        };

        await Shell.Current.GoToAsync("//overview/edit", navigationParameter);
    }

    public async Task GoToPlantsOverview()
    {
        await Shell.Current.GoToAsync("//overview");
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