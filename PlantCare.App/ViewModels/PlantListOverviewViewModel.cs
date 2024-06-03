using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;

using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
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
    IRecipient<IsWateringNotifyEnabledMessage>,
    IRecipient<ReminderItemChangedMessage>
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
        WeakReferenceMessenger.Default.Register<ReminderItemChangedMessage>(this);

        if (_notificationService.IsSupported)
        {
            WeakReferenceMessenger.Default.Register<IsWateringNotifyEnabledMessage>(this);

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

    private List<Plant> _plantList = [];

    // Override
    public override async Task LoadDataWhenViewAppearingAsync()
    {
        // Load only once
        if (Plants.Count == 0)
        {
            await LoadAllPlantsFromDatabase();
        }
    }

    public override async Task OnDataLoadedWhenViewAppearingAsync()
    {
        if (Plants.Count == 0)
        {
            await SetAllPlants();
        }
    }

    private async Task LoadAllPlantsFromDatabase()
    {
        _plantList = await _plantService.GetAllPlantsAsync();
        _plantList = [.. _plantList.OrderBy(x => x.Name)];
    }

    private async Task SetAllPlants()
    {
        try
        {
            Plants.Clear();
            foreach (Plant plant in _plantList)
            {
                Plants.Add(MapToViewModel(plant));
            }

            _allPlantsBackup.AddRange(Plants);

            if (await _settingsService.GetWateringNotificationSettingAsync())
            {
                await ScheduleNotifications(ReminderType.Watering);
            }
            if (await _settingsService.GetFertilizationNotificationSettingAsync())
            {
                await ScheduleNotifications(ReminderType.Fertilization);
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

    private readonly List<PlantListItemViewModel> _allPlantsBackup = [];

    [RelayCommand]
    private async Task Search()
    {
        if (IsBusy)
            return;

        try
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                List<PlantListItemViewModel> searchedPlants = await FilterPlantsAsync(Plants, SearchText);

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
        if (_allPlantsBackup.Count == 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(SearchText.Trim()))
        {
            Plants.Clear();

            foreach (var item in _allPlantsBackup)
            {
                Plants.Add(item);
            }
        }
    }

    private static Task<List<PlantListItemViewModel>> FilterPlantsAsync(IEnumerable<PlantListItemViewModel> plants, string searchText)
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

    async void IRecipient<PlantAddedOrChangedMessage>.Receive(PlantAddedOrChangedMessage message)
    {
        await LoadAllPlantsFromDatabase();
        await SetAllPlants();
    }

    void IRecipient<PlantDeletedMessage>.Receive(PlantDeletedMessage message)
    {
        PlantListItemViewModel? deletedPlant = Plants.FirstOrDefault(e => e.Id == message.PlantId);
        if (deletedPlant != null)
        {
            Plants.Remove(deletedPlant);
        }
    }

    #region Deal with watering notification

    async void IRecipient<IsWateringNotifyEnabledMessage>.Receive(IsWateringNotifyEnabledMessage message)
    {
        switch (message.IsWateringNotificationEnabled)
        {
            case true:
                {
                    _notificationService.CancelAll();

                    await ScheduleNotifications(ReminderType.Watering);
                    await ScheduleNotifications(ReminderType.Fertilization);
                }
                break;

            case false:
                {
                    _notificationService.CancelAll();
                }
                break;

            default:
        }
    }

    private async Task ScheduleNotifications(ReminderType reminderType)
    {
        if (!_notificationService.IsSupported)
            return;

        for (int i = 0; i < Plants.Count; i++)
        {
            PlantListItemViewModel plant = Plants[i];
            await ScheduleNotification(reminderType, plant, i);
        }
    }

    private async Task ScheduleNotification(ReminderType reminderType, PlantListItemViewModel plant, int plantIndex)
    {
        try
        {
            string title = $"Remember to Water Your Plant: {plant.Name}";

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
            if (scheduledTime == null)
                return;

            if (await _settingsService.GetDebugSettingAsync())
            {
                scheduledTime = DateTime.Now.AddSeconds((plantIndex + 1) * 5);
            }

            // Data to be returned by the notification
            var list = new List<string>
                {
                    typeof(NotificationPage).FullName ?? "NotificationPage",
                    plant.Id.GetHashCode().ToString(),
                    plant.Id.ToString(),
                };
            string serializeReturningData = JsonSerializer.Serialize(list);

            var notificationRequest = new NotificationRequest
            {
                NotificationId = plant.Id.GetHashCode(),
                Title = title,
                Description = $"Scheduled {reminderType.ToString()} Time: {scheduledTime}",
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
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message, "OK");
        }
    }

    //private void OnNotificationReceived(NotificationEventArgs e)
    //{
    //    if (e.Request is null)
    //    {
    //        return;
    //    }

    //    MainThread.BeginInvokeOnMainThread(() =>
    //    {
    //        // TODO
    //        //if (!CustomAlert.IsToggled)
    //        //{
    //        //    return;
    //        //}
    //        var requestJson = JsonSerializer.Serialize(e.Request, new JsonSerializerOptions
    //        {
    //            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    //        });

    //        //_dialogService.Notify(e.Request.Title, requestJson, "OK");
    //    });
    //}

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
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _dialogService.Notify(e.Request.Title, $"No Request", "OK");
                    });
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

    #endregion Deal with watering notification

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
    async void IRecipient<ReminderItemChangedMessage>.Receive(ReminderItemChangedMessage message)
    {
        if (null == message)
        {
            return;
        }

        PlantListItemViewModel? updatePlant = Plants.FirstOrDefault(x => x.Id == message.PlantId);

        Plant? plantFromDB = await _plantService.GetPlantByIdAsync(message.PlantId);
        if (null == plantFromDB)
            return;

        if (updatePlant != null)
        {
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
        }
    }
}