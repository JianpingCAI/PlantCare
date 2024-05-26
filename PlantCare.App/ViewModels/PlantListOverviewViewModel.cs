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

namespace PlantCare.App.ViewModels;

public partial class PlantListOverviewViewModel : ViewModelBase, IRecipient<PlantAddedOrChangedMessage>, IRecipient<PlantDeletedMessage>
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;

    public PlantListOverviewViewModel(IPlantService plantService, INavigationService navigationService, INotificationService notificationService, IDialogService dialogService)
    {
        _plantService = plantService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _dialogService = dialogService;

        //_notificationService.NotificationReceiving = OnNotificationReceiving;
        _notificationService.NotificationReceived += ShowCustomAlertFromNotification;
        _notificationService.NotificationActionTapped += Current_NotificationActionTapped;

        WeakReferenceMessenger.Default.Register<PlantAddedOrChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantDeletedMessage>(this);
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
            await LoadingDataWhenViewAppearing(GetPlants);
        }
    }

    private async Task GetPlants()
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
        //    viewModels.Add(MapToViewModel(new Plant
        //    {
        //        Name = "Plant2",
        //        Species = "species",
        //        PhotoPath = "https://picsum.photos/200/300"
        //    }));
        //}
    }

    [RelayCommand]
    private void AddPlant()
    {
        if (IsBusy)
            return;

        try
        {
            _navigationService.GoToAddPlant();
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

    [RelayCommand]
    public async void SetWateringReminder()
    {
        for (int i = 0; i < Plants.Count; i++)
        {
            PlantListItemViewModel viewModel = Plants[i];
            //_notificationService.ScheduleNotification(i, viewModel.MapToModel());

            await ScheduleNotification("my test", 10);
        }
    }

    [RelayCommand]
    public void CancelWateringReminder()
    {
        //_notificationService.CancelNotification(1);
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

        await GetPlants();
    }

    void IRecipient<PlantDeletedMessage>.Receive(PlantDeletedMessage message)
    {
        PlantListItemViewModel? deletedPlant = Plants.FirstOrDefault(e => e.Id == message.PlantId);
        if (deletedPlant != null)
        {
            Plants.Remove(deletedPlant);
        }
    }

    private int _tapCount = 0;

    private async Task ScheduleNotification(string title, double seconds)
    {
        _tapCount++;
        var notificationId = (int)DateTime.Now.Ticks;
        var list = new List<string>
            {
                typeof(NotificationPage).FullName ?? "NotificationPage",
                notificationId.ToString(),
                title,
                _tapCount.ToString()
            };
        var serializeReturningData = JsonSerializer.Serialize(list);

        var notification = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = title,
            Description = $"Tap Count: {_tapCount}",
            ReturningData = serializeReturningData,
            Group = AndroidOptions.DefaultGroupId,
            Schedule =
                {
                    NotifyTime = DateTime.Now.AddSeconds(seconds),
                    //RepeatType = NotificationRepeat.TimeInterval,
                    //NotifyRepeatInterval = TimeSpan.FromSeconds(10),
                }
        };

        if (await _notificationService.AreNotificationsEnabled() == false)
        {
            await _notificationService.RequestNotificationPermission();
        }
        await _notificationService.Show(notification);
    }

    private void ShowCustomAlertFromNotification(NotificationEventArgs e)
    {
        if (e.Request is null)
        {
            return;
        }

        System.Diagnostics.Debug.WriteLine(e);

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

            _dialogService.Notify(e.Request.Title, requestJson, "OK");
        });
    }

    private async void Current_NotificationActionTapped(NotificationActionEventArgs e)
    {
        if (IsBusy)
        {
            return;
        }
        try
        {
            var log = new StringBuilder();
            log.AppendLine($"{Environment.NewLine}ActionId {e.ActionId} {DateTime.Now}");

            if (e.IsDismissed)
            {
                log.AppendLine($"{Environment.NewLine}Dismissed {DateTime.Now}");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _dialogService.Notify(e.Request.Title, "User Dismissed Notification", "OK");
                });
                return;
            }

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
                if (list is null || list.Count != 4)
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

                var id = list[1];
                var message = list[2];
                var tapCount = list[3];

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await ((NavigationPage)App.Current.MainPage).Navigation.PushModalAsync(
                    new NotificationPage(_notificationService,
                    int.Parse(id),
                    message,
                    int.Parse(tapCount)));
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
}