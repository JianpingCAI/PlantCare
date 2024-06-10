using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.Data.Repositories;

public class WateringHistoryRepository(ApplicationDbContext context)
    : GenericRepository<WateringHistory>(context), IWateringHistoryRepository
{
    public Task<List<WateringHistory>> GetWateringHistoryByPlantIdAsync(Guid plantId)
    {
        return _context.WateringHistories.Where(x => x.PlantId == plantId).ToListAsync();
    }
}