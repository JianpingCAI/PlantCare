using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.Services;

public interface IPlantService
{
    Task CreatePlantAsync(PlantDbModel plant);

    Task DeletePlantAsync(Guid plantId);

    Task<List<Plant>> GetAllPlantsAsync();

    Task<List<Plant>> GetPlantsToWater();

    Task<Plant> GetPlantByIdAsync(Guid id);

    Task<bool> UpdatePlantAsync(PlantDbModel plant);
}