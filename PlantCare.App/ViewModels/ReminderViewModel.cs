namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;
using System.Collections.ObjectModel;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data.DbModels;

public partial class ReminderViewModel : ViewModelBase
{
    private readonly IReminderService _reminderService;

    [ObservableProperty]
    private ObservableCollection<Reminder> reminders;

    public ReminderViewModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
        LoadReminders();
    }

    [RelayCommand]
    private async void LoadReminders()
    {
        if (IsBusy) return;

        try
        {
            var reminderList = await _reminderService.GetAllRemindersAsync();
            Reminders = new ObservableCollection<Reminder>(reminderList);
        }
        finally { IsBusy = false; }
    }
}