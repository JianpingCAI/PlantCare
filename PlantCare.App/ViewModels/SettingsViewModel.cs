
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;

namespace PlantCare.App.ViewModels;
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private bool enableNotifications;

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
            EnableNotifications = await _settingsService.GetNotificationSettingAsync();
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
            await _settingsService.SetNotificationSettingAsync(EnableNotifications);
            await _settingsService.SetThemeSettingAsync(Theme);
        }
        finally { IsBusy = false; }
    }
}