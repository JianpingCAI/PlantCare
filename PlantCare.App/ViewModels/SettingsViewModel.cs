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

    [ObservableProperty]
    private bool _isWateringNotificationEnabled = true;

    [ObservableProperty]
    private string theme;

    [ObservableProperty]
    private AppTheme _selectedTheme;

    //public AppTheme SelectedTheme
    //{
    //    get => _selectedTheme;
    //    set
    //    {
    //        if (_selectedTheme != value)
    //        {
    //            _selectedTheme = value;
    //            OnPropertyChanged();
    //        }
    //    }
    //}

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;

    }

    partial void OnIsWateringNotificationEnabledChanged(bool isNotificationEnabled)
    {
        // Handle additional logic when the switch is toggled
        // For example, update some other properties or call a service
        Debug.WriteLine($"{isNotificationEnabled}");
        WeakReferenceMessenger.Default.Send(new WateringNotificationChangedMessage { IsWateringNotificationEnabled = isNotificationEnabled });

    }

    [RelayCommand]
    private void SelectTheme(RadioButton selectedRadioButton)
    {
        if (selectedRadioButton != null)
        {
            selectedRadioButton.IsChecked = true;
        }
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsWateringNotificationEnabled = await _settingsService.GetWateringNotificationSettingAsync();
            Theme = await _settingsService.GetThemeSettingAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

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
}