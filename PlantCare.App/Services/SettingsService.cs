using PlantCare.Data;

namespace PlantCare.App.Services;

public class SettingsService : ISettingsService
{
    public async Task<bool> GetWateringNotificationSettingAsync()
    {
        string? value = await SecureStorage.GetAsync(Consts.EnableWateringNotification);
        if (value is null)
        {
            return true;
        }

        // Assume settings are stored with a key-value pair locally
        return value == "true";
    }

    public async Task SetWateringNotificationSettingAsync(bool isEnabled)
    {
        // Save the setting locally
        await SecureStorage.SetAsync(Consts.EnableWateringNotification, isEnabled.ToString());
    }

    public async Task<string> GetThemeSettingAsync()
    {
        // Retrieve the theme setting
        return await SecureStorage.GetAsync("AppTheme");
    }

    public async Task SetThemeSettingAsync(string theme)
    {
        // Save the theme setting locally
        await SecureStorage.SetAsync("AppTheme", theme);
    }
}