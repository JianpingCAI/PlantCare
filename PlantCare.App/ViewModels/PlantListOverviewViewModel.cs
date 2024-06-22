using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;

using System.Collections.ObjectModel;
using PlantCare.App.ViewModels.Base;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.Data.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.Text.Json;
using Plugin.LocalNotification.EventArgs;
using PlantCare.App.Services.DBService;
using PlantCare.Data;
using PlantCare.App.Utils;
using CommunityToolkit.Maui.Alerts;

namespace PlantCare.App.ViewModels;

public partial class PlantListOverviewViewModel : ViewModelBase,
    IRecipient<PlantModifiedMessage>,
    IRecipient<PlantAddedMessage>,
    IRecipient<PlantDeletedMessage>,
    IRecipient<IsNotificationEnabledMessage>,
    IRecipient<PlantStateChangedMessage>,
    IRecipient<PlantCareHistoryChangedMessage>,
    IRecipient<DataImportMessage>,
    IDisposable
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly IAppSettingsService _settingsService;
    private readonly IAppLogger<PlantListOverviewViewModel> _logger;

    public PlantListOverviewViewModel(
        IPlantService plantService,
        INavigationService navigationService,
        INotificationService notificationService,
        IDialogService dialogService,
        IAppSettingsService settingsService,
        IAppLogger<PlantListOverviewViewModel> logger)
    {
        _plantService = plantService;

        _navigationService = navigationService;
        _notificationService = notificationService;
        _dialogService = dialogService;
        _settingsService = settingsService;
        _logger = logger;

        WeakReferenceMessenger.Default.Register<PlantAddedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantModifiedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantDeletedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantStateChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantCareHistoryChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<DataImportMessage>(this);

        if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
        {
            WeakReferenceMessenger.Default.Register<IsNotificationEnabledMessage>(this);

            //_notificationService.NotificationReceiving = OnNotificationReceiving;
            //_notificationService.NotificationReceived += OnNotificationReceived;
            _notificationService.NotificationActionTapped += OnNotificationActionTapped;
        }

        AdjustSpan();
        DeviceDisplay.MainDisplayInfoChanged += OnDeviceDisplay_MainDisplayInfoChanged;
    }

    public ObservableCollection<PlantListItemViewModel> Plants { get; } = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private PlantListItemViewModel? _selectedPlant = null;

    [RelayCommand]
    private async Task SelectPlant()
    {
        if (IsBusy)
            return;

        try
        {
            // Navigate to details view with selected plant
            if (SelectedPlant is not null)
            {
                await _navigationService.GoToPlantDetail(SelectedPlant.Id);

                SelectedPlant = null;
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region For data loading

    private readonly List<PlantListItemViewModel> _allPlantViewModelsCache = [];

    /// <summary>
    /// When view is loading
    /// </summary>
    /// <returns></returns>
    public override async Task LoadDataWhenViewAppearingAsync()
    {
        try
        {
            // Load only once
            if (Plants.Count == 0)
            {
                await LoadAllPlantsFromDatabaseAsync();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }

    private async Task LoadAllPlantsFromDatabaseAsync()
    {
        // Load from DB
        List<Plant> plantListDatabase = await _plantService.GetAllPlantsAsync();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Plants.Clear();
            // Cache
            _allPlantViewModelsCache.Clear();
            foreach (Plant plantDB in plantListDatabase)
            {
                PlantListItemViewModel plantVM = MapToViewModel(plantDB);
                _allPlantViewModelsCache.Add(plantVM);
                Plants.Add(plantVM);
            }
        });

        _logger.LogInformation($"{_allPlantViewModelsCache.Count} plants are loaded.");

        // Set up notifications
        if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (await _notificationService.AreNotificationsEnabled() == false)
                {
                    await _notificationService.RequestNotificationPermission();
                }
            });

            IList<NotificationRequest> pendingNotifications = await _notificationService.GetPendingNotificationList();
            if (pendingNotifications.Count > 0)
            {
                _notificationService.CancelAll();
                _logger.LogInformation($"{pendingNotifications.Count} pending notifications cancelled.");
            }

            if (await _settingsService.GetWateringNotificationSettingAsync())
            {
                await ScheduleNotifications(CareType.Watering);
            }
            if (await _settingsService.GetFertilizationNotificationSettingAsync())
            {
                await ScheduleNotifications(CareType.Fertilization);
            }
        }
    }

    private async Task ResetPlantsAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Plants.Clear();

            foreach (PlantListItemViewModel item in _allPlantViewModelsCache)
            {
                Plants.Add(item);
            }
        });
    }

    #endregion For data loading

    [RelayCommand]
    private async Task AddPlant()
    {
        if (IsBusy)
            return;

        try
        {
            await _navigationService.GoToAddPlant(Plants.Count + 1);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region Search methods

    [RelayCommand]
    private async Task Search()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            IsLoading = true;

            if (!string.IsNullOrEmpty(SearchText))
            {
                List<PlantListItemViewModel> searchedPlants = await SearchPlantsByNameAsync(Plants, SearchText);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Plants.Clear();
                    foreach (PlantListItemViewModel plant in searchedPlants)
                    {
                        Plants.Add(plant);
                    }
                });
            }
            else
            {
                await ResetPlantsAsync();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = false;
                IsBusy = false;
            });
        }
    }

    /// <summary>
    /// Reset search when search text is empty
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SearchTextChanged()
    {
        if (SearchText.Length == 0)
        {
            try
            {
                IsLoading = true;
                await ResetPlantsAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsLoading = false; IsBusy = false;
                });
            }
        }
    }

    private static Task<List<PlantListItemViewModel>> SearchPlantsByNameAsync(IEnumerable<PlantListItemViewModel> plants, string searchText)
    {
        return Task.Run(() =>
        {
            List<PlantListItemViewModel> filtered = [];
            foreach (PlantListItemViewModel item in plants)
            {
                if (item.Name.Contains(searchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        });
    }

    #endregion Search methods

    /// <summary>
    /// TODO: need performance optimization
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<PlantAddedMessage>.Receive(PlantAddedMessage message)
    {
        if (message is null || message.PlantId == default)
        {
            return;
        }

        try
        {
            Plant? plantDB = await _plantService.GetPlantByIdAsync(message.PlantId);
            if (plantDB is null)
            {
                return;
            }
            PlantListItemViewModel newPlantVM = MapToViewModel(plantDB);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = true;

                InsertPlant(newPlantVM);

                _allPlantViewModelsCache.Add(newPlantVM);
                _allPlantViewModelsCache.Sort((x, y) => x.Name.CompareTo(y.Name));
                var toast = Toast.Make($"{plantDB.Name} {LocalizationManager.Instance[ConstStrings.Added] ?? ConstStrings.Added}", CommunityToolkit.Maui.Core.ToastDuration.Short);
                toast.Show();
            });

            if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
            {
                await ScheduleNotificationAsync(CareType.Watering, CareType.Watering.GetActionName(), newPlantVM);
                await ScheduleNotificationAsync(CareType.Fertilization, CareType.Fertilization.GetActionName(), newPlantVM);
            }

            _logger.LogInformation($"New plant {plantDB.Name} is added, with id: {message.PlantId}");
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = false;
            });
        }
    }

    private void InsertPlant(PlantListItemViewModel newPlantVM)
    {
        int i = 0;
        for (i = 0; i < Plants.Count; ++i)
        {
            // greater
            if (newPlantVM.Name.CompareTo(Plants[i].Name) <= 0)
            {
                break;
            }
        }
        Plants.Insert(i, newPlantVM);
    }

    async void IRecipient<PlantModifiedMessage>.Receive(PlantModifiedMessage message)
    {
        if (message is null || message.PlantId == default)
        {
            return;
        }

        IsLoading = true;
        try
        {
            PlantListItemViewModel? plantVM = Plants.FirstOrDefault(e => e.Id == message.PlantId);
            if (plantVM is null) { return; }

            Plant? plantDB = await _plantService.GetPlantByIdAsync(message.PlantId);
            if (plantDB is null) { return; }

            string originalName = plantVM.Name;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                UpdatePlantViewModel(plantVM, plantDB);

                // Reorder needed if name changed
                if (originalName.CompareTo(plantVM.Name) != 0)
                {
                    Plants.Remove(plantVM);

                    InsertPlant(plantVM);
                }

                var toast = Toast.Make($"{plantDB.Name} {LocalizationManager.Instance[ConstStrings.Updated] ?? ConstStrings.Updated}", CommunityToolkit.Maui.Core.ToastDuration.Short);

                toast.Show();
            });

            _logger.LogInformation($"Plant {plantVM.Name} is modified.");

            await CancelPendingNotificationAsync(plantVM.Id, plantVM.Name);
            await ScheduleNotificationAsync(CareType.Watering, CareType.Watering.GetActionName(), plantVM);
            await ScheduleNotificationAsync(CareType.Fertilization, CareType.Fertilization.GetActionName(), plantVM);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = false;
            });
        }
    }

    /// <summary>
    /// Delete a plant
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<PlantDeletedMessage>.Receive(PlantDeletedMessage message)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = true;

                PlantListItemViewModel? deletedPlant = Plants.FirstOrDefault(e => e.Id == message.PlantId);

                if (deletedPlant is null) { return; }
                Plants.Remove(deletedPlant);

                _allPlantViewModelsCache.Remove(deletedPlant);
                var toast = Toast.Make($"{deletedPlant.Name} {LocalizationManager.Instance[ConstStrings.Deleted] ?? ConstStrings.Deleted}", CommunityToolkit.Maui.Core.ToastDuration.Short);
                toast.Show();
            });

            _logger.LogInformation($"Plant {message.Name} is deleted");
            await CancelPendingNotificationAsync(message.PlantId, message.Name);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = false;
            });
        }
    }

    #region Refresh plant states

    [ObservableProperty]
    private bool _isPlantStatesRefreshing = false;

    [RelayCommand]
    public async Task RefreshPlantStates()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = false;

            if (Plants.Count == 0) return;

            foreach (PlantListItemViewModel plant in Plants)
            {
                plant.RefreshStates();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            IsPlantStatesRefreshing = false;
            IsBusy = false;
        }
    }

    #endregion Refresh plant states

    #region Notification methods

    private readonly List<int> _wateringNotificationIds = [];
    private readonly List<int> _fertilizationNotificationIds = [];

    private async Task ScheduleNotifications(CareType careType)
    {
        if (!_notificationService.IsSupported && !DeviceService.IsLocalNotificationSupported())
            return;

        string actionName = careType.GetActionName();

        switch (careType)
        {
            case CareType.Watering:
                {
                    _wateringNotificationIds.Clear();
                }
                break;

            case CareType.Fertilization:
                {
                    _fertilizationNotificationIds.Clear();
                }
                break;

            default:
                break;
        }

        for (int i = 0; i < Plants.Count; i++)
        {
            PlantListItemViewModel plant = Plants[i];
            int notificationId = await ScheduleNotificationAsync(careType, actionName, plant);

            if (notificationId > 0)
            {
                switch (careType)
                {
                    case CareType.Watering:
                        _wateringNotificationIds.Add(notificationId);
                        break;

                    case CareType.Fertilization:
                        _fertilizationNotificationIds.Add(notificationId);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private async Task<int> ScheduleNotificationAsync(CareType reminderType, string actionName, PlantListItemViewModel plant)
    {
        if (!_notificationService.IsSupported && !DeviceService.IsLocalNotificationSupported())
        {
            return 0;
        }
        try
        {
            string title = $"Remember to {actionName} Your Plant: {plant.Name}";

            int notificationId = GetNotificationId(reminderType, plant.Id);

            DateTime? scheduledTime = null;
            switch (reminderType)
            {
                case CareType.Watering:
                    scheduledTime = plant.NextWateringTime;
                    break;

                case CareType.Fertilization:
                    scheduledTime = plant.NextFertilizeTime;
                    break;

                default:
                    break;
            }

            if (scheduledTime == null || scheduledTime < DateTime.Now)
                return 0;

            // Data to be returned by the notification
            var list = new List<string>
                {
                    notificationId.ToString(),
                    plant.Id.ToString(),
                    plant.Name,
                };
            string serializeReturningData = JsonSerializer.Serialize(list);

            var notificationRequest = new NotificationRequest
            {
                NotificationId = notificationId,
                Title = title,
                Description = $"Scheduled {reminderType} Time: {scheduledTime}",
                ReturningData = serializeReturningData,
                Group = AndroidOptions.DefaultGroupId,
                Schedule =
                {
                    NotifyTime = scheduledTime,
                    //RepeatType = NotificationRepeat.TimeInterval,
                    //NotifyRepeatInterval = TimeSpan.FromSeconds(10),
                }
            };

            // Send a local notification to the device
            await _notificationService.Show(notificationRequest);

            _logger.LogInformation($"Notification scheduled (Notification Id = {notificationId}): {plant.Name} - {reminderType}, {scheduledTime}");

            return notificationId;
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");

            return 0;
        }
    }

    async void IRecipient<IsNotificationEnabledMessage>.Receive(IsNotificationEnabledMessage message)
    {
        if (!_notificationService.IsSupported && !DeviceService.IsLocalNotificationSupported())
        {
            return;
        }

        _notificationService.CancelAll();

        switch (message.ReminderType)
        {
            case CareType.Watering:
                {
                    if (message.IsNotificationEnabled)
                    {
                        await ScheduleNotifications(CareType.Watering);
                    }
                }
                break;

            case CareType.Fertilization:
                {
                    if (message.IsNotificationEnabled)
                    {
                        await ScheduleNotifications(CareType.Fertilization);
                    }
                }
                break;

            default:
                break;
        }
    }

    private async void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            //var logMessage = new StringBuilder();
            //logMessage.AppendLine($"{Environment.NewLine}ActionId {e.ActionId} {DateTime.Now}");

            // Notification is cancelled
            if (e.IsDismissed)
            {
                //logMessage.AppendLine($"{Environment.NewLine}Dismissed {DateTime.Now}");
                //MainThread.BeginInvokeOnMainThread(() =>
                //{
                await _dialogService.Notify("Notification Dismissed", e.Request.Title, "OK");
                //});
                return;
            }

            // Notification is tapped
            if (e.IsTapped)
            {
                _logger.LogInformation("Notification tapped ..");

                //logMessage.AppendLine($"{Environment.NewLine}Tapped {DateTime.Now}");
                if (e.Request is null)
                {
                    //MainThread.BeginInvokeOnMainThread(() =>
                    //{
                    //    _dialogService.Notify(e.Request.Title, $"No Request", "OK");
                    //});
                    return;
                }

                // No need to use NotificationSerializer, you can use your own one.
                var list = JsonSerializer.Deserialize<List<string>>(e.Request.ReturningData);
                if (list is null || list.Count != 3)
                {
                    //await _dialogService.Notify(e.Request.Title, $"No ReturningData {e.Request.ReturningData}", "OK");
                    return;
                }

                var notificationId = list[0];
                var plantId = list[1];
                string plantName = list[2];

                _logger.LogInformation($"Notification tapped: Notification Id = {notificationId}, Plant {plantName}, Id = {plantId}");

                await _navigationService.GoToPlantDetail(Guid.Parse(plantId));

                return;
            }

            //switch (e.ActionId)
            //{
            //    case 100:
            //        //logMessage.AppendLine($"{Environment.NewLine}Hello {DateTime.Now}");

            //        MainThread.BeginInvokeOnMainThread(() =>
            //        {
            //            _dialogService.Notify(e.Request.Title, "Hello Button was Tapped", "OK");
            //        });

            //        _notificationService.Cancel(e.Request.NotificationId);
            //        break;

            //    case 101:
            //        //logMessage.AppendLine($"{Environment.NewLine}Cancel {DateTime.Now}");
            //        _notificationService.Cancel(e.Request.NotificationId);
            //        break;
            //}

            //await File.AppendAllTextAsync(_cacheFilePath, $"{Environment.NewLine}Cancel {DateTime.Now}");
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static int GetNotificationId(CareType reminderType, Guid plantId)
    {
        if (plantId == default)
        {
            return 0;
        }

        int notificationId = plantId.GetHashCode();

        switch (reminderType)
        {
            case CareType.Watering:
                break;

            case CareType.Fertilization:
                notificationId = -notificationId;
                break;
        }

        return notificationId;
    }

    private async Task CancelPendingNotificationAsync(Guid plantId, string name)
    {
        if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
        {
            int noticeId = plantId.GetHashCode();

            await CancelPendingNotificationAsync(noticeId, name);
            await CancelPendingNotificationAsync(-noticeId, name);
        }
    }

    private async Task CancelPendingNotificationAsync(int noticeId, string name)
    {
        if (!_notificationService.IsSupported && !DeviceService.IsLocalNotificationSupported())
            return;

        List<int> notificationIds = await GetPendingNotificationIdsAsync();
        if (notificationIds.Contains(noticeId))
        {
            _notificationService.Cancel([noticeId]);

            _logger.LogInformation($"Cancel pending notification for Plant {name}, notification Id = {noticeId}");
        }
    }

    private async Task<List<int>> GetPendingNotificationIdsAsync()
    {
        if (!_notificationService.IsSupported && !DeviceService.IsLocalNotificationSupported())
        {
            return [];
        }

        List<int> notificationIds = (await _notificationService.GetPendingNotificationList())
                .Select(x => x.NotificationId).ToList();

        return notificationIds;
    }

    #endregion Notification methods

    private static PlantListItemViewModel MapToViewModel(Plant plant)
    {
        PlantListItemViewModel plantVM = new();
        UpdatePlantViewModel(plantVM, plant);

        return plantVM;
    }

    private static void UpdatePlantViewModel(PlantListItemViewModel plantVM, Plant plant)
    {
        plantVM.Id = plant.Id;

        plantVM.Name = plant.Name;
        plantVM.PhotoPath = plant.PhotoPath;

        plantVM.LastWatered = plant.LastWatered;
        plantVM.WateringFrequencyInHours = plant.WateringFrequencyInHours;

        plantVM.LastFertilized = plant.LastFertilized;
        plantVM.FertilizeFrequencyInHours = plant.FertilizeFrequencyInHours;
    }

    /// <summary>
    /// Mark Watering/Fertilization done
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="NotImplementedException"></exception>
    async void IRecipient<PlantStateChangedMessage>.Receive(PlantStateChangedMessage message)
    {
        if (null == message)
        {
            return;
        }

        try
        {
            IsLoading = true;

            PlantListItemViewModel? updatePlant = Plants.FirstOrDefault(x => x.Id == message.PlantId);
            if (updatePlant is null)
                return;

            Plant? plantFromDB = await _plantService.GetPlantByIdAsync(message.PlantId);
            if (null == plantFromDB)
                return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                switch (message.ReminderType)
                {
                    case CareType.Watering:
                        {
                            //updatePlant.LastWatered = message.UpdatedTime;
                            updatePlant.LastWatered = plantFromDB.LastWatered;
                        }
                        break;

                    case CareType.Fertilization:
                        {
                            //updatePlant.LastFertilized = message.UpdatedTime;
                            updatePlant.LastFertilized = plantFromDB.LastFertilized;
                        }
                        break;

                    default:
                        break;
                }
            });

            _logger.LogInformation($"Status changed {message.ReminderType}: Plant {updatePlant.Name}.");
            if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
            {
                int noticeId = GetNotificationId(message.ReminderType, updatePlant.Id);

                await CancelPendingNotificationAsync(noticeId, plantFromDB.Name);
                await ScheduleNotificationAsync(message.ReminderType, message.ReminderType.GetActionName(), updatePlant);
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

    async void IRecipient<PlantCareHistoryChangedMessage>.Receive(PlantCareHistoryChangedMessage message)
    {
        try
        {
            if (message is null) return;

            Guid plantId = message.PlantId;
            CareType careType = message.CareType;

            Plant? plant = await _plantService.GetPlantByIdAsync(plantId);
            if (plant is null) return;

            int plantVMIndex = _allPlantViewModelsCache.FindIndex(x => x.Id == plantId);
            if (plantVMIndex == -1) return;

            PlantListItemViewModel plantVM = _allPlantViewModelsCache[plantVMIndex];

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                UpdatePlantViewModel(plantVM, plant);
            });
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }

    async void IRecipient<DataImportMessage>.Receive(DataImportMessage message)
    {
        IsLoading = true;
        try
        {
            Plants.Clear();
            await LoadDataWhenViewAppearingAsync();
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

    #region Layout related

    [ObservableProperty]
    private int _photoWidth = 110;

    [ObservableProperty]
    private int _photoHeight = 120;

    [ObservableProperty]
    private int _photoSpan = 3;

    private void AdjustSpan()
    {
        DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
        double width = mainDisplayInfo.Width / mainDisplayInfo.Density;

        PhotoSpan = ((int)width - 10) / PhotoWidth;
    }

    private void OnDeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        AdjustSpan();
    }

    public void Dispose()
    {
        DeviceDisplay.MainDisplayInfoChanged -= OnDeviceDisplay_MainDisplayInfoChanged;
    }

    #endregion Layout related
}