using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.Data.Repositories;

public class PlantRepository(ApplicationDbContext context) : GenericRepository<PlantDbModel>(context), IPlantRepository
{
    // Additional plant-specific methods can be added here if needed

    public async Task UpdateLastWateringTime(Guid plantId, DateTime time)
    {
        PlantDbModel? plant = await _context.Plants.FindAsync(plantId);
        if (plant != null)
        {
            plant.LastWatered = time;
            plant.WateringHistories.Add(new WateringHistory { PlantId = plant.Id, CareTime = time });
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLastFertilizationTime(Guid plantId, DateTime time)
    {
        PlantDbModel? plant = await _context.Plants.FindAsync(plantId);
        if (plant != null)
        {
            plant.LastFertilized = time;
            plant.FertilizationHistories.Add(new FertilizationHistory { PlantId = plant.Id, CareTime = time });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<PlantDbModel>> GetAllPlantsWithCareHistoryAsync()
    {
        return await _context.Plants
                             .Include(p => p.WateringHistories)
                             .Include(p => p.FertilizationHistories)
                             .ToListAsync();
    }
}