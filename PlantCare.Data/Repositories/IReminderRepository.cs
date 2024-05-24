using PlantCare.Data.DbModels;

namespace PlantCare.Data.Repositories;

public interface IReminderRepository
{
    Task<List<Reminder>> GetAllAsync();

    Task<Reminder?> GetByIdAsync(Guid id);

    Task AddAsync(Reminder reminder);

    Task<bool> UpdateAsync(Reminder reminder);

    Task<bool> DeleteAsync(Guid reminderId);
}