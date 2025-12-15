using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Services.DataExportImport;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using System.Diagnostics;

namespace PlantCare.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettingsService _settingsService;
    private readonly IDialogService _dialogService;
    private readonly IFolderPicker _folderPicker;
    private readonly IDataExportService _dataExportService;
    private readonly IDataImportService _dataImportService;
    private readonly INavigationService _navigationService;

    public SettingsViewModel(
        IAppSettingsService settingsService,
        IDialogService dialogService,
        IFolderPicker folderPicker,
        IDataExportService dataExportService,
        IDataImportService dataImportService,
        INavigationService navigationService)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
        _folderPicker = folderPicker;
        _dataExportService = dataExportService;
        _dataImportService = dataImportService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private bool _isWateringNotificationEnabled = true;

    [ObservableProperty]
    private bool _isFertilizationNotificationEnabled = true;

    //[ObservableProperty]
    //private bool _isDebugModeEnabled = false;
    public Language[] LanguageOptions { get; } = Enum.GetValues(typeof(Language)).Cast<Language>().ToArray();

    [ObservableProperty]
    private AppTheme _selectedTheme = AppTheme.Light;

    private bool _isSettingsLoaded = false;

    [ObservableProperty]
    private Language _selectedLanguage = App.AppLanguage;

    partial void OnSelectedLanguageChanged(Language value)
    {
        if (!_isSettingsLoaded)
        {
            return;
        }

        LocalizationManager.Instance.SetLanguage(value);
        App.UpdateAppLanguage(value);
        _ = SaveLanguagePreferenceAsync(value);
    }

    private async Task SaveLanguagePreferenceAsync(Language language)
    {
        try
        {
            await _settingsService.SaveLanguageAsync(language);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }

    //private partial void OnIsWateringNotificationEnabledChanged(bool isEnabled)
    [RelayCommand]
    public async Task ToggleWateringNotification(object? isEnabledObj)
    {
        try
        {
            if (isEnabledObj is not bool isEnabled)
            {
                return;
            }

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
        {
            return;
        }

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
        }
    }

    [RelayCommand]
    public async Task ExportData()
    {
        try
        {
            IsLoading = true;
            string exportDirectory = await PickFolderAsync();
            if (!string.IsNullOrEmpty(exportDirectory))
            {
                string filePath = await _dataExportService.ExportDataAsync(exportDirectory);
                await _dialogService.Notify("Export Completed", $"Data exported to {filePath}", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<string> PickFolderAsync(CancellationToken cancellationToken = default)
    {
        FolderPickerResult result = await _folderPicker.PickAsync(cancellationToken);
        //result.EnsureSuccess();

        if (result.IsSuccessful)
        {
            return result.Folder.Path;
        }
        else
        {
            return string.Empty;
        }
    }

    [ObservableProperty]
    private bool _isRemoveExistingData = false;

    [RelayCommand]
    public async Task ImportData()
    {
        try
        {
            IsLoading = true;

            FilePickerFileType zipFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
                                                  {
                                                     { DevicePlatform.iOS, new[] { "public.archive" } },
                                                     { DevicePlatform.Android, new[] { "application/zip" } },
                                                     { DevicePlatform.WinUI, new[] { "zip" } },
                                                     { DevicePlatform.MacCatalyst, new[] { "zip" } },
                                                  });

            FileResult? result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Please select a zip file for import",
                FileTypes = zipFileType
            });

            if (result != null && !string.IsNullOrEmpty(result.FullPath)
                && File.Exists(result.FullPath))
            {
                ExportDataModel importedData = await _dataImportService.ImportDataAsync(result.FullPath, IsRemoveExistingData);
                if (importedData == null)
                {
                    throw new Exception("Failed to import data");
                }

                AppSettings importedSettings = importedData.AppSettings;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                 {
                     IsWateringNotificationEnabled = importedSettings.WateringNotificationEnabled;
                     IsFertilizationNotificationEnabled = importedSettings.FertilizationNotificationEnabled;

                     SelectedLanguage = importedSettings.Language;
                     SelectedTheme = importedSettings.Theme;

                     WeakReferenceMessenger.Default.Send<DataImportMessage>(new DataImportMessage { PlantsCount = importedData.Plants.Count });

                     IToast toast = Toast.Make($"{importedData.Plants.Count} {LocalizationManager.Instance[ConstStrings.Added] ?? ConstStrings.Added}", ToastDuration.Short);
                     await toast.Show();
                 });
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            //await DisplayAlert("Import Failed", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CheckLogs()
    {
        try
        {
            IsLoading = true;

            await _navigationService.GotoLogsViewer();
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
