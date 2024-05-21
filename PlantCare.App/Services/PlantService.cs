using AutoMapper;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using PlantCare.Data.Repositories;

namespace PlantCare.App.Services;

public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;
    private readonly IMapper _mapper;

    public PlantService(IPlantRepository plantRepository, IMapper mapper)
    {
        _plantRepository = plantRepository;
        _mapper = mapper;
    }

    public async Task<List<Plant>> GetAllPlantsAsync()
    {
        var dbModels = await _plantRepository.GetAllAsync();
        return _mapper.Map<List<Plant>>(dbModels);
    }

    public async Task<PlantDbModel> GetPlantByIdAsync(Guid id)
    {
        return await _plantRepository.GetByIdAsync(id);
    }

    public async Task CreatePlantAsync(PlantDbModel plant)
    {
        await _plantRepository.AddAsync(plant);
    }

    public async Task UpdatePlantAsync(PlantDbModel plant)
    {
        await _plantRepository.UpdateAsync(plant);
    }

    public async Task DeletePlantAsync(PlantDbModel plant)
    {
        await _plantRepository.DeleteAsync(plant);
    }

    public Task<IEnumerable<object>> SearchPlantsAsync(string searchText)
    {
        throw new NotImplementedException();
    }
}