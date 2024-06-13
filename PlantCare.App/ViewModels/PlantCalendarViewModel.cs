using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using PlantCare.Data.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using XCalendar.Core.Collections;
using XCalendar.Core.Enums;
using XCalendar.Core.Extensions;
using XCalendar.Core.Interfaces;
using XCalendar.Core.Models;
using XCalendar.Maui.Models;

namespace PlantCare.App.ViewModels
{
    public class PlantEvent : ColoredEvent
    {
        public Guid PlantId { get; set; }
        public ReminderType ReminderType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;

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

    public class PlantEventDay<TEvent> : CalendarDay<TEvent> where TEvent : IEvent
    {
    }

    public class PlantEventDay : PlantEventDay<PlantEvent>
    {
    }

    public partial class PlantCalendarViewModel : ViewModelBase,
        IDisposable,
        IRecipient<LanguageChangedMessage>
    {
        private readonly IPlantService _plantService;
        private readonly IDialogService _dialogService;

        public PlantCalendarViewModel(IPlantService plantService, IDialogService dialogService)
        {
            _plantService = plantService;
            _dialogService = dialogService;

            //LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
            WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this);
        }

        [ObservableProperty]
        private Calendar<PlantEventDay, PlantEvent>? _reminderCalendar = null;

        public bool IsShowUnattendedOnly { get; set; } = true;

        // Displayed events
        public ObservableRangeCollection<PlantEvent> PlantEvents { get; } = [];

        [ObservableProperty]
        private ObservableCollection<object> _tickedPlantEvents = [];

        [ObservableProperty]
        private bool _isSetRemindersDoneEnabled = false;

        [ObservableProperty]
        private string _selectAllButtonText = ConstStrings.SelectAll;

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
            try
            {
                _loadCalendarTask = Task.Run(() =>
                   {
                       ReminderCalendar ??= CreateCalendar(null);
                   });

                await UpdateCalendarAndEventListAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
        }

        private Calendar<PlantEventDay, PlantEvent> CreateCalendar(string? cultureCode)
        {
            var calendar = new Calendar<PlantEventDay, PlantEvent>()
            {
                SelectedDates = [],
                SelectionAction = SelectionAction.Modify,
                SelectionType = SelectionType.Single
            };

            if (!string.IsNullOrEmpty(cultureCode))
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(CultureInfo.CurrentCulture.Name);
            }
            // Dates selection changed event
            calendar.SelectedDates.CollectionChanged += SelectedDates_CollectionChanged;

            return calendar;
        }

