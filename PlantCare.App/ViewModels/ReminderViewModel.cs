using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;
using System.Collections.ObjectModel;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data.DbModels;

namespace PlantCare.App.ViewModels;

public partial class ReminderViewModel : ViewModelBase
{
    private readonly IReminderService _reminderService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<Reminder> reminders;

    public ReminderViewModel(IReminderService reminderService, IDialogService dialogService)
    {
        _reminderService = reminderService;
        _dialogService = dialogService;
    }

    public override async Task LoadDataWhenViewAppearingAsync()
    {
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
        List<Reminder> notifications = await _reminderService.GetAllRemindersAsync();
        Reminders = new ObservableCollection<Reminder>(notifications);
    }
}