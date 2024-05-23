namespace PlantCare.App.Services;

public interface ISettingsService
{
    Task<bool> GetNotificationSettingAsync();

    Task<string> GetThemeSettingAsync();

    Task SetNotificationSettingAsync(bool isEnabled);

    Task SetThemeSettingAsync(string theme);
}