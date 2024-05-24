using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public class ReminderRepository : GenericRepository<Reminder>, IReminderRepository
{
    public ReminderRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Additional reminder-specific methods can be added here if needed
}