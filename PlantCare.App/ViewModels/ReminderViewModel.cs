using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PlantCare.App.ViewModels;

public partial class ReminderViewModel : ViewModelBase
{
    private readonly IReminderService _reminderService;
    private readonly IDialogService _dialogService;
    private readonly IPlantService _plantService;

    public ReminderViewModel(IReminderService reminderService, IDialogService dialogService, IPlantService plantService)
    {
        _reminderService = reminderService;
        _dialogService = dialogService;
        _plantService = plantService;
    }

    //public static ReminderType[] ReminderTypes => Enum.GetValues(typeof(ReminderType)).Cast<ReminderType>().ToArray();

    public ObservableCollection<ReminderType> ReminderTypes { get; } = Enum.GetValues(typeof(ReminderType)).Cast<ReminderType>().ToObservableCollection();

    [ObservableProperty]
    private ReminderType _selectedReminderType = ReminderType.Watering;

    //[ObservableProperty]
    //private ObservableCollection<Reminder> _reminders;

    [ObservableProperty]
    private ObservableCollection<ReminderItemViewModel> _reminders = [];

    [ObservableProperty]
    private ObservableCollection<object> _selectedReminders = [];

    [ObservableProperty]
    private bool _isSetRemindersDoneEnabled = false;

    partial void OnSelectedReminderTypeChanged(ReminderType selectedReminderType)
    {
        //if (Reminders.Count == 0)
        //{
        //    return;
        //}

        switch (selectedReminderType)
        {
            case ReminderType.Watering:
            case ReminderType.Fertilization:
                {
                    IsLoading = true;
                    LoadAllReminders();
                    IsLoading = false;
                }
                break;

            default:
                break;
        }
    }

    [RelayCommand]
    public async void SelectedReminderItemChanged(/*SelectionChangedEventArgs args*/object args)
    {
        try
        {
            //await Task.Run(() =>
            //{
            SelectAllButtonText = SelectedReminders.Count > 0 ? Consts.Unselect : Consts.SelectAll;
            IsSetRemindersDoneEnabled = SelectedReminders.Count > 0;

            if (null == SelectedReminders)
                return;

            foreach (var item in Reminders)
            {
                item.IsSelected = false;
            }

            foreach (var item in SelectedReminders)
            {
                if (item is ReminderItemViewModel wateringPlant)
                {
                    wateringPlant.IsSelected = true;
                }
            }
            //});
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
            Debug.WriteLine(ex.Message);
        }
    }

    [ObservableProperty]
    private string _selectAllButtonText = Consts.SelectAll;

    [RelayCommand]
    public async Task SelectAllWateringReminders()
    {
        if (IsBusy)
        { return; }

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (SelectAllButtonText == Consts.SelectAll)
                {
                    SelectedReminders.Clear();
                    foreach (var item in Reminders)
                    {
                        item.IsSelected = true;
                        SelectedReminders.Add(item);
                    }
                    SelectAllButtonText = Consts.Unselect;
                }
                else
                {
                    foreach (var item in SelectedReminders)
                    {
                        if (item is ReminderItemViewModel wateringPlant)
                        {
                            wateringPlant.IsSelected = false;
                        }
                    }
                    SelectedReminders.Clear();

                    SelectAllButtonText = Consts.SelectAll;
                }
            });
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

    [RelayCommand]
    public async Task SetSelectedRemindersDone()
    {
        if (IsBusy)
        {
            return;
        }

        List<object> toRemovedSelections = [];

        try
        {
            DateTime updateTime = DateTime.Now;

            bool isConfirmed = await _dialogService.Ask("Confirm", $"Mark as done and update the last {SelectedReminderType} time as now {updateTime.ToShortTimeString()}?");
            if (!isConfirmed)
                return;

            foreach (object item in SelectedReminders)
            {
                if (item is ReminderItemViewModel reminder)
                {
                    switch (SelectedReminderType)
                    {
                        case ReminderType.Watering:
                            {
                                await _plantService.UpdateLastWateringTime(reminder.PlantId, updateTime);
                            }
                            break;

                        case ReminderType.Fertilization:
                            {
                                await _plantService.UpdateLastFertilizationTime(reminder.PlantId, updateTime);
                            }
                            break;

                        default:
                            break;
                    }

                    toRemovedSelections.Add(item);
                    Reminders.Remove(reminder);

                    WeakReferenceMessenger.Default.Send<ReminderItemChangedMessage>(new ReminderItemChangedMessage
                    {
                        PlantId = reminder.PlantId,
                        UpdatedTime = updateTime,
                        ReminderType = SelectedReminderType
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
        }
        finally
        {
            if (toRemovedSelections.Count > 0)
            {
                foreach (var item in toRemovedSelections)
                {
                    SelectedReminders.Remove(item);
                }
            }
            IsBusy = false;
        }
    }

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        if (Reminders.Count != 0)
        {
            return;
        }

        try
        {
            await LoadAllReminders();
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message, "OK");
        }
    }

    private Task LoadAllReminders()
    {
        return Task.Run(async () =>
        {
            try
            {
                List<ReminderItemViewModel> reminderItemViewModels = [];
                SelectedReminders.Clear();

                switch (SelectedReminderType)
                {
                    case ReminderType.Watering:
                        {
                            List<Plant> plants = await _plantService.GetPlantsToWater();
                            plants = [.. plants.OrderBy(x => x.Name)];

                            foreach (Plant plant in plants)
                            {
                                reminderItemViewModels.Add(MapToReminderItem(plant, ReminderType.Watering));
                            }
                        }
                        break;

                    case ReminderType.Fertilization:
                        {
                            List<Plant> plants = await _plantService.GetPlantsToFertilize();
                            plants = [.. plants.OrderBy(x => x.Name)];

                            foreach (Plant plant in plants)
                            {
                                reminderItemViewModels.Add(MapToReminderItem(plant, ReminderType.Fertilization));
                            }
                        }
                        break;
                }

                Reminders.Clear();

                foreach (ReminderItemViewModel reminder in reminderItemViewModels)
                {
                    Reminders.Add(reminder);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify("Error", ex.Message, "OK");
            }
        });
    }

    private static ReminderItemViewModel MapToReminderItem(Plant plant, ReminderType reminderType)
    {
        return new ReminderItemViewModel(reminderType, plant.Id)
        {
            Name = plant.Name,
            PhotoPath = plant.PhotoPath,
            ExpectedTime = plant.LastWatered.AddHours(plant.WateringFrequencyInHours)
        };
    }
}