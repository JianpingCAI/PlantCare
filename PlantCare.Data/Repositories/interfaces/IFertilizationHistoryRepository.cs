using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories.interfaces;

public interface IFertilizationHistoryRepository : IRepository<FertilizationHistory>
{
    Task<List<FertilizationHistory>> GetFertilizationHistoryByPlantIdAsync(Guid plantId);
}