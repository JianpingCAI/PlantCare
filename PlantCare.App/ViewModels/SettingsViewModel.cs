using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;
using System.Diagnostics;

namespace PlantCare.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [ObservableProperty]
    private bool _isWateringNotificationEnabled = true;

    [ObservableProperty]
    private bool _isFertilizationNotificationEnabled = true;

    [ObservableProperty]
    private bool _isDebugModeEnabled = true;

    [ObservableProperty]
    private string theme;

    //[RelayCommand]
    //private async Task LoadSettingsAsync()
    //{
    //    if (IsBusy) return;

    //    try
    //    {
    //        IsWateringNotificationEnabled = await _settingsService.GetWateringNotificationSettingAsync();
    //        IsFertilizationNotificationEnabled = await _settingsService.GetFertilizationNotificationSettingAsync();

    //        Theme = await _settingsService.GetThemeSettingAsync();
    //    }
    //    finally
    //    {
    //        IsBusy = false;
    //    }
    //}

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (IsBusy) return;

        try
        {
            await _settingsService.SetWateringNotificationSettingAsync(IsWateringNotificationEnabled);
            await _settingsService.SetThemeSettingAsync(Theme);
        }
        finally { IsBusy = false; }
    }

    partial void OnIsWateringNotificationEnabledChanged(bool isEnabled)
    {
        WeakReferenceMessenger.Default.Send(new IsWateringNotificationEnabledMessage { IsWateringNotificationEnabled = isEnabled });

        _settingsService.SetWateringNotificationSettingAsync(isEnabled);
    }

    partial void OnIsFertilizationNotificationEnabledChanged(bool isEnabled)
    {
        WeakReferenceMessenger.Default.Send(new IsFertilizationNotificationEnabledMessage { IsFertilizationNotificationEnabled = isEnabled });

        _settingsService.SetFertilizationNotificationSettingAsync(isEnabled);
    }

    partial void OnIsDebugModeEnabledChanged(bool isEnabled)
    {
        _settingsService.SetDebugSettingAsync(isEnabled);
    }

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        await LoadingDataWhenViewAppearing(LoadSettingsAsync);
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsWateringNotificationEnabled = await _settingsService.GetWateringNotificationSettingAsync();
            IsFertilizationNotificationEnabled = await _settingsService.GetFertilizationNotificationSettingAsync();

            Theme = await _settingsService.GetThemeSettingAsync();

            IsDebugModeEnabled = await _settingsService.GetDebugSettingAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception thrown: {ex.Message}");
            throw;
        }
    }
}