using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.Data.Repositories;

public class PlantRepository : GenericRepository<PlantDbModel>, IPlantRepository
{
    public PlantRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Additional plant-specific methods can be added here if needed
    public async Task<List<PlantDbModel>> GetPlantsToWater()
    {
        return await _context.Plants
                .Where(x => x.LastWatered.AddHours(x.WateringFrequencyInHours) <= DateTime.Now)
                .ToListAsync();
    }
}