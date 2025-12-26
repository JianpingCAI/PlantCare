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
        IRecipient<LanguageChangedMessage>, IDisposable
    {
        private readonly IPlantService _plantService;
        private readonly IDialogService _dialogService;

        public PlantCalendarViewModel(IPlantService plantService, IDialogService dialogService)
        {
            _plantService = plantService;
            _dialogService = dialogService;

            WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this);

            //AdjustSpan();
            //DeviceDisplay.MainDisplayInfoChanged += OnDeviceDisplay_MainDisplayInfoChanged;
        }

        #region Event Caching

        private List<PlantEvent>? _cachedAllEvents;
        private DateTime _cacheTimestamp;
        private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);
        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Invalidates the event cache, forcing regeneration on next load
        /// </summary>
        private void InvalidateCache()
        {
            _cachedAllEvents = null;
        }

        #endregion Event Caching

        [ObservableProperty]
        private Calendar<PlantEventDay, PlantEvent>? _reminderCalendar = null;

        private bool _isShowUnattendedOnly = true;

        public bool IsShowUnattendedOnly
        {
            get => _isShowUnattendedOnly;
            set
            {
                if (SetProperty(ref _isShowUnattendedOnly, value))
                {
                    // Filter events from cache (synchronous operation, safe on any thread)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = true);

                            // If cache is null, reload everything properly
                            if (_cachedAllEvents == null)
                            {
                                await UpdateCalendarAndEventListAsync();
                            }
                            else
                            {
                                // Filter cached events (no DB access)
                                List<PlantEvent> filteredEvents = FilterCachedEvents(_cachedAllEvents);

                                // Only update the displayed events (filtered)
                                await MainThread.InvokeOnMainThreadAsync(() =>
                                {
                                    PlantEvents.ReplaceRange(filteredEvents);
                                });
                            }
                        }
                        finally
                        {
                            await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
                        }
                    });
                }
            }
        }

        // Displayed events
        public ObservableRangeCollection<PlantEvent> PlantEvents { get; } = [];

        [ObservableProperty]
        private ObservableCollection<object> _tickedPlantEvents = [];

        [ObservableProperty]
        private bool _isSetRemindersDoneEnabled = false;

        [ObservableProperty]
        private string _selectAllButtonText = ConstStrings.SelectAll;

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
                // Load data synchronously to avoid threading issues
                // Don't use fire-and-forget Task.Run here
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
                    SelectionAction = SelectionAction.Modify, // add if not exist, or remove otherwise
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
            // Prevent concurrent calls using semaphore
            if (!await _updateLock.WaitAsync(0))
            {
                // Another update is already in progress, skip this one
                Debug.WriteLine("[Calendar] Update already in progress, skipping duplicate call");
                return;
            }

            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = true);

                // 1) Get plant events (from cache if available)
                List<PlantEvent> allPlantEvents = await GetPlantEventsAsync();

                // 2) Initialize calendar if needed (lazy loading)
                Calendar<PlantEventDay, PlantEvent>? calendar = null;
                if (ReminderCalendar is null)
                {
                    calendar = await CreateCalendarAsync(null);
                }

                // 3 & 4) Batch all UI updates into single MainThread invocation
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Update calendar if just created
                    if (calendar != null)
                    {
                        ReminderCalendar = calendar;
                    }
                    
                    // Update calendar events
                    ReminderCalendar!.Events.ReplaceRange(allPlantEvents);
                    
                    // Clear selections
                    TickedPlantEvents.Clear();
                });

                // 5) Update plant events list (respects filter and date selection)
                await UpdatePlantEventsOnSelectedCalendarDates();
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
                _updateLock.Release();
            }
        }

        private async Task UpdatePlantEventsOnSelectedCalendarDates()
        {
            if (ReminderCalendar == null)
            {
                return;
            }

            List<PlantEvent> eventsToShow;

            if (ReminderCalendar.SelectedDates.Count > 0)
            {
                // Optimize: Use HashSet for O(1) lookup instead of O(n) Any()
                var selectedDatesSet = new HashSet<DateTime>(ReminderCalendar.SelectedDates);
                
                eventsToShow = ReminderCalendar.Events
                    .Where(pEvt => selectedDatesSet.Contains(pEvt.StartDate))
                    .OrderBy(pEvt => pEvt.ScheduledTime)
                    .ToList();
            }
            else
            {
                // Apply current filter to all events
                eventsToShow = FilterCachedEvents(_cachedAllEvents ?? ReminderCalendar.Events.ToList());
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                PlantEvents.ReplaceRange(eventsToShow);
            });
        }

        #region Load Data

        private async Task<List<PlantEvent>> GetPlantEventsAsync()
        {
            // Return cached events if still valid
            if (_cachedAllEvents != null && 
                DateTime.Now - _cacheTimestamp < _cacheLifetime)
            {
                return FilterCachedEvents(_cachedAllEvents);
            }

            // Regenerate cache
            List<Plant> plants = await _plantService.GetAllPlantsAsync();
            _cachedAllEvents = await GenerateAllEventsAsync(plants);
            _cacheTimestamp = DateTime.Now;

            return FilterCachedEvents(_cachedAllEvents);
        }

        /// <summary>
        /// Filters cached events based on current filter settings
        /// </summary>
        private List<PlantEvent> FilterCachedEvents(List<PlantEvent> allEvents)
        {
            if (!IsShowUnattendedOnly)
                return allEvents;

            DateTime now = DateTime.Now;
            return allEvents.Where(e => e.ScheduledTime <= now).ToList();
        }

        /// <summary>
        /// Generates all plant events efficiently in a single loop on background thread
        /// </summary>
        private Task<List<PlantEvent>> GenerateAllEventsAsync(List<Plant> plants)
        {
            return Task.Run(() =>
            {
                var events = new List<PlantEvent>(plants.Count * 2);

                foreach (Plant plant in plants)
                {
                    // Create both water and fertilize events in single loop
                    DateTime waterTime = plant.LastWatered.AddHours(plant.WateringFrequencyInHours);
                    DateTime waterDate = waterTime.Date; // Cache date calculation
                    var waterState = PlantState.GetCurrentStateValue(waterTime);
                    
                    events.Add(new PlantEvent
                    {
                        PlantId = plant.Id,
                        ReminderType = CareType.Watering,
                        Name = plant.Name,
                        PhotoPath = plant.ThumbnailPath, // Use thumbnail for better performance
                        StartDate = waterDate,
                        EndDate = waterDate.AddDays(1),
                        Color = ProgressToColorConverter.Convert(waterState),
                        ScheduledTime = waterTime
                    });

                    DateTime fertilizeTime = plant.LastFertilized.AddHours(plant.FertilizeFrequencyInHours);
                    DateTime fertilizeDate = fertilizeTime.Date; // Cache date calculation
                    var fertilizeState = PlantState.GetCurrentStateValue(fertilizeTime);
                    
                    events.Add(new PlantEvent
                    {
                        PlantId = plant.Id,
                        ReminderType = CareType.Fertilization,
                        Name = plant.Name,
                        PhotoPath = plant.ThumbnailPath, // Use thumbnail for better performance
                        StartDate = fertilizeDate,
                        EndDate = fertilizeDate.AddDays(1),
                        Color = ProgressToColorConverter.Convert(fertilizeState),
                        ScheduledTime = fertilizeTime
                    });
                }

                // Sort once at the end - more efficient than sorting during insertion
                events.Sort((a, b) => a.ScheduledTime.CompareTo(b.ScheduledTime));

                return events;
            });
        }

        #endregion Load Data

        #region Mark done

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
                {
                    return;
                }

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
                {
                    return;
                }

                IsBusy = true;

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

                // Invalidate cache after changes
                InvalidateCache();

                // Clear selections
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TickedPlantEvents.Clear();
                });

                // Refresh the entire list to show updated events
                await UpdateCalendarAndEventListAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
            finally
            {
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
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                
                // Invalidate cache to force fresh data
                InvalidateCache();
                
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
            {
                return;
            }

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
        private string _orientationState = "Portrait";

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
                PhotoSpan = ((int)Width - 10 - (IsShowCalendar ? ConstantValues.CalendarWidth : 0)) / PhotoWidth;
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

        void IDisposable.Dispose()
        {
            if (ReminderCalendar is not null)
            {
                ReminderCalendar.SelectedDates.CollectionChanged -= SelectedDates_CollectionChanged;
            }
            
            _updateLock.Dispose();
            
            //DeviceDisplay.MainDisplayInfoChanged -= OnDeviceDisplay_MainDisplayInfoChanged;
        }
    }
}
