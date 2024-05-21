using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public class PlantRepository : GenericRepository<PlantDbModel>, IPlantRepository
{
    public PlantRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Additional plant-specific methods can be added here if needed
}