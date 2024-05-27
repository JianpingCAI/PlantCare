using PlantCare.Data.DbModels;
using System.Diagnostics;

namespace PlantCare.App.Services;

public class NavigationService : INavigationService
{
    public async Task GoToPlantDetail(Guid id)
    {
        var parameters = new Dictionary<string, object> { { "PlantId", id } };
        await Shell.Current.GoToAsync("//overview/plant", parameters);
    }

    public async Task GoToAddPlant()
    {
        await Shell.Current.GoToAsync("//overview/add");
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