using PlantCare.App.ViewModels;

namespace PlantCare.App.Services;

public interface INavigationService
{
    Task GotoPlantDetail(Guid plantId);

    Task GotoAddPlant();

    Task GoToEditPlant(PlantDetailViewModel plant);

    Task GoToOverview();

    Task GoBack();
}