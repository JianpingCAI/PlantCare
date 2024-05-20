using PlantCare.Data.Models;

namespace PlantCare.App.Services;

public interface IPlantService
{
    Task AddPlantAsync(Plant plant);

    Task DeletePlantAsync(Plant plant);

    Task<List<Plant>> GetAllPlantsAsync();

    Task<Plant> GetPlantByIdAsync(Guid id);

    Task UpdatePlantAsync(Plant plant);

    Task<IEnumerable<object>> SearchPlantsAsync(string searchText);
}