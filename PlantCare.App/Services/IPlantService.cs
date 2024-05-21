using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.Services;

public interface IPlantService
{
    Task CreatePlantAsync(PlantDbModel plant);

    Task DeletePlantAsync(PlantDbModel plant);

    Task<List<Plant>> GetAllPlantsAsync();

    Task<PlantDbModel> GetPlantByIdAsync(Guid id);

    Task UpdatePlantAsync(PlantDbModel plant);

    Task<IEnumerable<object>> SearchPlantsAsync(string searchText);
}