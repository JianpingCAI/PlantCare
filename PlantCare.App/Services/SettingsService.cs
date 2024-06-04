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
        return value == "True";
    }

    public async Task SaveWateringNotificationSettingAsync(bool isEnabled)
    {
        // Save the setting locally
        await SecureStorage.SetAsync(Consts.EnableWateringNotification, isEnabled.ToString());
    }

    public async Task<bool> GetFertilizationNotificationSettingAsync()
    {
        string? value = await SecureStorage.GetAsync(Consts.EnableFertilizationNotification);
        if (value is null)
        {
            return true;
        }

        // Assume settings are stored with a key-value pair locally
        return value == "True";
    }

    public async Task SaveFertilizationNotificationSettingAsync(bool isEnabled)
    {
        // Save the setting locally
        await SecureStorage.SetAsync(Consts.EnableFertilizationNotification, isEnabled.ToString());
    }

    public async Task<AppTheme> GetThemeSettingAsync()
    {
        // Retrieve the theme setting
        string? strTheme = await SecureStorage.GetAsync("AppTheme");
        if (string.IsNullOrEmpty(strTheme))
        {
            return AppTheme.Unspecified;
        }

        try
        {
            var theme = (AppTheme)Enum.Parse(typeof(AppTheme), strTheme, true);
            return theme;
        }
        catch (Exception)
        {
            return AppTheme.Unspecified;
        }
    }

    public async Task SaveThemeSettingAsync(AppTheme theme)
    {
        // Save the theme setting locally
        await SecureStorage.SetAsync("AppTheme", theme.ToString());
    }

    public async Task<bool> GetDebugSettingAsync()
    {
        string? value = await SecureStorage.GetAsync(Consts.IsDebugMode);
        if (value is null)
        {
            return false;
        }

        // Assume settings are stored with a key-value pair locally
        return value == "True";
    }

    public async Task SaveDebugSettingAsync(bool isEnabled)
    {
        await SecureStorage.SetAsync(Consts.IsDebugMode, isEnabled.ToString());
    }
}