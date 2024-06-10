using AutoMapper;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.App.Services.DBService;

public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;
    private readonly IWateringHistoryRepository _waterHistoryRepository;
    private readonly IFertilizationHistoryRepository _fertilizationHistoryRepository;
    private readonly IMapper _mapper;

    public PlantService(
        IPlantRepository plantRepository,
        IWateringHistoryRepository waterHistoryRepository,
        IFertilizationHistoryRepository fertilizationHistoryRepository,
        IMapper mapper)
    {
        _plantRepository = plantRepository;
        _waterHistoryRepository = waterHistoryRepository;
        _fertilizationHistoryRepository = fertilizationHistoryRepository;
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

    public async Task<Guid> CreatePlantAsync(PlantDbModel plant)
    {
        PlantDbModel plantDB = await _plantRepository.AddAsync(plant);
        return plantDB.Id;
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

    public async Task<List<WateringHistory>> GetPlantWateringHistoryAsync(Guid plantId)
    {
        return await _waterHistoryRepository.GetWateringHistoryByPlantIdAsync(plantId);
    }

    public async Task<List<FertilizationHistory>> GetPlanFertilizationHistoryAsync(Guid plantId)
    {
        return await _fertilizationHistoryRepository.GetFertilizationHistoryByPlantIdAsync(plantId);
    }
}