using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.Data.Repositories;

public class ReminderRepository(ApplicationDbContext context) : GenericRepository<Reminder>(context), IReminderRepository
{

    // Additional reminder-specific methods can be added here if needed
}