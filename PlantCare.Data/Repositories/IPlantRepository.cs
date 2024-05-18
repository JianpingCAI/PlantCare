using PlantCare.Data.Models;

namespace PlantCare.Data.Repositories;

public interface IPlantRepository
{
    Task<List<Plant>> GetAllAsync();

    Task<Plant> GetByIdAsync(Guid id);

    Task AddAsync(Plant plant);

    Task UpdateAsync(Plant plant);

    Task DeleteAsync(Plant plant);
}