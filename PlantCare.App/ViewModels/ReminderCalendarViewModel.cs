using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data.Models;
using System.Collections.Specialized;
using XCalendar.Core.Collections;
using XCalendar.Core.Enums;
using XCalendar.Core.Extensions;
using XCalendar.Core.Interfaces;
using XCalendar.Core.Models;
using XCalendar.Maui.Models;

namespace PlantCare.App.ViewModels
{
    public class PlantEventDay<TEvent> : CalendarDay<TEvent> where TEvent : IEvent
    {
    }

    public class PlantEventDay : PlantEventDay<PlantEvent>
    {
    }

    public class PlantEvent : ColoredEvent
    {
        public Guid PlantId { get; set; }
        public ReminderType ReminderType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        //public bool IsOverdue { get; set; }

        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public DateTime ScheduledTime { get; set; } = default;
    }

    public partial class ReminderCalendarViewModel : ViewModelBase
    {
        private readonly IPlantService _plantService;

        [ObservableProperty]
        public Calendar<PlantEventDay, PlantEvent>? _reminderCalendar = null;

        public ReminderCalendarViewModel(IPlantService plantService)
        {
            _plantService = plantService;
        }

        public ObservableRangeCollection<PlantEvent> SelectedEvents { get; } = [];

        // When the page is appearing
        public override async Task LoadDataWhenViewAppearingAsync()
        {
            await Task.Run(() =>
            {
                if (ReminderCalendar is null)
                {
                    ReminderCalendar = new Calendar<PlantEventDay, PlantEvent>()
                    {
                        SelectedDates = [],
                        SelectionAction = SelectionAction.Modify,
                        SelectionType = SelectionType.Single
                    };

                    // Dates selection changed event
                    ReminderCalendar.SelectedDates.CollectionChanged += SelectedDates_CollectionChanged;
                }
            });

            List<PlantEvent> reminderEvents = await GetAllRemindersAsync();

            ReminderCalendar!.Events.ReplaceRange(reminderEvents);
        }

        private async Task<List<PlantEvent>> GetAllRemindersAsync()
        {
            List<Plant> allPlants = await _plantService.GetAllPlantsAsync();

            List<PlantEvent> reminderEvents = [];
            foreach (Plant plant in allPlants)
            {
                DateTime waterTime = plant.LastWatered.AddHours(plant.WateringFrequencyInHours);
                reminderEvents.Add(new PlantEvent
                {
                    //Title = plant.Name,
                    //Description = plant.PhotoPath,
                    ReminderType = ReminderType.Watering,
                    Name = plant.Name,
                    PhotoPath = plant.PhotoPath,
                    StartDate = waterTime.AddDays(-1), // a trick here
                    EndDate = waterTime,
                    Color = Colors.DeepSkyBlue,

                    //IsOverdue = waterTime <= DateTime.Now
                });

                DateTime fertTime = plant.LastFertilized.AddHours(plant.FertilizeFrequencyInHours);
                reminderEvents.Add(new PlantEvent
                {
                    //Title = plant.Name,
                    //Description = plant.PhotoPath,
                    ReminderType = ReminderType.Fertilization,
                    Name = plant.Name,
                    PhotoPath = plant.PhotoPath,
                    StartDate = fertTime.AddDays(-1),
                    EndDate = fertTime,
                    Color = Colors.OrangeRed,

                    //IsOverdue = fertTime <= DateTime.Now
                });
            }

            return reminderEvents;
        }

        /// <summary>
        /// Update SelectedEvents when dates selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedDates_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is null)
            {
                return;
            }

            List<PlantEvent> remindersOnSelectedDates = ReminderCalendar.Events
                .Where(colorEvt => ReminderCalendar.SelectedDates.Any(selectedDate => selectedDate.Date.Date == colorEvt.StartDate.AddDays(1).Date))
                .OrderByDescending(colorEvt => colorEvt.StartDate).ToList();

            SelectedEvents.ReplaceRange(remindersOnSelectedDates);
        }

        [RelayCommand]
        public void NavigateCalendar(int amount)
        {
            if (ReminderCalendar.NavigatedDate.TryAddMonths(amount, out DateTime targetDate))
            {
                ReminderCalendar.Navigate(targetDate - ReminderCalendar.NavigatedDate);
            }
            else
            {
                ReminderCalendar.Navigate(amount > 0 ? TimeSpan.MaxValue : TimeSpan.MinValue);
            }
        }

        [RelayCommand]
        public void ChangeDateSelection(DateTime dateTime)
        {
            ReminderCalendar?.ChangeDateSelection(dateTime);
        }
    }
}