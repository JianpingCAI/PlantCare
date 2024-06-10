using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories.interfaces;

public interface IPlantRepository : IRepository<PlantDbModel>
{
    Task<List<PlantDbModel>> GetPlantsToWater();

    Task<List<PlantDbModel>> GetPlantsToFertilize();

    Task UpdateLastWateringTime(Guid plantId, DateTime time);

    Task UpdateLastFertilizationTime(Guid plantId, DateTime time);
}