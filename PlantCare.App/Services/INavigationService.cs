using PlantCare.Data.DbModels;

namespace PlantCare.App.Services;

public interface INavigationService
{
    Task GoToPlantDetail(Guid plantId);

    Task GoToAddPlant(int plantCount);

    Task GoToEditPlant(PlantDbModel plant);

    Task GoToPlantsOverview();

    //Task GoBack();
}