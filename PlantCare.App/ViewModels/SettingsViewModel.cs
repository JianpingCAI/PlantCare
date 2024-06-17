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
using PlantCare.Data.Models;
using System.Diagnostics;
using System.Globalization;

namespace PlantCare.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IAppSettingsService _settingsService;
    private readonly IDialogService _dialogService;
    private readonly IFolderPicker _folderPicker;
    private readonly IDataExportService _dataExportService;
    private readonly IDataImportService _dataImportService;

    public SettingsViewModel(
        IAppSettingsService settingsService,
        IDialogService dialogService,
        IFolderPicker folderPicker,
        IDataExportService dataExportService,
        IDataImportService dataImportService)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
        _folderPicker = folderPicker;
        _dataExportService = dataExportService;
        _dataImportService = dataImportService;
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

        //FolderPickerResult result = await FolderPicker.Default.PickAsync(cancellationToken);
        if (result.IsSuccessful)
        {
            //await Toast.Make($"The folder was picked: Name - {result.Folder.Name}, Path - {result.Folder.Path}", ToastDuration.Long).Show(cancellationToken);
            return result.Folder.Path;
        }
        else
        {
            //await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
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

            var zipFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
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

            if (result != null && !string.IsNullOrEmpty(result.FullPath))
            {
                int plantsCount = await _dataImportService.ImportDataAsync(result.FullPath, IsRemoveExistingData);

                WeakReferenceMessenger.Default.Send<DataImportMessage>(new DataImportMessage { PlantsCount = plantsCount });

                var toast = Toast.Make($"{plantsCount} {LocalizationManager.Instance[ConstStrings.Added] ?? ConstStrings.Added}", ToastDuration.Short);
                await toast.Show();
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
}