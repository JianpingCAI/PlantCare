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
        public CareType ReminderType { get; set; }
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

            //AdjustSpan();
            //DeviceDisplay.MainDisplayInfoChanged += OnDeviceDisplay_MainDisplayInfoChanged;
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

        //[ObservableProperty]
        private bool _isShowCalendar = true;

        [RelayCommand]
        public void HideCalendar()
        {
            IsShowCalendar = false;
        }

        /// <summary>
        /// When the page is appearing
        /// </summary>
        /// <returns></returns>
        public override async Task LoadDataWhenViewAppearingAsync()
        {
            try
            {
                await UpdateCalendarAndEventListAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
        }

        private Task<Calendar<PlantEventDay, PlantEvent>> CreateCalendarAsync(string? cultureCode)
        {
            return Task.Run(() =>
            {
                var calendar = new Calendar<PlantEventDay, PlantEvent>()
                {
                    SelectedDates = [],
                    SelectionAction = SelectionAction.Modify,
                    SelectionType = SelectionType.Single
                };

                //if (!string.IsNullOrEmpty(cultureCode))
                //{
                //    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name);
                //    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(CultureInfo.CurrentCulture.Name);
                //}
                // Dates selection changed event
                calendar.SelectedDates.CollectionChanged += SelectedDates_CollectionChanged;

                return calendar;
            });
        }

        private async Task UpdateCalendarAndEventListAsync()
        {
            //1) events in the calendar view
            List<PlantEvent> allPlantEvents = await GetPlantEventsAsync();

            if (ReminderCalendar is null)
            {
                Calendar<PlantEventDay, PlantEvent> calendar = await CreateCalendarAsync(null);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ReminderCalendar = calendar;
                });
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ReminderCalendar!.Events.ReplaceRange(allPlantEvents);

                TickedPlantEvents.Clear();
            });

            await UpdatePlantEventsOnSelectedCalendarDates();
        }

        private async Task UpdatePlantEventsOnSelectedCalendarDates()
        {
            if (ReminderCalendar == null)
            {
                return;
            }

            if (ReminderCalendar.SelectedDates.Count > 0)
            {
                List<PlantEvent> plantEventsOnSelectedDates
                    = [.. ReminderCalendar!.Events
                     .Where(pEvt => ReminderCalendar.SelectedDates.Any(selectedDate => selectedDate == pEvt.StartDate))
                     .OrderBy(pEvt => pEvt.ScheduledTime)];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PlantEvents.ReplaceRange(plantEventsOnSelectedDates);
                });
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PlantEvents.ReplaceRange(ReminderCalendar.Events);
                });
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
                                ReminderType = CareType.Watering,
                                Name = plant.Name,
                                PhotoPath = plant.PhotoPath,
                                StartDate = expectedWaterTime.Date,
                                EndDate = expectedWaterTime.Date.AddDays(1),
                                Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(expectedWaterTime)),

                                ScheduledTime = expectedWaterTime,
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
                            ReminderType = CareType.Watering,
                            Name = plant.Name,
                            PhotoPath = plant.PhotoPath,
                            StartDate = expectedWaterTime.Date,
                            EndDate = expectedWaterTime.Date.AddDays(1),
                            Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(expectedWaterTime)),

                            ScheduledTime = expectedWaterTime,
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
                                ReminderType = CareType.Fertilization,
                                Name = plant.Name,
                                PhotoPath = plant.PhotoPath,
                                StartDate = fertilizationTime.Date,
                                EndDate = fertilizationTime.Date.AddDays(1),
                                Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(fertilizationTime)),

                                ScheduledTime = fertilizationTime,
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
                            ReminderType = CareType.Fertilization,
                            Name = plant.Name,
                            PhotoPath = plant.PhotoPath,
                            StartDate = fertilizationTime.Date,
                            EndDate = fertilizationTime.Date.AddDays(1),
                            Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(fertilizationTime)),

                            ScheduledTime = fertilizationTime,
                        });
                    }
                }

                plantEvents.Sort((p1, p2) => p1.ScheduledTime.CompareTo(p2.ScheduledTime));

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
        public async Task TickedPlantEventsChanged(object args)
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
                            case CareType.Watering:
                                {
                                    await _plantService.UpdateLastWateringTime(plantEvent.PlantId, updatedTime);
                                }
                                break;

                            case CareType.Fertilization:
                                {
                                    await _plantService.UpdateLastFertilizationTime(plantEvent.PlantId, updatedTime);
                                }
                                break;

                            default:
                                break;
                        }

                        toRemovedSelections.Add(item);

                        WeakReferenceMessenger.Default.Send<PlantStateChangedMessage>(new PlantStateChangedMessage
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

                await UpdatePlantEventsOnSelectedCalendarDates();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
        }

        [RelayCommand]
        public async Task NavigateCalendar(int amount)
        {
            try
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
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
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

        async void IRecipient<LanguageChangedMessage>.Receive(LanguageChangedMessage message)
        {
            if (string.IsNullOrEmpty(message?.CultureCode) || ReminderCalendar is null)
                return;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    ////Set DefaultThreadCurrentCulture because CurrentCulture gets automatically reset when changed.
                    ////CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture; // new CultureInfo(TargetCultureCode);
                    ////CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;//new CultureInfo(TargetCultureCode);

                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(message.CultureCode);
                    CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(message.CultureCode);

                    //CultureInfo.CurrentCulture = new CultureInfo(message.CultureCode);
                    //CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

                    //This causes the binding converters (which use the current culture) to update.
                    //Day Names
                    var oldDayNamesOlder = ReminderCalendar.DayNamesOrder.ToList();
                    ReminderCalendar.DayNamesOrder.ReplaceRange([DayOfWeek.Monday]);
                    ReminderCalendar.DayNamesOrder.ReplaceRange(oldDayNamesOlder);

                    //NavigationView Title
                    await NavigateCalendar(1);
                    await NavigateCalendar(-1);
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
                });
            }
        }

        #region Layout related

        [ObservableProperty]
        private string _orientationState;

        public double Width { get; set; }

        public double Height { get; set; }

        public bool IsShowCalendar
        {
            get => _isShowCalendar;
            set
            {
                SetProperty(ref _isShowCalendar, value);
                AdjustPlantsGridSpan();
            }
        }

        public void UpdateOrientation()
        {
            if (Width > 0 && Height > 0)
            {
                OrientationState = Width > Height ? "Landscape" : "Portrait";
                AdjustPlantsGridSpan();
            }
        }

        [ObservableProperty]
        private int _photoWidth = 110;

        [ObservableProperty]
        private int _photoHeight = 120;

        [ObservableProperty]
        private int _photoSpan = 3;

        private void AdjustPlantsGridSpan()
        {
            //DisplayInfo mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            //double width = mainDisplayInfo.Width / mainDisplayInfo.Density;

            //PhotoSpan = ((int)width - 10) / PhotoWidth;

            // Landscape
            if (Width > Height)
            {
                //int displayWidth = IsShowCalendar ? ((int)Width / 2 - 10) : ((int)Width - 10);
                //PhotoSpan = (displayWidth - 10) / PhotoWidth;
                PhotoSpan = ((int)Width - 10) / PhotoWidth;
            }
            // Portrait
            else
            {
                PhotoSpan = ((int)Width - 10) / PhotoWidth;
            }
        }

        //private void OnDeviceDisplay_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        //{
        //    AdjustSpan();
        //}

        #endregion Layout related

        //void IDisposable.Dispose()
        //{
        //    //LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
        //    if (ReminderCalendar is not null)
        //    {
        //        ReminderCalendar.SelectedDates.CollectionChanged -= SelectedDates_CollectionChanged;
        //    }
        //    DeviceDisplay.MainDisplayInfoChanged -= OnDeviceDisplay_MainDisplayInfoChanged;
        //}
    }
}