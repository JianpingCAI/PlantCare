using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.Services.DBService;

public interface IPlantService
{
    Task<Guid> CreatePlantAsync(PlantDbModel plant);

    Task DeletePlantAsync(Guid plantId);

    Task<List<Plant>> GetAllPlantsAsync();

    Task<Plant?> GetPlantByIdAsync(Guid id);

    Task<bool> UpdatePlantAsync(PlantDbModel plant);

    Task<List<Plant>> GetPlantsToWater();

    Task<List<Plant>> GetPlantsToFertilize();

    Task UpdateLastWateringTime(Guid plantId, DateTime time);

    Task UpdateLastFertilizationTime(Guid plantId, DateTime updateTime);

    Task<List<WateringHistory>> GetPlantWateringHistoryAsync(Guid plantId);

    Task<List<FertilizationHistory>> GetPlanFertilizationHistoryAsync(Guid plantId);
}