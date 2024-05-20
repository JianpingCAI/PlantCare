using PlantCare.Data.Models;
using PlantCare.Data.Repositories;

namespace PlantCare.App.Services;

public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;

    public PlantService(IPlantRepository plantRepository)
    {
        _plantRepository = plantRepository;
    }

    public async Task<List<Plant>> GetAllPlantsAsync()
    {
        return await _plantRepository.GetAllAsync();
    }

    public async Task<Plant> GetPlantByIdAsync(Guid id)
    {
        return await _plantRepository.GetByIdAsync(id);
    }

    public async Task AddPlantAsync(Plant plant)
    {
        await _plantRepository.AddAsync(plant);
    }

    public async Task UpdatePlantAsync(Plant plant)
    {
        await _plantRepository.UpdateAsync(plant);
    }

    public async Task DeletePlantAsync(Plant plant)
    {
        await _plantRepository.DeleteAsync(plant);
    }

    public Task<IEnumerable<object>> SearchPlantsAsync(string searchText)
    {
        throw new NotImplementedException();
    }
}