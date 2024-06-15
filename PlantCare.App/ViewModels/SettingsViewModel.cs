using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using System.Diagnostics;
using System.Globalization;

namespace PlantCare.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettingsService _settingsService;
    private readonly IDialogService _dialogService;

    public SettingsViewModel(IAppSettingsService settingsService, IDialogService dialogService)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private bool _isWateringNotificationEnabled = true;

    [ObservableProperty]
    private bool _isFertilizationNotificationEnabled = true;

    //[ObservableProperty]
    //private bool _isDebugModeEnabled = false;
    public Language[] LanguageOptions { get; } = Enum.GetValues(typeof(Language)).Cast<Language>().ToArray();

    [ObservableProperty]
    private AppTheme _selectedTheme = AppTheme.Unspecified;

    private bool _isSettingsLoaded = false;

    [ObservableProperty]
    private Language _selectedLanguage = App.AppLanguage;

    //private partial void OnIsWateringNotificationEnabledChanged(bool isEnabled)
    [RelayCommand]
    public async Task ToggleWateringNotification(object? isEnabledObj)
    {
        try
        {
            if (isEnabledObj is not bool isEnabled)
                return;

            WeakReferenceMessenger.Default.Send(new IsNotificationEnabledMessage
            {
                ReminderType = CareType.Watering,
                IsNotificationEnabled = isEnabled
            });

            await _settingsService.SaveWateringNotificationSettingAsync(isEnabled);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }

    //partial void OnIsFertilizationNotificationEnabledChanged(bool isEnabled)
    [RelayCommand]
    public async Task ToggleFertilizationNotification(object? isEnabledObj)
    {
        if (isEnabledObj is not bool isEnabled)
            return;

        try
        {
            WeakReferenceMessenger.Default.Send(new IsNotificationEnabledMessage
            {
                ReminderType = CareType.Fertilization,
                IsNotificationEnabled = isEnabled
            });

            await _settingsService.SaveFertilizationNotificationSettingAsync(isEnabled);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }

    //partial void OnIsDebugModeEnabledChanged(bool isEnabled)
    //{
    //    _settingsService.SaveDebugSettingAsync(isEnabled);
    //}

    partial void OnSelectedThemeChanged(AppTheme value)
    {
        try
        {
            _settingsService.SaveThemeSettingAsync(value);
        }
        catch (Exception)
        {
        }
    }

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        if (_isSettingsLoaded)
        {
            return;
        }

        try
        {
            await LoadSettingsAsync();
            _isSettingsLoaded = true;
        }
        catch (Exception ex)
        {
            _isSettingsLoaded = false;
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsWateringNotificationEnabled = await _settingsService.GetWateringNotificationSettingAsync();
            IsFertilizationNotificationEnabled = await _settingsService.GetFertilizationNotificationSettingAsync();

            SelectedTheme = await _settingsService.GetThemeSettingAsync();

            SelectedLanguage = await _settingsService.GetLanguageAsync();

            //IsDebugModeEnabled = await _settingsService.GetDebugSettingAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception thrown: {ex.Message}");
            throw;
        }
    }

    [RelayCommand]
    public async Task SelectLanguageChanged(object? selectedItem)
    {
        if (selectedItem is not Language selectedLanguage) return;

        try
        {
            LocalizationManager.Instance.SetLanguage(selectedLanguage);
            await _settingsService.SaveLanguageAsync(SelectedLanguage);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }
}