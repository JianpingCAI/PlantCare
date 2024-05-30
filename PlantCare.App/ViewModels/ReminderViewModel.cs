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

    [ObservableProperty]
    private ObservableCollection<Reminder> _reminders;

    [ObservableProperty]
    private ObservableCollection<ReminderItemViewModel> _wateringReminders = [];

    [ObservableProperty]
    private ObservableCollection<object> _selectedWateringReminders = [];

    [RelayCommand]
    public async Task SelectedWateringReminderChanged(/*SelectionChangedEventArgs args*/object args)
    {
        try
        {
            await Task.Run(() =>
            {
                if (null == SelectedWateringReminders)
                    return;

                foreach (var item in WateringReminders)
                {
                    item.IsSelected = false;
                }

                foreach (var item in SelectedWateringReminders)
                {
                    if (item is ReminderItemViewModel wateringPlant)
                    {
                        wateringPlant.IsSelected = true;
                    }
                }

                SelectAllButtonText = SelectedWateringReminders.Count > 0 ? Consts.UnSelect : Consts.SelectAll;
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
                    SelectedWateringReminders.Clear();
                    foreach (var item in WateringReminders)
                    {
                        item.IsSelected = true;
                        SelectedWateringReminders.Add(item);
                    }
                    SelectAllButtonText = Consts.UnSelect;
                }
                else
                {
                    foreach (var item in SelectedWateringReminders)
                    {
                        if (item is ReminderItemViewModel wateringPlant)
                        {
                            wateringPlant.IsSelected = false;
                        }
                    }
                    SelectedWateringReminders.Clear();

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
    public void SetWatered()
    {
        Debug.WriteLine("set watered called");
    }

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        if (WateringReminders.Count != 0)
        {
            return;
        }

        try
        {
            await LoadingDataWhenViewAppearingAsync(LoadAllReminders);
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
            List<Plant> plants = await _plantService.GetPlantsToWater();
            plants = [.. plants.OrderBy(x => x.Name)];

            foreach (Plant plant in plants)
            {
                reminderItemViewModels.Add(MapToReminderItem(plant, ReminderType.Watering));
            }

            WateringReminders.Clear();

            foreach (ReminderItemViewModel reminder in reminderItemViewModels)
            {
                WateringReminders.Add(reminder);
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