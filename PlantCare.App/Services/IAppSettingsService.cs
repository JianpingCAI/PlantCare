using PlantCare.App.Utils;

namespace PlantCare.App.Services;

public interface IAppSettingsService
{
    Task<AppTheme> GetThemeSettingAsync();

    Task<bool> GetWateringNotificationSettingAsync();

    Task SaveWateringNotificationSettingAsync(bool isEnabled);

    Task<bool> GetFertilizationNotificationSettingAsync();

    Task SaveFertilizationNotificationSettingAsync(bool isEnabled);

    Task SaveThemeSettingAsync(AppTheme theme);

    Task<bool> GetDebugSettingAsync();

    Task SaveDebugSettingAsync(bool isEnabled);

    Task<Language> GetLanguageAsync();
    Task SaveLanguageAsync(Language language);

}