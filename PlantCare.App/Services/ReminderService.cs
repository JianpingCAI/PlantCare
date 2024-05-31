using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories;

namespace PlantCare.App.Services;

public class ReminderService(IReminderRepository reminderRepository) : IReminderService
{
    private readonly IReminderRepository _reminderRepository = reminderRepository;

    public async Task<List<Reminder>> GetAllRemindersAsync()
    {
        return await _reminderRepository.GetAllAsync();
    }

    public async Task<Reminder?> GetReminderByIdAsync(Guid id)
    {
        return await _reminderRepository.GetByIdAsync(id);
    }

    public async Task AddReminderAsync(Reminder reminder)
    {
        await _reminderRepository.AddAsync(reminder);
    }

    public async Task UpdateReminderAsync(Reminder reminder)
    {
        await _reminderRepository.UpdateAsync(reminder);
    }

    public async Task<bool> DeleteReminderAsync(Guid reminderId)
    {
        return await _reminderRepository.DeleteAsync(reminderId);
    }
}