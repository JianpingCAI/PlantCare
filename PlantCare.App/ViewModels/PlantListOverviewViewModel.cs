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
using System.Diagnostics;

namespace PlantCare.App.ViewModels;

public partial class PlantListOverviewViewModel : ViewModelBase,
    IRecipient<PlantModifiedMessage>,
    IRecipient<PlantAddedMessage>,
    IRecipient<PlantDeletedMessage>,
    IRecipient<IsNotificationEnabledMessage>,
    IRecipient<PlantEventStatusChangedMessage>
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly IAppLogger<PlantListOverviewViewModel> _logger;

    public PlantListOverviewViewModel(
        IPlantService plantService,
        INavigationService navigationService,
        INotificationService notificationService,
        IDialogService dialogService,
        ISettingsService settingsService,
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
        WeakReferenceMessenger.Default.Register<PlantEventStatusChangedMessage>(this);

        if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
        {
            WeakReferenceMessenger.Default.Register<IsNotificationEnabledMessage>(this);

            //_notificationService.NotificationReceiving = OnNotificationReceiving;
            //_notificationService.NotificationReceived += OnNotificationReceived;
            _notificationService.NotificationActionTapped += OnNotificationActionTapped;
        }
    }

    [ObservableProperty]
    private ObservableCollection<PlantListItemViewModel> _plants = [];

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
        finally
        {
            IsBusy = false;
        }
    }

    #region For data loading

    private List<Plant> _plantListDatabase = [];
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
                await LoadAllPlantsFromDatabase();
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    /// <summary>
    /// When view is loading, and after data is loaded from database
    /// </summary>
    /// <returns></returns>
    public override async Task OnDataLoadedWhenViewAppearingAsync()
    {
        if (Plants.Count == 0)
        {
            await SetAllPlants();
        }
    }

    private async Task LoadAllPlantsFromDatabase()
    {
        _plantListDatabase = await _plantService.GetAllPlantsAsync();
        _plantListDatabase = [.. _plantListDatabase.OrderBy(x => x.Name)];
    }

    private void ResetDisplayedPlants()
    {
        Plants.Clear();
        foreach (PlantListItemViewModel item in _allPlantViewModelsCache)
        {
            Plants.Add(item);
        }
    }

    private async Task SetAllPlants()
    {
        try
        {
            _allPlantViewModelsCache.Clear();
            foreach (Plant plantDB in _plantListDatabase)
            {
                _allPlantViewModelsCache.Add(MapToViewModel(plantDB));
            }

            _logger.LogInformation($"{_allPlantViewModelsCache.Count} plants are loaded.");

            ResetDisplayedPlants();

            if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
            {
                if (await _notificationService.AreNotificationsEnabled() == false)
                {
                    await _notificationService.RequestNotificationPermission();
                }

                IList<NotificationRequest> pendingNotifications = await _notificationService.GetPendingNotificationList();
                if( pendingNotifications.Count > 0)
                {
                    _notificationService.CancelAll();
                    _logger.LogInformation($"{pendingNotifications.Count} pending notifications cancelled.");
                }

                if (await _settingsService.GetWateringNotificationSettingAsync())
                {
                    await ScheduleNotifications(ReminderType.Watering);
                }
                if (await _settingsService.GetFertilizationNotificationSettingAsync())
                {
                    await ScheduleNotifications(ReminderType.Fertilization);
                }
            }
        }
        catch (Exception ex)
        {
            //Debug.WriteLine(ex.Message);
            await _dialogService.Notify("Error", ex.Message);
        }
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
            try
            {
                await _dialogService.Notify("Error", ex.Message);
                await _navigationService.GoToPlantsOverview();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
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
            if (!string.IsNullOrEmpty(SearchText))
            {
                List<PlantListItemViewModel> searchedPlants = await SearchPlantsByNameAsync(Plants, SearchText);

                Plants.Clear();
                foreach (PlantListItemViewModel plant in searchedPlants)
                {
                    Plants.Add(plant);
                }
            }
            else
            {
                ResetSearch();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    internal void ResetSearch()
    {
        if (_allPlantViewModelsCache.Count == 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(SearchText.Trim()))
        {
            ResetDisplayedPlants();
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
            InsertPlant(newPlantVM);

            _allPlantViewModelsCache.Add(newPlantVM);
            _allPlantViewModelsCache.Sort((x, y) => x.Name.CompareTo(y.Name));

            if (_notificationService.IsSupported && DeviceService.IsLocalNotificationSupported())
            {
                await ScheduleNotificationAsync(ReminderType.Watering, ReminderType.Watering.GetActionName(), newPlantVM);
                await ScheduleNotificationAsync(ReminderType.Fertilization, ReminderType.Fertilization.GetActionName(), newPlantVM);
            }

            _logger.LogInformation($"New plant {plantDB.Name} is added, with id: {message.PlantId}");
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
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
            PlantListItemViewModel? plantVM = Plants.FirstOrDefault(e => e.Id == message.PlantId);
            if (plantVM is null) { return; }

            Plant? plantDB = await _plantService.GetPlantByIdAsync(message.PlantId);
            if (plantDB is null) { return; }

            string originalName = plantVM.Name;

            UpdatePlantViewModel(plantVM, plantDB);

            // Reorder needed if name changed
            if (originalName.CompareTo(plantVM.Name) != 0)
            {
                Plants.Remove(plantVM);

                InsertPlant(plantVM);
            }

            _logger.LogInformation($"Plant {plantVM.Name} is modified.");

            await CancelPendingNotificationAsync(plantVM.Id, plantVM.Name);
            await ScheduleNotificationAsync(ReminderType.Watering, ReminderType.Watering.GetActionName(), plantVM);
            await ScheduleNotificationAsync(ReminderType.Fertilization, ReminderType.Fertilization.GetActionName(), plantVM);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    async void IRecipient<PlantDeletedMessage>.Receive(PlantDeletedMessage message)
    {
        try
        {
            PlantListItemViewModel? deletedPlant = Plants.FirstOrDefault(e => e.Id == message.PlantId);
            if (deletedPlant is null) { return; }

            Plants.Remove(deletedPlant);

            _logger.LogInformation($"Plant {deletedPlant.Name} is deleted");

            await CancelPendingNotificationAsync(message.PlantId, deletedPlant.Name);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    #region Notification methods

    private readonly List<int> _wateringNotificationIds = [];
    private readonly List<int> _fertilizationNotificationIds = [];

    private async Task ScheduleNotifications(ReminderType reminderType)
    {
        if (!_notificationService.IsSupported && !DeviceService.IsLocalNotificationSupported())
            return;

        string actionName = reminderType.GetActionName();

        switch (reminderType)
        {
            case ReminderType.Watering:
                {
                    _wateringNotificationIds.Clear();
                }
                break;

            case ReminderType.Fertilization:
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
            int notificationId = await ScheduleNotificationAsync(reminderType, actionName, plant);

            if (notificationId > 0)
            {
                switch (reminderType)
                {
                    case ReminderType.Watering:
                        _wateringNotificationIds.Add(notificationId);
                        break;

                    case ReminderType.Fertilization:
                        _fertilizationNotificationIds.Add(notificationId);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private async Task<int> ScheduleNotificationAsync(ReminderType reminderType, string actionName, PlantListItemViewModel plant)
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
                case ReminderType.Watering:
                    scheduledTime = plant.NextWateringTime;
                    break;

                case ReminderType.Fertilization:
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
            await _dialogService.Notify("Error", ex.Message, "OK");

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
            case ReminderType.Watering:
                {
                    if (message.IsNotificationEnabled)
                    {
                        await ScheduleNotifications(ReminderType.Watering);
                    }
                }
                break;

            case ReminderType.Fertilization:
                {
                    if (message.IsNotificationEnabled)
                    {
                        await ScheduleNotifications(ReminderType.Fertilization);
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
            await _dialogService.Notify("Error", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion Notification methods

    private static PlantListItemViewModel MapToViewModel(Plant plant)
    {
        PlantListItemViewModel plantVM = new();
        UpdatePlantViewModel(plantVM, plant);

        return plantVM;
    }

    private static void UpdatePlantViewModel(PlantListItemViewModel plantVM, Plant plantDB)
    {
        plantVM.Id = plantDB.Id;

        plantVM.Name = plantDB.Name;
        plantVM.PhotoPath = plantDB.PhotoPath;

        plantVM.LastWatered = plantDB.LastWatered;
        plantVM.WateringFrequencyInHours = plantDB.WateringFrequencyInHours;

        plantVM.LastFertilized = plantDB.LastFertilized;
        plantVM.FertilizeFrequencyInHours = plantDB.FertilizeFrequencyInHours;
    }

    /// <summary>
    /// Mark Watering/Fertilization done
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="NotImplementedException"></exception>
    async void IRecipient<PlantEventStatusChangedMessage>.Receive(PlantEventStatusChangedMessage message)
    {
        try
        {
            if (null == message)
            {
                return;
            }

            PlantListItemViewModel? updatePlant = Plants.FirstOrDefault(x => x.Id == message.PlantId);
            if (updatePlant is null)
                return;

            Plant? plantFromDB = await _plantService.GetPlantByIdAsync(message.PlantId);
            if (null == plantFromDB)
                return;

            switch (message.ReminderType)
            {
                case ReminderType.Watering:
                    {
                        //updatePlant.LastWatered = message.UpdatedTime;
                        updatePlant.LastWatered = plantFromDB.LastWatered;
                    }
                    break;

                case ReminderType.Fertilization:
                    {
                        //updatePlant.LastFertilized = message.UpdatedTime;
                        updatePlant.LastFertilized = plantFromDB.LastFertilized;
                    }
                    break;

                default:
                    break;
            }

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
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    private static int GetNotificationId(ReminderType reminderType, Guid id)
    {
        if (id == default)
        {
            return 0;
        }

        int notificationId = id.GetHashCode();

        switch (reminderType)
        {
            case ReminderType.Watering:
                break;

            case ReminderType.Fertilization:
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
}