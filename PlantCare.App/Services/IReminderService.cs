﻿using PlantCare.Data.DbModels;

namespace PlantCare.App.Services;

public interface IReminderService
{
    Task<List<Reminder>> GetAllRemindersAsync();

    Task<Reminder?> GetReminderByIdAsync(Guid id);

    Task AddReminderAsync(Reminder reminder);

    Task UpdateReminderAsync(Reminder reminder);

    Task<bool> DeleteReminderAsync(Guid reminderId);
}