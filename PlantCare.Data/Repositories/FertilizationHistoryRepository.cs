using Microsoft.EntityFrameworkCore;
using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.Data.Repositories;

public class FertilizationHistoryRepository(ApplicationDbContext context)
    : GenericRepository<FertilizationHistory>(context), IFertilizationHistoryRepository
{
    public Task<List<FertilizationHistory>> GetFertilizationHistoryByPlantIdAsync(Guid plantId)
    {
        return _context.FertilizationHistories.Where(x => x.PlantId == plantId).ToListAsync();
    }
}