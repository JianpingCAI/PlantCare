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
using System.Security.Principal;
using System.Text.Json;
using Plugin.LocalNotification.EventArgs;
using System.Text.Json.Serialization;
using System.Text;
using System.Diagnostics;

namespace PlantCare.App.ViewModels;

public partial class PlantListOverviewViewModel : ViewModelBase,
    IRecipient<PlantAddedOrChangedMessage>,
    IRecipient<PlantDeletedMessage>,
    IRecipient<WateringNotificationChangedMessage>
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

        if (_notificationService.IsSupported)
        {
            WeakReferenceMessenger.Default.Register<WateringNotificationChangedMessage>(this);

            //_notificationService.NotificationReceiving = OnNotificationReceiving;
            _notificationService.NotificationReceived += ShowCustomAlertFromNotification;
            _notificationService.NotificationActionTapped += CurrentNotificationActionTapped;
        }
    }

    [ObservableProperty]
    private ObservableCollection<PlantListItemViewModel> plants = [];

    [ObservableProperty]
    private string searchText = string.Empty;

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

    // Override
    public override async Task LoadDataWhenViewAppearingAsync()
    {
        // Load only once
        if (Plants.Count == 0)
        {
            await LoadingDataWhenViewAppearing(LoadAllPlants);
        }
    }

    private async Task LoadAllPlants()
    {
        try
        {
            List<Plant> plants = await _plantService.GetAllPlantsAsync();

            List<PlantListItemViewModel> viewModels = [];
            foreach (Plant plant in plants)
            {
                viewModels.Add(MapToViewModel(plant));
            }
            Plants.Clear();
            Plants = viewModels.ToObservableCollection();

            //if (plants.Count == 0)
            //{
            //    viewModels.Add(MapToViewModel(new Plant
            //    {
            //        Name = "Plant1",
            //        Species = "species",
            //        PhotoPath = "https://picsum.photos/200/300"
            //    }));
            //}

            if (await _settingsService.GetWateringNotificationSettingAsync())
            {
                await ScheduleWateringNotifications();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private async Task AddPlant()
    {
        if (IsBusy)
            return;

        try
        {
            await _navigationService.GoToAddPlant();
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

    [RelayCommand]
    private void Search()
    {
        if (IsBusy)
            return;

        try
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                //var searchResults = await _plantService.SearchPlantsAsync(SearchText);
                //Plants.Clear();
                //foreach (var plant in searchResults)
                //{
                //    Plants.Add(plant);
                //}
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public static PlantListItemViewModel MapToViewModel(Plant plant)
    {
        return new PlantListItemViewModel
        {
            Id = plant.Id,
            Species = plant.Species,
            Name = plant.Name,
            Age = plant.Age,
            PhotoPath = plant.PhotoPath,

            LastWatered = plant.LastWatered,
            WateringFrequencyInHours = plant.WateringFrequencyInHours,
        };
    }

    async void IRecipient<PlantAddedOrChangedMessage>.Receive(PlantAddedOrChangedMessage message)
    {
        Plants.Clear();

        await LoadAllPlants();
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

    async void IRecipient<WateringNotificationChangedMessage>.Receive(WateringNotificationChangedMessage message)
    {
        switch (message.IsWateringNotificationEnabled)
        {
            case true:
                {
                    _notificationService.CancelAll();

                    await ScheduleWateringNotifications();
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

    private async Task ScheduleWateringNotifications()
    {
        if (!_notificationService.IsSupported)
            return;

        for (int i = 0; i < Plants.Count; i++)
        {
            PlantListItemViewModel plant = Plants[i];
            await ScheduleWateringNotification(plant, i, 10);
        }
    }

    private async Task ScheduleWateringNotification(PlantListItemViewModel plant, int plantIndex, double seconds)
    {
        try
        {
            string title = $"Remember to Water Your Plant: {plant.Name}";

            DateTime waterTime = plant.NextWateringTime;

            if (await _settingsService.GetDebugSettingAsync())
            {
                waterTime = DateTime.Now.AddSeconds((plantIndex + 1) * 5);
            }

            int notificationId = plant.Id.GetHashCode();

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
                Description = $"Planed Watering Time: {plant.NextWateringTime}",
                ReturningData = serializeReturningData,
                Group = AndroidOptions.DefaultGroupId,
                Schedule =
                {
                    NotifyTime = waterTime,
                    //RepeatType = NotificationRepeat.TimeInterval,
                    //NotifyRepeatInterval = TimeSpan.FromSeconds(10),
                }
            };

            if (await _notificationService.AreNotificationsEnabled() == false)
            {
                await _notificationService.RequestNotificationPermission();
            }
            await _notificationService.Show(notificationRequest);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    private void ShowCustomAlertFromNotification(NotificationEventArgs e)
    {
        if (e.Request is null)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // TODO
            //if (!CustomAlert.IsToggled)
            //{
            //    return;
            //}
            var requestJson = JsonSerializer.Serialize(e.Request, new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            });

            //_dialogService.Notify(e.Request.Title, requestJson, "OK");
        });
    }

    private void CurrentNotificationActionTapped(NotificationActionEventArgs e)
    {
        if (IsBusy)
        {
            return;
        }
        try
        {
            var log = new StringBuilder();
            log.AppendLine($"{Environment.NewLine}ActionId {e.ActionId} {DateTime.Now}");

            // Notification is cancelled
            if (e.IsDismissed)
            {
                log.AppendLine($"{Environment.NewLine}Dismissed {DateTime.Now}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _dialogService.Notify(e.Request.Title, "User Dismissed Notification", "OK");
                });
                return;
            }

            // Notification is tapped
            if (e.IsTapped)
            {
                log.AppendLine($"{Environment.NewLine}Tapped {DateTime.Now}");
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
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _dialogService.Notify(e.Request.Title, $"No ReturningData {e.Request.ReturningData}", "OK");
                    });
                    return;
                }

                if (list[0] != typeof(NotificationPage).FullName)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _dialogService.Notify(e.Request.Title, $"Not NotificationPage", "OK");
                    });
                    return;
                }

                var notificationId = list[1];
                var plantId = list[2];

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await _navigationService.GoToPlantDetail(Guid.Parse(plantId));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });
                return;
            }

            switch (e.ActionId)
            {
                case 100:
                    log.AppendLine($"{Environment.NewLine}Hello {DateTime.Now}");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _dialogService.Notify(e.Request.Title, "Hello Button was Tapped", "OK");
                    });

                    _notificationService.Cancel(e.Request.NotificationId);
                    break;

                case 101:
                    log.AppendLine($"{Environment.NewLine}Cancel {DateTime.Now}");
                    _notificationService.Cancel(e.Request.NotificationId);
                    break;
            }

            //await File.AppendAllTextAsync(_cacheFilePath, $"{Environment.NewLine}Cancel {DateTime.Now}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion Deal with watering notification
}