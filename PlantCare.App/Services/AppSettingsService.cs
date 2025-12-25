using PlantCare.App.Utils;
using PlantCare.Data;

namespace PlantCare.App.Services;

public class AppSettingsService : IAppSettingsService
{
    public Task<bool> GetWateringNotificationSettingAsync()
    {
        return Task.Run(() =>
        {
            return Preferences.Get(ConstStrings.EnableWateringNotification, true);
        });
    }

    public Task SaveWateringNotificationSettingAsync(bool isEnabled)
    {
        return Task.Run(() =>
        {
            Preferences.Set(ConstStrings.EnableWateringNotification, isEnabled);
        });
    }

    public Task<bool> GetFertilizationNotificationSettingAsync()
    {
        return Task.Run(() =>
        {
            return Preferences.Get(ConstStrings.EnableFertilizationNotification, true);
        });
    }

    public Task SaveFertilizationNotificationSettingAsync(bool isEnabled)
    {
        return Task.Run(() =>
        {
            Preferences.Set(ConstStrings.EnableFertilizationNotification, isEnabled);
        });
    }

    public Task<AppTheme> GetThemeSettingAsync()
    {
        return Task.Run(() =>
        {
            string strTheme = Preferences.Get(ConstStrings.AppTheme, AppTheme.Unspecified.ToString());

            if (string.IsNullOrEmpty(strTheme))
            {
                return AppTheme.Light;
            }

            try
            {
                AppTheme theme = (AppTheme)Enum.Parse(typeof(AppTheme), strTheme, true);
                return theme;
            }
            catch (Exception)
            {
                return AppTheme.Light;
            }
        });
    }

    public Task SaveThemeSettingAsync(AppTheme theme)
    {
        return Task.Run(() =>
        {
            Preferences.Set(ConstStrings.AppTheme, theme.ToString());
        });
    }

    public Task<Language> GetLanguageAsync()
    {
        return Task.Run(() =>
        {
            string? languageString = Preferences.Get(ConstStrings.Language, Language.English.ToString());
            if (string.IsNullOrEmpty(languageString))
            {
                return Language.English;
            }

            try
            {
                Language language = (Language)Enum.Parse(typeof(Language), languageString, true);
                return language;
            }
            catch (Exception)
            {
                return Language.English;
            }
        });
    }

    public Task SaveLanguageAsync(Language language)
    {
        return Task.Run(() =>
        {
            Preferences.Set(ConstStrings.Language, language.ToString());
        });
    }

    public Task<bool> GetDebugSettingAsync()
    {
        return Task.Run(() =>
        {
            return Preferences.Get(ConstStrings.IsDebugMode, false);
        });
    }

    public Task SaveDebugSettingAsync(bool isEnabled)
    {
        return Task.Run(() =>
        {
            Preferences.Set(ConstStrings.IsDebugMode, isEnabled);
        });
    }

    public Task<AppSettings> GetAppSettingsAsync()
    {
        return Task.Run(async () =>
        {
            return new AppSettings
            {
                Theme = await GetThemeSettingAsync(),
                WateringNotificationEnabled = await GetWateringNotificationSettingAsync(),
                FertilizationNotificationEnabled = await GetFertilizationNotificationSettingAsync(),
                Language = await GetLanguageAsync(),
            };
        });
    }

    public Task SaveAppSettingsAsync(AppSettings settings)
    {
        return Task.Run(async () =>
        {
            await SaveThemeSettingAsync(settings.Theme);
            await SaveWateringNotificationSettingAsync(settings.WateringNotificationEnabled);
            await SaveFertilizationNotificationSettingAsync(settings.FertilizationNotificationEnabled);
            await SaveLanguageAsync(settings.Language);
        });
    }
}

//public class SettingsServiceWithSecureStorage : IAppSettingsService
//{
//    public async Task<bool> GetWateringNotificationSettingAsync()
//    {
//        string? value = await SecureStorage.GetAsync(Consts.EnableWateringNotification);
//        if (value is null)
//        {
//            return true;
//        }

//        // Assume settings are stored with a key-value pair locally
//        return value == "True";
//    }

//    public async Task SaveWateringNotificationSettingAsync(bool isEnabled)
//    {
//        // Save the setting locally
//        await SecureStorage.SetAsync(Consts.EnableWateringNotification, isEnabled.ToString());
//    }

//    public async Task<bool> GetFertilizationNotificationSettingAsync()
//    {
//        string? value = await SecureStorage.GetAsync(Consts.EnableFertilizationNotification);
//        if (value is null)
//        {
//            return true;
//        }

//        // Assume settings are stored with a key-value pair locally
//        return value == "True";
//    }

//    public async Task SaveFertilizationNotificationSettingAsync(bool isEnabled)
//    {
//        // Save the setting locally
//        await SecureStorage.SetAsync(Consts.EnableFertilizationNotification, isEnabled.ToString());
//    }

//    public async Task<AppTheme> GetThemeSettingAsync()
//    {
//        // Retrieve the theme setting
//        string? strTheme = await SecureStorage.GetAsync("AppTheme");
//        if (string.IsNullOrEmpty(strTheme))
//        {
//            return AppTheme.Unspecified;
//        }

//        try
//        {
//            var theme = (AppTheme)Enum.Parse(typeof(AppTheme), strTheme, true);
//            return theme;
//        }
//        catch (Exception)
//        {
//            return AppTheme.Unspecified;
//        }
//    }

//    public async Task SaveThemeSettingAsync(AppTheme theme)
//    {
//        // Save the theme setting locally
//        await SecureStorage.SetAsync("AppTheme", theme.ToString());
//    }

//    public async Task<Language> GetLanguageAsync()
//    {
//        // Retrieve the theme setting
//        string? languageString = await SecureStorage.GetAsync("Language");
//        if (string.IsNullOrEmpty(languageString))
//        {
//            return Language.English;
//        }

//        try
//        {
//            Language language = (Language)Enum.Parse(typeof(Language), languageString, true);
//            return language;
//        }
//        catch (Exception)
//        {
//            return Language.English;
//        }
//    }

//    public async Task SaveLanguageAsync(Language language)
//    {
//        await SecureStorage.SetAsync("Language", language.ToString());
//    }

//    public async Task<bool> GetDebugSettingAsync()
//    {
//        string? value = await SecureStorage.GetAsync(Consts.IsDebugMode);
//        if (value is null)
//        {
//            return false;
//        }

//        // Assume settings are stored with a key-value pair locally
//        return value == "True";
//    }

//    public async Task SaveDebugSettingAsync(bool isEnabled)
//    {
//        await SecureStorage.SetAsync(Consts.IsDebugMode, isEnabled.ToString());
//    }
//}
