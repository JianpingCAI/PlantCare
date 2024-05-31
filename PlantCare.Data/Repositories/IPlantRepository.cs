using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public interface IPlantRepository
{
    Task<List<PlantDbModel>> GetAllAsync();

    Task<PlantDbModel?> GetByIdAsync(Guid id);

    Task AddAsync(PlantDbModel plant);

    Task<bool> UpdateAsync(PlantDbModel plant);

    Task<bool> DeleteAsync(Guid plantId);

    Task<List<PlantDbModel>> GetPlantsToWater();
    Task<List<PlantDbModel>> GetPlantsToFertilize();
    Task UpdateLastWateringTime(Guid plantId, DateTime time);
    Task UpdateLastFertilizationTime(Guid plantId, DateTime time);
}