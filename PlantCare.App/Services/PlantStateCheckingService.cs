using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlantCare.App.Messaging;
using PlantCare.Data.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.EventArgs;
using System.Text.Json;

//using BCrypt.Net;

namespace PlantCare.App.Services;

public class PlantStateCheckingService : BackgroundService, IRecipient<IsWateringNotifyEnabledMessage>
{
    private readonly ILogger<PlantStateCheckingService> _logger;
    private readonly IPlantService _plantService;
    private readonly ISettingsService _settingsService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    public PlantStateCheckingService(ILogger<PlantStateCheckingService> logger, IPlantService plantService, ISettingsService settingsService, INotificationService notificationService, IDialogService dialogService, INavigationService navigationService)
    {
        _logger = logger;
        _plantService = plantService;
        _settingsService = settingsService;
        _notificationService = notificationService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        if (_notificationService.IsSupported)
        {
            WeakReferenceMessenger.Default.Register<IsWateringNotifyEnabledMessage>(this);

            //_notificationService.NotificationReceiving = OnNotificationReceiving;
            //_notificationService.NotificationReceived += OnNotificationReceived;
            _notificationService.NotificationActionTapped += OnNotificationActionTapped;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WateringCheckService running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking watering state.");

            List<Plant> plants = await _plantService.GetAllPlantsAsync();

            for (int i = 0; i < plants.Count; i++)
            {
                Plant plant = plants[i];
                //var nextWateringTime = plant.LastWatered.AddDays(plant.WateringFrequencyInDays);
                //var timeLeft = nextWateringTime - DateTime.Now;

                //if (timeLeft <= TimeSpan.Zero)
                //{
                //    // Notify user to water the plant
                //    _logger.LogInformation($"Time to water the plant {plant.Name}.");
                //    // Add notification logic here
                //}

                await ScheduleWateringNotification(plant, i);
            }

            // Delay for a specific interval before checking again
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }

        _logger.LogInformation("WateringCheckService stopped.");
    }

    private async Task ScheduleWateringNotification(Plant plant, int plantIndex)
    {
        try
        {
            string title = $"Remember to Water Your Plant: {plant.Name}";

            DateTime waterTime = plant.LastWatered.AddHours(plant.WateringFrequencyInHours);

            if (await _settingsService.GetDebugSettingAsync())
            {
                waterTime = DateTime.Now.AddSeconds((plantIndex + 1) * 5);
            }

            // Data to be returned by the notification
            var list = new List<string>
            {
                plant.Id.GetHashCode().ToString(),
                plant.Id.ToString(),
            };
            string serializeReturningData = JsonSerializer.Serialize(list);

            var notificationRequest = new NotificationRequest
            {
                NotificationId = plant.Id.GetHashCode(),
                Title = title,
                Description = $"Planed Watering Time: {waterTime}",
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

            // Send a local notification to the device
            await _notificationService.Show(notificationRequest);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message, "OK");
        }
    }

    private async void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
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

                //if (list[0] != typeof(NotificationPage).FullName)
                //{
                //    await _dialogService.Notify(e.Request.Title, $"Not NotificationPage", "OK");
                //    return;
                //}

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
    }

    void IRecipient<IsWateringNotifyEnabledMessage>.Receive(IsWateringNotifyEnabledMessage message)
    {
        switch (message.IsWateringNotificationEnabled)
        {
            case true:
                {
                    //_notificationService.CancelAll();
                    //await ScheduleWateringNotifications();
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
}