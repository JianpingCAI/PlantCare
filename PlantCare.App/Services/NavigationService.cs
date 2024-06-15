using PlantCare.App.ViewModels;
using PlantCare.Data.Models;

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

        return Shell.Current.GoToAsync($"//{PageName.Overview}/{PageName.Add}", navigationParameter);
    }

    public Task GoToEditPlant(Plant plant)
    {
        var navigationParameter = new ShellNavigationQueryParameters
        {
            { "Plant", plant }
        };

        return Shell.Current.GoToAsync($"//{PageName.Overview}/{PageName.Edit}", navigationParameter);
    }

    public Task GoToPlantsOverview()
    {
        return Shell.Current.GoToAsync($"//{PageName.Overview}");
    }

    public Task GoToCareHistory(string plantName, CareType careType, List<TimeStampRecord> timestampRecords)
    {
        var navigationParameters = new ShellNavigationQueryParameters
        {
            {"plantName", plantName},
            {"careType", careType },
            {"records", timestampRecords }
        };

        return Shell.Current.GoToAsync($"//{PageName.History}/{PageName.SinglePlantCareHistory}", navigationParameters);
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