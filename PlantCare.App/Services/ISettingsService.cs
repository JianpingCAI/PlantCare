namespace PlantCare.App.Services;

public interface ISettingsService
{
    Task<bool> GetWateringNotificationSettingAsync();

    Task<string> GetThemeSettingAsync();

    Task SetWateringNotificationSettingAsync(bool isEnabled);

    Task SetThemeSettingAsync(string theme);

    Task<bool> GetDebugSettingAsync();
    Task SetDebugSettingAsync(bool isEnabled);
}