using PlantCare.Data.Models;

namespace PlantCare.Data.Repositories;

public interface IReminderRepository
{
    Task<List<Reminder>> GetAllAsync();

    Task<Reminder> GetByIdAsync(Guid id);

    Task AddAsync(Reminder reminder);

    Task UpdateAsync(Reminder reminder);

    Task DeleteAsync(Reminder reminder);
}