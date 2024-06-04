using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;

using System.Collections.ObjectModel;
using PlantCare.App.ViewModels.Base;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.Data.Models;
using Plugin.LocalNotification;
using PlantCare.App.Views;
using Plugin.LocalNotification.AndroidOption;
using System.Text.Json;
using Plugin.LocalNotification.EventArgs;
using System.Diagnostics;

namespace PlantCare.App.ViewModels;

public partial class PlantListOverviewViewModel : ViewModelBase,
    IRecipient<PlantAddedOrChangedMessage>,
    IRecipient<PlantDeletedMessage>,
    IRecipient<IsNotificationEnabledMessage>,
    IRecipient<PlantEventStatusChangedMessage>
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settingsService;

    public PlantListOverviewViewModel(IPlantService plantService, INavigationService navigationService, INotificationService notificationService, IDialogService dialogService, ISettingsService settingsService)
    {
        _plantService = plantService;

        _navigationService = navigationService;
        _notificationService = notificationService;
        _dialogService = dialogService;
        _settingsService = settingsService;

        WeakReferenceMessenger.Default.Register<PlantAddedOrChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantDeletedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantEventStatusChangedMessage>(this);

        if (_notificationService.IsSupported)
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

    private async Task SetAllPlants()
    {
        try
        {
            Plants.Clear();
            _allPlantViewModelsCache.Clear();

            foreach (Plant plant in _plantListDatabase)
            {
                Plants.Add(MapToViewModel(plant));
            }

            _allPlantViewModelsCache.AddRange(Plants);

            if (_notificationService.IsSupported)
            {
                _notificationService.Cancel();

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
            Plants.Clear();
            foreach (PlantListItemViewModel item in _allPlantViewModelsCache)
            {
                Plants.Add(item);
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
    async void IRecipient<PlantAddedOrChangedMessage>.Receive(PlantAddedOrChangedMessage message)
    {
        try
        {
            await LoadAllPlantsFromDatabase();
            await SetAllPlants();
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
            if (deletedPlant != null)
            {
                Plants.Remove(deletedPlant);
            }

            if (_notificationService.IsSupported)
            {
                int noticeId = message.PlantId.GetHashCode();

                await CancelPendingNotificationAsync(noticeId);
                await CancelPendingNotificationAsync(-noticeId);
            }
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
        if (!_notificationService.IsSupported)
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
        try
        {
            string title = $"Remember to {actionName} Your Plant: {plant.Name}";

            int notificationId = GetNotificationId(reminderType, plant.Id);

            ReminderType[] reminderTypes = Enum.GetValues(typeof(ReminderType)).Cast<ReminderType>().ToArray();

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
                    typeof(NotificationPage).FullName ?? "NotificationPage",
                    notificationId.ToString(),
                    plant.Id.ToString(),
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

            if (await _notificationService.AreNotificationsEnabled() == false)
            {
                await _notificationService.RequestNotificationPermission();
            }

            // Send a local notification to the device
            await _notificationService.Show(notificationRequest);

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
        if (!_notificationService.IsSupported)
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
                    await _dialogService.Notify(e.Request.Title, $"No ReturningData {e.Request.ReturningData}", "OK");
                    return;
                }

                if (list[0] != typeof(NotificationPage).FullName)
                {
                    await _dialogService.Notify(e.Request.Title, $"Not NotificationPage", "OK");
                    return;
                }

                var notificationId = list[1];
                var plantId = list[2];

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

    public static PlantListItemViewModel MapToViewModel(Plant plant)
    {
        return new PlantListItemViewModel
        {
            Id = plant.Id,
            Name = plant.Name,
            PhotoPath = plant.PhotoPath,

            LastWatered = plant.LastWatered,
            WateringFrequencyInHours = plant.WateringFrequencyInHours,

            LastFertilized = plant.LastFertilized,
            FertilizeFrequencyInHours = plant.FertilizeFrequencyInHours
        };
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

            if (_notificationService.IsSupported)
            {
                int noticeId = GetNotificationId(message.ReminderType, updatePlant.Id);

                await CancelPendingNotificationAsync(noticeId);
                await CancelPendingNotificationAsync(-noticeId);

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

    private async Task CancelPendingNotificationAsync(int noticeId)
    {
        List<int> notificationIds = await GetPendingNotificationIdsAsync();
        if (notificationIds.Contains(noticeId))
        {
            _notificationService.Cancel([noticeId]);
        }
    }

    private async Task<List<int>> GetPendingNotificationIdsAsync()
    {
        if (_notificationService.IsSupported)
        {
            List<int> notificationIds = (await _notificationService.GetPendingNotificationList())
                .Select(x => x.NotificationId).ToList();

            return notificationIds;
        }

        return [];
    }
}