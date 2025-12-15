using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.Data.Repositories;

public class PlantRepository(ApplicationDbContext context) : GenericRepository<PlantDbModel>(context), IPlantRepository
{
    // Additional plant-specific methods can be added here if needed

    public async Task UpdateLastWateringTime(Guid plantId, DateTime time)
    {
        PlantDbModel? plant = await _context.Plants
            .Include(p => p.WateringHistories)
            .FirstOrDefaultAsync(p => p.Id == plantId);
            
        if (plant != null)
        {
            plant.LastWatered = time;
            plant.WateringHistories.Add(new WateringHistory { PlantId = plant.Id, CareTime = time });
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLastFertilizationTime(Guid plantId, DateTime time)
    {
        PlantDbModel? plant = await _context.Plants
            .Include(p => p.FertilizationHistories)
            .FirstOrDefaultAsync(p => p.Id == plantId);
            
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

    public async Task AddPlantsAsync(List<PlantDbModel> plants)
    {
        if (plants == null || plants.Count == 0)
        {
            return;
        }

        await _context.Plants.AddRangeAsync(plants);
        await _context.SaveChangesAsync();
    }

    public async Task ClearAllTablesAsync()
    {
        await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {nameof(WateringHistory)}");
        await _context.Database.ExecuteSqlRawAsync($"DELETE FROM {nameof(FertilizationHistory)}");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Plants");
    }

    public Task<List<string>> GetAllPhotoPathsAsync()
    {
        return Task.Run(() =>
        {
            return _context.Plants.Select(x => x.PhotoPath).ToList();
        });
    }
}
