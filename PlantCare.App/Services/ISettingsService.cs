namespace PlantCare.App.Services;

public interface ISettingsService
{
    Task<string> GetThemeSettingAsync();

    Task<bool> GetWateringNotificationSettingAsync();

    Task SetWateringNotificationSettingAsync(bool isEnabled);

    Task<bool> GetFertilizationNotificationSettingAsync();

    Task SetFertilizationNotificationSettingAsync(bool isEnabled);

    Task SetThemeSettingAsync(string theme);

    Task<bool> GetDebugSettingAsync();

    Task SetDebugSettingAsync(bool isEnabled);
}