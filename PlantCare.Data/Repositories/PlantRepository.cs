using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.Data.Repositories;

public class PlantRepository(ApplicationDbContext context) : GenericRepository<PlantDbModel>(context), IPlantRepository
{
    // Additional plant-specific methods can be added here if needed
    public async Task<List<PlantDbModel>> GetPlantsToWater()
    {
        return await _context.Plants
                .Where(x => x.LastWatered.AddHours(x.WateringFrequencyInHours) <= DateTime.Now)
                .ToListAsync();
    }

    public async Task<List<PlantDbModel>> GetPlantsToFertilize()
    {
        return await _context.Plants
                .Where(x => x.LastFertilized.AddHours(x.FertilizeFrequencyInHours) <= DateTime.Now)
                .ToListAsync();
    }

    public async Task UpdateLastWateringTime(Guid plantId, DateTime time)
    {
        PlantDbModel? plant = await _context.Plants.FindAsync(plantId);
        if (plant != null)
        {
            plant.LastWatered = time;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLastFertilizationTime(Guid plantId, DateTime time)
    {
        PlantDbModel? plant = await _context.Plants.FindAsync(plantId);
        if (plant != null)
        {
            plant.LastFertilized = time;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<PlantDbModel>> GetAllPlantsWithWateringHistoryAsync()
    {
        return await _context.Plants
                             .Include(p => p.WateringHistories)
                             .ToListAsync();
    }
}