using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public class ReminderRepository : GenericRepository<Reminder>, IReminderRepository
{
    public ReminderRepository(ApplicationDbContext context) : base(context)
    {
    }

    Task<List<Reminder>> IReminderRepository.GetAllAsync()
    {
        throw new NotImplementedException();
    }

    // Additional reminder-specific methods can be added here if needed
}