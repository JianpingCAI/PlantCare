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
        List<PlantDbModel> dbModels = await _plantRepository.GetAllAsync();
        return _mapper.Map<List<Plant>>(dbModels);
    }

    public async Task<Plant?> GetPlantByIdAsync(Guid id)
    {
        PlantDbModel? dbModel = await _plantRepository.GetByIdAsync(id);
        if (null == dbModel)
        {
            return null;
        }
        return _mapper.Map<Plant>(dbModel);
    }

    public async Task CreatePlantAsync(PlantDbModel plant)
    {
        await _plantRepository.AddAsync(plant);
    }

    public async Task<bool> UpdatePlantAsync(PlantDbModel plant)
    {
        return await _plantRepository.UpdateAsync(plant);
    }

    public async Task DeletePlantAsync(Guid plantId)
    {
        await _plantRepository.DeleteAsync(plantId);
    }

    public async Task<List<Plant>> GetPlantsToWater()
    {
        var dbModels = await _plantRepository.GetPlantsToWater();

        return _mapper.Map<List<Plant>>(dbModels);
    }

    public async Task<List<Plant>> GetPlantsToFertilize()
    {
        var dbModels = await _plantRepository.GetPlantsToFertilize();

        return _mapper.Map<List<Plant>>(dbModels);
    }

    public async Task UpdateLastWateringTime(Guid plantId, DateTime time)
    {
        await _plantRepository.UpdateLastWateringTime(plantId, time);
    }

    public async Task UpdateLastFertilizationTime(Guid plantId, DateTime time)
    {
        await _plantRepository.UpdateLastFertilizationTime(plantId, time);
    }
}