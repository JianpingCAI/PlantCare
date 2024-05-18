using PlantCare.Data.Models;

namespace PlantCare.Data.Repositories;

public class PlantRepository : GenericRepository<Plant>, IPlantRepository
{
    public PlantRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Additional plant-specific methods can be added here if needed
}