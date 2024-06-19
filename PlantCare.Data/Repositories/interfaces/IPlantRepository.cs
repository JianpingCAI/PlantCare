using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories.interfaces;

public interface IPlantRepository : IRepository<PlantDbModel>
{
    Task UpdateLastWateringTime(Guid plantId, DateTime time);

    Task UpdateLastFertilizationTime(Guid plantId, DateTime time);

    Task<List<PlantDbModel>> GetAllPlantsWithCareHistoryAsync();

    Task AddPlantsAsync(List<PlantDbModel> plants);

    Task ClearAllAsync();
}