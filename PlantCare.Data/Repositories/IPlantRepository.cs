using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public interface IPlantRepository
{
    Task<List<PlantDbModel>> GetAllAsync();

    Task<PlantDbModel> GetByIdAsync(Guid id);

    Task AddAsync(PlantDbModel plant);

    Task UpdateAsync(PlantDbModel plant);

    Task DeleteAsync(PlantDbModel plant);
}