        private async Task UpdateCalendarAndEventListAsync()
        {
            //1) events in the calendar view
            List<PlantEvent> allPlantEvents = await GetPlantEventsAsync();

            if (ReminderCalendar is null)
            {
                await _loadCalendarTask;
            }
            ReminderCalendar!.Events.ReplaceRange(allPlantEvents);

            UpdatePlantEventsOnSelectedCalendarDates();

            TickedPlantEvents.Clear();
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
                    = [.. ReminderCalendar!.Events
                     .Where(pEvt => ReminderCalendar.SelectedDates.Any(selectedDate => selectedDate.Date == pEvt.StartDate.AddDays(1).Date))
                     .OrderByDescending(pEvt => pEvt.StartDate)];

                PlantEvents.ReplaceRange(plantEventsOnSelectedDates);
            }
            else
            {
                PlantEvents.ReplaceRange(ReminderCalendar.Events);
            }
        }

        #region Load Data

        private Task<List<PlantEvent>> GetPlantEventsAsync()
        {
            return Task.Run(async () =>
            {
                List<Plant> _allPlantsCache = await _plantService.GetAllPlantsAsync();

                DateTime nowTime = DateTime.Now;

                List<Plant> filteredPlants;

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
                                Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(expectedWaterTime)),

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
                            Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(expectedWaterTime)),

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
                                Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(fertilizationTime)),

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
                            Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(fertilizationTime)),

                            ScheduledTime = fertilizationTime,
                            //IsOverdue = fertilizationTime <= DateTime.Now
                        });
                    }
                }

                return plantEvents;
            });
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
                IsBusy = true;
                IsLoading = true;

                bool state = (bool)isChecked;

                if (IsShowUnattendedOnly != state)
                {
                    IsShowUnattendedOnly = state;

                    await UpdateCalendarAndEventListAsync();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
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
                SelectAllButtonText = TickedPlantEvents.Count > 0 ? ConstStrings.Unselect : ConstStrings.SelectAll;
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
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
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
                DateTime updatedTime = DateTime.Now;

                bool isConfirmed = await _dialogService.Ask(
                    LocalizationManager.Instance[ConstStrings.Confirm] ?? ConstStrings.Confirm,
                    $"{LocalizationManager.Instance[ConstStrings.MarkDone]}: {updatedTime.ToShortTimeString()}?",
                    LocalizationManager.Instance[ConstStrings.Yes] ?? ConstStrings.Yes,
                    LocalizationManager.Instance[ConstStrings.No] ?? ConstStrings.No);
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
                                    await _plantService.UpdateLastWateringTime(plantEvent.PlantId, updatedTime);
                                }
                                break;

                            case ReminderType.Fertilization:
                                {
                                    await _plantService.UpdateLastFertilizationTime(plantEvent.PlantId, updatedTime);
                                }
                                break;

                            default:
                                break;
                        }

                        toRemovedSelections.Add(item);

                        WeakReferenceMessenger.Default.Send<PlantEventStatusChangedMessage>(new PlantEventStatusChangedMessage
                        (
                             plantEvent.PlantId,
                             plantEvent.ReminderType,
                             updatedTime
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
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

                        TickedPlantEvents.Remove(item);
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
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
        }

        [RelayCommand]
        public void NavigateCalendar(int amount)
        {
            if (ReminderCalendar is null)
            {
                return;
            }

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

        #region Refresh plant events

        [ObservableProperty]
        private bool _isPlantEventRefreshing = false;

        [RelayCommand]
        public async Task RefreshPlantEvents()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                await UpdateCalendarAndEventListAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
            finally
            {
                IsPlantEventRefreshing = false;
                IsBusy = false;
            }
        }

        #endregion Refresh plant events

        void IDisposable.Dispose()
        {
            //LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
            if (ReminderCalendar is not null)
            {
                ReminderCalendar.SelectedDates.CollectionChanged -= SelectedDates_CollectionChanged;
            }
        }

        async void IRecipient<LanguageChangedMessage>.Receive(LanguageChangedMessage message)
        {
            if (string.IsNullOrEmpty(message?.CultureCode) || ReminderCalendar is null)
                return;

            try
            {
                //Set DefaultThreadCurrentCulture because CurrentCulture gets automatically reset when changed.
                //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture; // new CultureInfo(TargetCultureCode);
                //CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;//new CultureInfo(TargetCultureCode);

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(message.CultureCode);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(message.CultureCode);

                //This causes the binding converters (which use the current culture) to update.
                //Day Names
                var oldDayNamesOlder = ReminderCalendar.DayNamesOrder.ToList();
                ReminderCalendar.DayNamesOrder.ReplaceRange(new List<DayOfWeek>() { DayOfWeek.Monday });
                ReminderCalendar.DayNamesOrder.ReplaceRange(oldDayNamesOlder);

                //NavigationView Title
                NavigateCalendar(1);
                NavigateCalendar(-1);
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }

            //if (ReminderCalendar is null)
            //{
            //    ReminderCalendar = CreateCalendar(message.CultureCode);
            //}
            //else
            //{
            //    ReminderCalendar.SelectedDates.CollectionChanged -= SelectedDates_CollectionChanged;

            //    ReminderCalendar = CreateCalendar(message.CultureCode);
            //}
        }

        //private async void OnLanguageChanged(object? sender, EventArgs e)
        //{
        //    if (ReminderCalendar is null) return;
        //    try
        //    {
        //        //Set DefaultThreadCurrentCulture because CurrentCulture gets automatically reset when changed.
        //        //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture; // new CultureInfo(TargetCultureCode);
        //        //CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;//new CultureInfo(TargetCultureCode);

        //        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name);
        //        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(CultureInfo.CurrentCulture.Name);

        //        //This causes the binding converters (which use the current culture) to update.
        //        //Day Names
        //        var oldDayNamesOlder = ReminderCalendar.DayNamesOrder.ToList();
        //        ReminderCalendar.DayNamesOrder.ReplaceRange(new List<DayOfWeek>() { DayOfWeek.Monday });
        //        ReminderCalendar.DayNamesOrder.ReplaceRange(oldDayNamesOlder);

        //        //NavigationView Title
        //        NavigateCalendar(1);
        //        NavigateCalendar(-1);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _dialogService.Notify(LocalizationManager.Instance[Consts.Error] ?? Consts.Error, ex.Message);
        //    }
        //}
    }
}