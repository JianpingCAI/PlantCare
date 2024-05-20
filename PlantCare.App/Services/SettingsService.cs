namespace PlantCare.App.Services;

public class SettingsService : ISettingsService
{
    public async Task<bool> GetNotificationSettingAsync()
    {
        // Assume settings are stored with a key-value pair locally
        return await SecureStorage.GetAsync("EnableNotifications") == "true";
    }

    public async Task SetNotificationSettingAsync(bool isEnabled)
    {
        // Save the setting locally
        await SecureStorage.SetAsync("EnableNotifications", isEnabled.ToString());
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