using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using PlantCare.Data.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using XCalendar.Core.Collections;
using XCalendar.Core.Enums;
using XCalendar.Core.Extensions;
using XCalendar.Core.Models;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace PlantCare.App.ViewModels
{
    public partial class PlantCalendarViewModel : ViewModelBase
    {
        private readonly IPlantService _plantService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private Calendar<PlantEventDay, PlantEvent>? _reminderCalendar = null;

        public PlantCalendarViewModel(IPlantService plantService, IDialogService dialogService)
        {
            _plantService = plantService;
            _dialogService = dialogService;
        }

        public bool IsShowUnattendedOnly { get; set; } = true;

        // Displayed events
        public ObservableRangeCollection<PlantEvent> PlantEvents { get; } = [];

        [ObservableProperty]
        private ObservableCollection<object> _tickedPlantEvents = [];

        [ObservableProperty]
        private bool _isSetRemindersDoneEnabled = false;

        [ObservableProperty]
        private string _selectAllButtonText = Consts.SelectAll;

        [ObservableProperty]
        private bool _isShowCalendar = true;

        [RelayCommand]
        public void HideCalendar()
        {
            IsShowCalendar = false;
        }

        private Task _loadCalendarTask = Task.CompletedTask;

        /// <summary>
        /// When the page is appearing
        /// </summary>
        /// <returns></returns>
        public override async Task LoadDataWhenViewAppearingAsync()
        {
            _loadCalendarTask = Task.Run(() =>
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

            await UpdateCalendarAndEventList();
        }

        private async Task UpdateCalendarAndEventList()
        {
            //1) events in the calendar view
            List<PlantEvent> allPlantEvents = await GetPlantEventsAsync();

            if (ReminderCalendar is null)
            {
                await _loadCalendarTask;
            }
            ReminderCalendar!.Events.ReplaceRange(allPlantEvents);

            UpdatePlantEventsOnSelectedCalendarDates();
        }

        private void UpdatePlantEventsOnSelectedCalendarDates()
        {
            if (ReminderCalendar == null)
            {
                return;
            }

            if (ReminderCalendar.SelectedDates.Count > 0)
            {
                List<PlantEvent> plantEventsOnSelectedDates
                    = ReminderCalendar!.Events
                     .Where(pEvt => ReminderCalendar.SelectedDates.Any(selectedDate => selectedDate.Date == pEvt.StartDate.AddDays(1).Date))
                     .OrderByDescending(pEvt => pEvt.StartDate).ToList();

                PlantEvents.ReplaceRange(plantEventsOnSelectedDates);
            }
            else
            {
                PlantEvents.ReplaceRange(ReminderCalendar.Events);
            }
        }

        #region Load Data

        //private List<Plant> _allPlantsCache = [];

        private async Task<List<PlantEvent>> GetPlantEventsAsync()
        {
            List<Plant> _allPlantsCache = await _plantService.GetAllPlantsAsync();

            DateTime nowTime = DateTime.Now;

            ProgressToColorConverter colorConverter = new();

            List<Plant> filteredPlants = [];

            // filter by IsShowUnattendedOnly
            if (IsShowUnattendedOnly)
            {
                filteredPlants = _allPlantsCache
                    .Where(x => x.LastWatered.AddHours(x.WateringFrequencyInHours) <= nowTime
                        || x.LastFertilized.AddHours(x.FertilizeFrequencyInHours) <= nowTime)
                    .ToList();
            }
            else
            {
                filteredPlants = _allPlantsCache;
            }

            // Convert to PlantEvent
            List<PlantEvent> plantEvents = [];
            foreach (Plant plant in filteredPlants)
            {
                DateTime expectedWaterTime = plant.LastWatered.AddHours(plant.WateringFrequencyInHours);
                if (IsShowUnattendedOnly)
                {
                    if (expectedWaterTime <= nowTime)
                    {
                        plantEvents.Add(new PlantEvent
                        {
                            //Title = plant.Name,
                            //Description = plant.PhotoPath,
                            PlantId = plant.Id,
                            ReminderType = ReminderType.Watering,
                            Name = plant.Name,
                            PhotoPath = plant.PhotoPath,
                            StartDate = expectedWaterTime.AddDays(-1), // a trick needed here to use XCalendar
                            EndDate = expectedWaterTime,
                            Color = colorConverter.Convert(PlantState.GetCurrentStateValue(expectedWaterTime), null, null, null) as Color ?? Colors.Red,

                            ScheduledTime = expectedWaterTime,
                            //IsOverdue = expectedWaterTime <= DateTime.Now
                        });
                    }
                }
                else
                {
                    plantEvents.Add(new PlantEvent
                    {
                        //Title = plant.Name,
                        //Description = plant.PhotoPath,
                        PlantId = plant.Id,
                        ReminderType = ReminderType.Watering,
                        Name = plant.Name,
                        PhotoPath = plant.PhotoPath,
                        StartDate = expectedWaterTime.AddDays(-1), // a trick needed here to use XCalendar
                        EndDate = expectedWaterTime,
                        Color = colorConverter.Convert(PlantState.GetCurrentStateValue(expectedWaterTime), null, null, null) as Color ?? Colors.Red,

                        ScheduledTime = expectedWaterTime,
                        //IsOverdue = expectedWaterTime <= DateTime.Now
                    });
                }

                DateTime fertilizationTime = plant.LastFertilized.AddHours(plant.FertilizeFrequencyInHours);
                if (IsShowUnattendedOnly)
                {
                    if (fertilizationTime <= nowTime)
                    {
                        plantEvents.Add(new PlantEvent
                        {
                            //Title = plant.Name,
                            //Description = plant.PhotoPath,
                            PlantId = plant.Id,
                            ReminderType = ReminderType.Fertilization,
                            Name = plant.Name,
                            PhotoPath = plant.PhotoPath,
                            StartDate = fertilizationTime.AddDays(-1),
                            EndDate = fertilizationTime,
                            Color = colorConverter.Convert(PlantState.GetCurrentStateValue(fertilizationTime), null, null, null) as Color ?? Colors.Red,

                            ScheduledTime = fertilizationTime,
                            //IsOverdue = fertilizationTime <= DateTime.Now
                        });
                    }
                }
                else
                {
                    plantEvents.Add(new PlantEvent
                    {
                        //Title = plant.Name,
                        //Description = plant.PhotoPath,
                        PlantId = plant.Id,
                        ReminderType = ReminderType.Fertilization,
                        Name = plant.Name,
                        PhotoPath = plant.PhotoPath,
                        StartDate = fertilizationTime.AddDays(-1),
                        EndDate = fertilizationTime,
                        Color = colorConverter.Convert(PlantState.GetCurrentStateValue(fertilizationTime), null, null, null) as Color ?? Colors.Red,

                        ScheduledTime = fertilizationTime,
                        //IsOverdue = fertilizationTime <= DateTime.Now
                    });
                }
            }

            return plantEvents;
        }

        #endregion Load Data

        /// <summary>
        /// Checkbox: whether only to show unattended plants
        /// </summary>
        /// <param name="isChecked"></param>
        /// <returns></returns>
        [RelayCommand]
        public async Task SelectUnattendedPlants(object isChecked)
        {
            if (IsBusy) return;

            try
            {
                bool state = (bool)isChecked;

                if (IsShowUnattendedOnly != state)
                {
                    IsShowUnattendedOnly = state;

                    await UpdateCalendarAndEventList();
                }
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

        /// <summary>
        /// Selection for checking done
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [RelayCommand]
        public async Task TickedPlantEventsChanged(/*SelectionChangedEventArgs args*/object args)
        {
            try
            {
                //await Task.Run(() =>
                //{
                SelectAllButtonText = TickedPlantEvents.Count > 0 ? Consts.Unselect : Consts.SelectAll;
                IsSetRemindersDoneEnabled = TickedPlantEvents.Count > 0;

                if (null == TickedPlantEvents)
                    return;

                foreach (PlantEvent item in PlantEvents)
                {
                    item.IsSelected = false;
                }

                foreach (object item in TickedPlantEvents)
                {
                    if (item is PlantEvent plantEvent)
                    {
                        plantEvent.IsSelected = true;
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

        #region Mark done

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

                bool isConfirmed = await _dialogService.Ask("Confirm", $"Mark as done and update the last attended time as now {updateTime.ToShortTimeString()}?");
                if (!isConfirmed)
                    return;

                foreach (object item in TickedPlantEvents)
                {
                    if (item is PlantEvent plantEvent)
                    {
                        switch (plantEvent.ReminderType)
                        {
                            case ReminderType.Watering:
                                {
                                    await _plantService.UpdateLastWateringTime(plantEvent.PlantId, updateTime);
                                }
                                break;

                            case ReminderType.Fertilization:
                                {
                                    await _plantService.UpdateLastFertilizationTime(plantEvent.PlantId, updateTime);
                                }
                                break;

                            default:
                                break;
                        }

                        toRemovedSelections.Add(item);
                        PlantEvents.Remove(plantEvent);

                        WeakReferenceMessenger.Default.Send<ReminderItemChangedMessage>(new ReminderItemChangedMessage
                        {
                            PlantId = plantEvent.PlantId,
                            UpdatedTime = updateTime,
                            ReminderType = plantEvent.ReminderType
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
                    foreach (object item in toRemovedSelections)
                    {
                        if (item is PlantEvent plantEvent)
                        {
                            ReminderCalendar!.Events.Remove(plantEvent);

                            PlantEvents.Remove(plantEvent);
                        }
                    }
                }
                IsBusy = false;
            }
        }

        #endregion Mark done

        #region Calendar Methods

        /// <summary>
        /// Update SelectedEvents when dates selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SelectedDates_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (sender is null)
                {
                    return;
                }

                UpdatePlantEventsOnSelectedCalendarDates();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify("Error", ex.Message);
            }
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

        #endregion Calendar Methods
    }
}