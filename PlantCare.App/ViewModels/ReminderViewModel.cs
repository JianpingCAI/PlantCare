using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    partial void OnSelectedReminderTypeChanged(ReminderType selectedReminderType)
    {
        if (Reminders.Count == 0)
        {
            return;
        }

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

    //partial void OnSelectedReminderTypeChangedAsync(ReminderType selectedReminderType)
    //{
    //    if (Reminders.Count == 0)
    //    {
    //        return;
    //    }

    //    switch (selectedReminderType)
    //    {
    //        case ReminderType.Watering:
    //        case ReminderType.Fertilization:
    //            {
    //                OnViewAppearingCommand.ExecuteAsync(null);
    //            }
    //            break;

    //        default:
    //            break;
    //    }
    //}

    [RelayCommand]
    public async Task SelectedReminderChanged(/*SelectionChangedEventArgs args*/object args)
    {
        try
        {
            await Task.Run(() =>
            {
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

                SelectAllButtonText = SelectedReminders.Count > 0 ? Consts.Unselect : Consts.SelectAll;
            });
        }
        catch (Exception ex)
        {
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
    public async Task SetRemindersDone()
    {
        bool isConfirmed = await _dialogService.Ask("Confirm", "Mark the selected as done and update the last {} time as now?");
        if (!isConfirmed)
            return;
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

    private async Task LoadAllReminders()
    {
        try
        {
            List<ReminderItemViewModel> reminderItemViewModels = [];
            SelectedReminders.Clear();

            switch (_selectedReminderType)
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