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
using INotificationService = PlantCare.App.Services.INotificationService;

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
    private readonly IInAppToastService _inAppToastService;

    public PlantListOverviewViewModel(
        IPlantService plantService,
        INavigationService navigationService,
        INotificationService notificationService,
        IDialogService dialogService,
        IAppSettingsService settingsService,
        IAppLogger<PlantListOverviewViewModel> logger,
        IInAppToastService inAppToastService)
    {
        _plantService = plantService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _dialogService = dialogService;
        _settingsService = settingsService;
        _logger = logger;
        _inAppToastService = inAppToastService;

        WeakReferenceMessenger.Default.Register<PlantAddedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantModifiedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantDeletedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantStateChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantCareHistoryChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<DataImportMessage>(this);

        if (_notificationService.IsSupported)
        {
            WeakReferenceMessenger.Default.Register<IsNotificationEnabledMessage>(this);

            _notificationService.NotificationActionTapped += OnNotificationActionTapped;
        }

        AdjustSpan();
        DeviceDisplay.MainDisplayInfoChanged += OnDeviceDisplay_MainDisplayInfoChanged;
    }

    [ObservableProperty]
    private ObservableCollection<PlantListItemViewModel> _plants = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    private CancellationTokenSource? _searchCts;

    partial void OnSearchTextChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        if (string.IsNullOrWhiteSpace(value))
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100, _searchCts.Token);
                    await SetLoadingStateAsync(true);
                    await ResetPlantsAsync();
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                    await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
                }
                finally
                {
                    await SetLoadingStateAsync(false);
                }
            });
        }
        else
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, _searchCts.Token);
                    await PerformSearchAsync(value, _searchCts.Token);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                    await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
                }
            });
        }
    }

    [ObservableProperty]
    private PlantListItemViewModel? _selectedPlant = null;

    [RelayCommand]
    private async Task SelectPlant()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            await SetBusyStateAsync(true);
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
            await SetBusyStateAsync(false);
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
            // Cache
            _allPlantViewModelsCache.Clear();

            List<PlantListItemViewModel> newPlantVMs = new(plantListDatabase.Count);
            foreach (Plant plantDB in plantListDatabase)
            {
                PlantListItemViewModel plantVM = MapToViewModel(plantDB);
                _allPlantViewModelsCache.Add(plantVM);
                newPlantVMs.Add(plantVM);
            }

            Plants = new ObservableCollection<PlantListItemViewModel>(newPlantVMs);
        });

        _logger.LogInformation($"{_allPlantViewModelsCache.Count} plants are loaded.");

        // Set up notifications
        if (_notificationService.IsSupported)
        {
            // Cleanup old delivered notifications
            await CleanupDeliveredNotificationsAsync(30);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (await _notificationService.AreNotificationsEnabled() == false)
                {
                    // Request notification permission with platform-specific options
                    var permission = new NotificationPermission
                    {
#if ANDROID
                        Android = new AndroidNotificationPermission
                        {
                            RequestPermissionToScheduleExactAlarm = true
                        }
#endif
                    };

                    bool result = await _notificationService.RequestNotificationPermission(permission);
                    if (!result)
                    {
                        _logger.LogWarning("User denied notification permission");
                    }
                }
            });

            IList<NotificationRequest> pendingNotifications = await _notificationService.GetPendingNotificationList();
            if (pendingNotifications.Count == 0)
            {
                if (await _settingsService.GetWateringNotificationSettingAsync())
                {
                    await ScheduleNotificationsAsync(CareType.Watering);
                }
                if (await _settingsService.GetFertilizationNotificationSettingAsync())
                {
                    await ScheduleNotificationsAsync(CareType.Fertilization);
                }
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
        {
            return;
        }

        try
        {
            await SetBusyStateAsync(true);
            await _navigationService.GoToAddPlant(Plants.Count + 1);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            await SetBusyStateAsync(false);
        }
    }

    #region Search methods

    private async Task PerformSearchAsync(string searchText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            await ResetPlantsAsync();
            return;
        }

        try
        {
            await SetLoadingStateAsync(true);

            List<PlantListItemViewModel> searchedPlants = await SearchPlantsByNameAsync(_allPlantViewModelsCache, searchText, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Plants = new ObservableCollection<PlantListItemViewModel>(searchedPlants);
                });
            }
        }
        finally
        {
            await SetLoadingStateAsync(false);
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            await SetBusyStateAsync(true);
            await SetLoadingStateAsync(true);

            if (!string.IsNullOrEmpty(SearchText))
            {
                List<PlantListItemViewModel> searchedPlants = await SearchPlantsByNameAsync(_allPlantViewModelsCache, SearchText, CancellationToken.None);

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
            await SetLoadingStateAsync(false);
            await SetBusyStateAsync(false);
        }
    }

    private static Task<List<PlantListItemViewModel>> SearchPlantsByNameAsync(IEnumerable<PlantListItemViewModel> plants, string searchText, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            List<PlantListItemViewModel> filtered = [];

            foreach (PlantListItemViewModel item in plants)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (item.Name.Contains(searchText.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    filtered.Add(item);
                }
            }

            return filtered;
        }, cancellationToken);
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
            });

            if (_notificationService.IsSupported)
            {
                Task<int>[] notificationTasks = new[]
                {
                    ScheduleNotificationAsync(CareType.Watering, CareType.Watering.GetActionName(), newPlantVM),
                    ScheduleNotificationAsync(CareType.Fertilization, CareType.Fertilization.GetActionName(), newPlantVM)
                };

                _ = Task.WhenAll(notificationTasks).ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        _logger.LogError($"Failed to schedule notifications for plant {message.PlantId}", t.Exception);
                    }
                }, TaskScheduler.Default);
            }

            _logger.LogInformation($"New plant {plantDB.Name} is added, with id: {message.PlantId}");
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            await Task.Delay(100);
            await SetLoadingStateAsync(false);
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

        try
        {
            await SetLoadingStateAsync(true);

            PlantListItemViewModel? plantVM = Plants.FirstOrDefault(e => e.Id == message.PlantId);
            if (plantVM is null)
            { return; }

            Plant? plantDB = await _plantService.GetPlantByIdAsync(message.PlantId);
            if (plantDB is null)
            { return; }

            string originalName = plantVM.Name;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                UpdatePlantViewModel(plantVM, plantDB);

                if (originalName.CompareTo(plantVM.Name) != 0)
                {
                    Plants.Remove(plantVM);

                    InsertPlant(plantVM);
                }

                int cacheIndex = _allPlantViewModelsCache.FindIndex(x => x.Id == plantVM.Id);
                if (cacheIndex >= 0)
                {
                    _allPlantViewModelsCache[cacheIndex] = plantVM;
                    if (originalName.CompareTo(plantVM.Name) != 0)
                    {
                        _allPlantViewModelsCache.Sort((x, y) => x.Name.CompareTo(y.Name));
                    }
                }
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
            await SetLoadingStateAsync(false);
        }
    }

    /// <summary>
    /// Delete a plant
    /// </summary>
    /// <param name="message"></param>
    async void IRecipient<PlantDeletedMessage>.Receive(PlantDeletedMessage message)
    {
        string? deletedName = null;

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                IsLoading = true;

                PlantListItemViewModel? deletedPlant = Plants.FirstOrDefault(e => e.Id == message.PlantId);

                if (deletedPlant is null)
                { return; }

                deletedName = deletedPlant.Name;

                Plants.Remove(deletedPlant);

                _allPlantViewModelsCache.Remove(deletedPlant);
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
            await SetLoadingStateAsync(false);

            // Show toast after loading is complete
            if (!string.IsNullOrWhiteSpace(deletedName))
            {
                // Small delay to ensure loading overlay is fully hidden
                await Task.Delay(100);

                string deleteMessage = $"{deletedName} {LocalizationManager.Instance[ConstStrings.Deleted] ?? ConstStrings.Deleted}";
                await _inAppToastService.ShowSuccessAsync(deleteMessage);
            }
        }
    }

    #region Refresh plant states

    [ObservableProperty]
    private bool _isPlantStatesRefreshing = false;

    [RelayCommand]
    public async Task RefreshPlantStates()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            await SetBusyStateAsync(true);

            if (Plants.Count == 0)
            {
                return;
            }

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
            await SetBusyStateAsync(false);
        }
    }

    #endregion Refresh plant states

    #region Notification methods

    /// <summary>
    /// Cleans up delivered notifications older than the specified number of days
    /// </summary>
    /// <param name="daysOld">Number of days to keep notifications (default: 7)</param>
    private async Task CleanupDeliveredNotificationsAsync(int daysOld = 7)
    {
        if (!_notificationService.IsSupported)
        {
            return;
        }

        try
        {
            IList<NotificationRequest> deliveredNotifications = await _notificationService.GetDeliveredNotificationList();
            if (deliveredNotifications.Count == 0)
            {
                return;
            }

            // Remove notifications older than the specified number of days
            DateTime cutoffTime = DateTime.Now.AddDays(-daysOld);
            int[] oldNotificationIds = deliveredNotifications
                .Where(n => n.Schedule?.NotifyTime < cutoffTime)
                .Select(n => n.NotificationId)
                .ToArray();

            if (oldNotificationIds.Length > 0)
            {
                _notificationService.Cancel(oldNotificationIds);
                _logger.LogInformation($"Cleaned up {oldNotificationIds.Length} old notifications (older than {daysOld} days)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to cleanup delivered notifications", ex);
        }
    }

    private async Task ScheduleNotificationsAsync(CareType careType)
    {
        if (!_notificationService.IsSupported)
        {
            return;
        }

        string actionName = careType.GetActionName();

        for (int i = 0; i < Plants.Count; i++)
        {
            PlantListItemViewModel plant = Plants[i];
            int notificationId = await ScheduleNotificationAsync(careType, actionName, plant);
        }
    }

    private async Task<int> ScheduleNotificationAsync(CareType reminderType, string actionName, PlantListItemViewModel plant)
    {
        if (!_notificationService.IsSupported)
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
            {
                return 0;
            }

            // Data to be returned by the notification
            var list = new List<string>
                {
                    notificationId.ToString(),
                    plant.Id.ToString(),
                    plant.Name,
                };
            string serializeReturningData = JsonSerializer.Serialize(list);

            NotificationRequest notificationRequest = new()
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
                        await ScheduleNotificationsAsync(CareType.Watering);
                    }
                }
                break;

            case CareType.Fertilization:
                {
                    if (message.IsNotificationEnabled)
                    {
                        await ScheduleNotificationsAsync(CareType.Fertilization);
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
            // Notification is cancelled
            if (e.IsDismissed)
            {
                await _dialogService.Notify("Notification Dismissed", e.Request.Title, "OK");
                return;
            }

            // Notification is tapped
            if (e.IsTapped)
            {
                _logger.LogInformation("Notification tapped ..");

                if (e.Request is null)
                {
                    _logger.LogWarning("Notification tap event has no request");
                    return;
                }

                if (e.Request.ReturningData == null)
                {
                    _logger.LogWarning("Notification tap event has no returning data");
                    return;
                }

                // Deserialize notification returning data
                List<string>? list = null;
                try
                {
                    list = JsonSerializer.Deserialize<List<string>>(e.Request.ReturningData);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    _logger.LogError("Failed to deserialize notification returning data", ex);
                    await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error,
                        "Invalid notification data format", "OK");
                    return;
                }

                if (list is null || list.Count != 3)
                {
                    _logger.LogWarning($"Invalid returning data format, expected 3 items, got {list?.Count ?? 0}");
                    return;
                }

                string notificationId = list[0];
                string plantId = list[1];
                string plantName = list[2];

                _logger.LogInformation($"Notification tapped: Notification Id = {notificationId}, Plant {plantName}, Id = {plantId}");

                await _navigationService.GoToPlantDetail(Guid.Parse(plantId));

                return;
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

    private const int WateringNotificationBase = 1000;
    private const int FertilizationNotificationBase = 2000;

    /// <summary>
    /// Provides ~900 different IDs per care type
    /// </summary>
    /// <param name="reminderType"></param>
    /// <param name="plantId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static int GetNotificationId(CareType reminderType, Guid plantId)
    {
        if (plantId == default)
        {
            return 0;
        }

        // Ensure positive hash code and limit to reasonable range to avoid collisions
        int hashCode = plantId.GetHashCode() & 0x7FFFFFFF;

        return reminderType switch
        {
            CareType.Watering => WateringNotificationBase + (hashCode % 900),
            CareType.Fertilization => FertilizationNotificationBase + (hashCode % 900),
            _ => throw new ArgumentException($"Unknown care type: {reminderType}")
        };
    }

    private async Task CancelPendingNotificationAsync(Guid plantId, string name)
    {
        if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
        {
            int wateringId = GetNotificationId(CareType.Watering, plantId);
            int fertilizationId = GetNotificationId(CareType.Fertilization, plantId);

            await CancelPendingNotificationAsync(wateringId, name);
            await CancelPendingNotificationAsync(fertilizationId, name);
        }
    }

    private async Task CancelPendingNotificationAsync(int noticeId, string name)
    {
        if (!_notificationService.IsSupported)
        {
            return;
        }

        List<int> notificationIds = await GetPendingNotificationIdsAsync();
        if (notificationIds.Contains(noticeId))
        {
            _notificationService.Cancel([noticeId]);

            _logger.LogInformation($"Cancel pending notification for Plant {name}, notification Id = {noticeId}");
        }
    }

    private async Task<List<int>> GetPendingNotificationIdsAsync()
    {
        if (!_notificationService.IsSupported)
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

        string originPhotoPath = plantVM.PhotoPath;
        plantVM.PhotoPath = plant.PhotoPath;
        plantVM.ThumbnailPath = plant.ThumbnailPath;

        plantVM.LastWatered = plant.LastWatered;
        plantVM.WateringFrequencyInHours = plant.WateringFrequencyInHours;

        plantVM.LastFertilized = plant.LastFertilized;
        plantVM.FertilizeFrequencyInHours = plant.FertilizeFrequencyInHours;

        // delete old photo file if changed
        if (originPhotoPath != ConstStrings.DefaultPhotoPath
            && originPhotoPath != plantVM.PhotoPath
            && File.Exists(originPhotoPath))
        {
            File.Delete(originPhotoPath);
        }
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
            await SetLoadingStateAsync(true);

            PlantListItemViewModel? updatePlant = Plants.FirstOrDefault(x => x.Id == message.PlantId);
            if (updatePlant is null)
            {
                return;
            }

            Plant? plantFromDB = await _plantService.GetPlantByIdAsync(message.PlantId);
            if (null == plantFromDB)
            {
                return;
            }

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
            await SetLoadingStateAsync(false);
        }
    }

    async void IRecipient<PlantCareHistoryChangedMessage>.Receive(PlantCareHistoryChangedMessage message)
    {
        try
        {
            if (message is null)
            {
                return;
            }

            Guid plantId = message.PlantId;
            CareType careType = message.CareType;

            Plant? plant = await _plantService.GetPlantByIdAsync(plantId);
            if (plant is null)
            {
                return;
            }

            int plantVMIndex = _allPlantViewModelsCache.FindIndex(x => x.Id == plantId);
            if (plantVMIndex == -1)
            {
                return;
            }

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
        try
        {
            await SetLoadingStateAsync(true);

            Plants.Clear();
            await LoadDataWhenViewAppearingAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            await SetLoadingStateAsync(false);
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

        // Calculate span based on device width
        // Phone: 3 columns, Tablet/Desktop: 4-6 columns max
        if (width < 600)
        {
            // Small phone
            PhotoSpan = 3;
            PhotoWidth = 110;
            PhotoHeight = 120;
        }
        else if (width < 900)
        {
            // Tablet or large phone
            PhotoSpan = 4;
            PhotoWidth = 140;
            PhotoHeight = 150;
        }
        else if (width < 1200)
        {
            // Small desktop
            PhotoSpan = 5;
            PhotoWidth = 160;
            PhotoHeight = 170;
        }
        else
        {
            // Large desktop - limit to 6 columns max
            PhotoSpan = 6;
            PhotoWidth = 180;
            PhotoHeight = 190;
        }
    }

    private void OnDeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
    {
        AdjustSpan();
    }

    #endregion Layout related

    #region IDisposable

    private bool _disposed = false;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Unsubscribe from device display changes
        DeviceDisplay.MainDisplayInfoChanged -= OnDeviceDisplay_MainDisplayInfoChanged;

        // Cancel and dispose search cancellation token
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = null;

        // Unsubscribe from notification events
        if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
        {
            _notificationService.NotificationActionTapped -= OnNotificationActionTapped;
        }

        // Unregister from all messenger messages
        WeakReferenceMessenger.Default.UnregisterAll(this);

        // Clear collections
        Plants.Clear();
        _allPlantViewModelsCache.Clear();
    }

    #endregion IDisposable
}
