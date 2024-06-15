using AutoMapper;
using PlantCare.App.ViewModels;
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

    public Task<List<Plant>> GetAllPlantsAsync()
    {
        return Task.Run(async () =>
        {
            List<PlantDbModel> dbModels = await _plantRepository.GetAllAsync();
            List<Plant> plants = _mapper.Map<List<Plant>>(dbModels);

            return plants.OrderBy(x => x.Name).ToList();
        });
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

    public async Task UpdateLastWateringTime(Guid plantId, DateTime time)
    {
        await _plantRepository.UpdateLastWateringTime(plantId, time);
    }

    public async Task UpdateLastFertilizationTime(Guid plantId, DateTime time)
    {
        await _plantRepository.UpdateLastFertilizationTime(plantId, time);
    }

    public async Task AddWateringHistoryAsync(Guid plantId, DateTime lastWatered)
    {
        await _waterHistoryRepository.AddAsync(new WateringHistory() { CareTime = lastWatered, PlantId = plantId });
    }

    public async Task DeleteWateringHistoryAsync(Guid plantId, Guid historyId)
    {
        WateringHistory? deleted = await _waterHistoryRepository.DeleteAsync(historyId);
        PlantDbModel? plantDB = await _plantRepository.GetByIdAsync(plantId);
        if (deleted is null || plantDB is null)
        {
            return;
        }

        if (plantDB.LastWatered == deleted.CareTime)
        {
            List<WateringHistory> historyList = await _waterHistoryRepository.GetWateringHistoryByPlantIdAsync(plantId);
            if (historyList.Count != 0)
            {
                plantDB.LastWatered = historyList.Last().CareTime;

                await _plantRepository.UpdateAsync(plantDB);
            }
        }
    }

    public async Task AddFertilizationHistoryAsync(Guid plantId, DateTime lastFertilized)
    {
        await _fertilizationHistoryRepository.AddAsync(new FertilizationHistory() { PlantId = plantId, CareTime = lastFertilized });
    }

    public async Task DeleteFertilizationHistoryAsync(Guid plantId, Guid historyId)
    {
        var deleted = await _fertilizationHistoryRepository.DeleteAsync(historyId);

        PlantDbModel? plantDB = await _plantRepository.GetByIdAsync(plantId);
        if (deleted is null || plantDB is null)
        {
            return;
        }

        if (plantDB.LastFertilized == deleted.CareTime)
        {
            List<FertilizationHistory> historyList = await _fertilizationHistoryRepository.GetFertilizationHistoryByPlantIdAsync(plantId);
            if (historyList.Count != 0)
            {
                plantDB.LastFertilized = historyList.Last().CareTime;

                await _plantRepository.UpdateAsync(plantDB);
            }
        }
    }

    public Task<List<PlantCareHistory>> GetAllPlantsWithCareHistoryAsync()
    {
        return Task.Run(async () =>
        {
            List<PlantDbModel> plants = await _plantRepository.GetAllPlantsWithCareHistoryAsync();
            plants = [.. plants.OrderBy(x => x.Name)];

            List<PlantCareHistory> plantCareHistoryList = new(plants.Count);

            foreach (PlantDbModel plant in plants)
            {
                List<TimeStampRecord> wateringTimestamps = plant.WateringHistories
                                                                .Select(x => new TimeStampRecord(x.CareTime, x.PlantId, x.Id))
                                                                .OrderBy(x => x.Timestamp).ToList();

                List<TimeStampRecord> fertilizationTimestamps = plant.FertilizationHistories
                                                                .Select(x => new TimeStampRecord(x.CareTime, x.PlantId, x.Id))
                                                                .OrderBy(x => x.Timestamp).ToList();

                plantCareHistoryList.Add(new PlantCareHistory()
                {
                    PlantId = plant.Id,
                    Name = plant.Name,
                    PhotoPath = plant.PhotoPath,
                    WateringTimestamps = wateringTimestamps,
                    FertilizationTimestamps = fertilizationTimestamps
                });
            }

            return plantCareHistoryList;
        });
    }
}