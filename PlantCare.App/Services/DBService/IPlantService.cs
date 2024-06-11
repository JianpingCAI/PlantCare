using PlantCare.App.ViewModels;
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

    Task UpdateLastWateringTime(Guid plantId, DateTime time);

    Task UpdateLastFertilizationTime(Guid plantId, DateTime updateTime);

    Task AddWateringHistoryAsync(Guid id, DateTime lastWatered);

    Task AddFertilizationHistoryAsync(Guid id, DateTime lastFertilized);

    Task<List<PlantCareHistory>> GetAllPlantsWithCareHistoryAsync();
}