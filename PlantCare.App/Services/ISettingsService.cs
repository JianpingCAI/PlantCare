namespace PlantCare.App.Services;

public interface ISettingsService
{
    Task<AppTheme> GetThemeSettingAsync();

    Task<bool> GetWateringNotificationSettingAsync();

    Task SaveWateringNotificationSettingAsync(bool isEnabled);

    Task<bool> GetFertilizationNotificationSettingAsync();

    Task SaveFertilizationNotificationSettingAsync(bool isEnabled);

    Task SaveThemeSettingAsync(AppTheme theme);

    Task<bool> GetDebugSettingAsync();

    Task SaveDebugSettingAsync(bool isEnabled);
}