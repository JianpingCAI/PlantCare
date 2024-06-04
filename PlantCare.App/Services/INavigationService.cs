using PlantCare.Data.Models;

namespace PlantCare.App.Services;

public interface INavigationService
{
    Task GoToPlantDetail(Guid plantId);

    Task GoToAddPlant(int plantCount);

    Task GoToEditPlant(Plant plant);

    Task GoToPlantsOverview();

    //Task GoBack();
}