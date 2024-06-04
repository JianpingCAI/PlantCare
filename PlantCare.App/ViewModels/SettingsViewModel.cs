using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;
using System.Diagnostics;

namespace PlantCare.App.ViewModels;

public partial class SettingsViewModel(ISettingsService settingsService, IDialogService dialogService) : ViewModelBase
{
    private readonly ISettingsService _settingsService = settingsService;
    private readonly IDialogService _dialogService = dialogService;

    [ObservableProperty]
    private bool _isWateringNotificationEnabled = true;

    [ObservableProperty]
    private bool _isFertilizationNotificationEnabled = true;

    //[ObservableProperty]
    //private bool _isDebugModeEnabled = false;

    [ObservableProperty]
    private AppTheme _selectedTheme = AppTheme.Unspecified;

    private bool _isSettingsLoaded = false;

#pragma warning disable CS8826 // Partial method declarations have signature differences.
    partial void OnIsWateringNotificationEnabledChanged(bool isEnabled)
#pragma warning restore CS8826 // Partial method declarations have signature differences.
    {
        WeakReferenceMessenger.Default.Send(new IsNotificationEnabledMessage
        {
            ReminderType = ReminderType.Watering,
            IsNotificationEnabled = isEnabled
        });

        _settingsService.SaveWateringNotificationSettingAsync(isEnabled);
    }

#pragma warning disable CS8826 // Partial method declarations have signature differences.
    partial void OnIsFertilizationNotificationEnabledChanged(bool isEnabled)
#pragma warning restore CS8826 // Partial method declarations have signature differences.
    {
        WeakReferenceMessenger.Default.Send(new IsNotificationEnabledMessage
        {
            ReminderType = ReminderType.Fertilization,
            IsNotificationEnabled = isEnabled
        });

        _settingsService.SaveFertilizationNotificationSettingAsync(isEnabled);
    }

    //partial void OnIsDebugModeEnabledChanged(bool isEnabled)
    //{
    //    _settingsService.SaveDebugSettingAsync(isEnabled);
    //}

    partial void OnSelectedThemeChanged(AppTheme value)
    {
        _settingsService.SaveThemeSettingAsync(value);
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
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsWateringNotificationEnabled = await _settingsService.GetWateringNotificationSettingAsync();
            IsFertilizationNotificationEnabled = await _settingsService.GetFertilizationNotificationSettingAsync();

            SelectedTheme = await _settingsService.GetThemeSettingAsync();

            //IsDebugModeEnabled = await _settingsService.GetDebugSettingAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception thrown: {ex.Message}");
            throw;
        }
    }
}