using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories.interfaces;

public interface IWateringHistoryRepository : IRepository<WateringHistory>
{
    Task<List<WateringHistory>> GetWateringHistoryByPlantIdAsync(Guid plantId);
}
