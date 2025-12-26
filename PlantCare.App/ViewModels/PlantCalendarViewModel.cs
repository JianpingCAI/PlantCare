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
using System.Diagnostics;
using System.Globalization;
using Plugin.Maui.Calendar.Models;

namespace PlantCare.App.ViewModels
{
    public class PlantEvent : ObservableObject
    {
        public Guid PlantId { get; set; }
        public CareType ReminderType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Color Color { get; set; } = Colors.Green;

        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public DateTime ScheduledTime { get; set; } = default;
    }

    public partial class PlantCalendarViewModel : ViewModelBase,
        IRecipient<LanguageChangedMessage>, IDisposable
    {
        private readonly IPlantService _plantService;
        private readonly IDialogService _dialogService;

        public PlantCalendarViewModel(IPlantService plantService, IDialogService dialogService)
        {
            _plantService = plantService ?? throw new ArgumentNullException(nameof(plantService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            _displayedMonth = DateTime.Today;
            _culture = CultureInfo.CurrentUICulture;

            WeakReferenceMessenger.Default.Register<LanguageChangedMessage>(this);
        }

        #region Event Caching

        private List<PlantEvent>? _cachedAllEvents;
        private DateTime _cacheTimestamp;
        private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);
        private readonly SemaphoreSlim _updateLock = new(1, 1);

        private void InvalidateCache() => _cachedAllEvents = null;

        #endregion Event Caching

        #region Calendar Properties

        [ObservableProperty]
        private EventCollection _eventsInCalendar = new();

        private DateTime? _selectedDate = null;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    _ = UpdateVisiblePlantEventsAsync();
                }
            }
        }

        private DateTime _displayedMonth = DateTime.Today;
        public DateTime DisplayedMonth
        {
            get => _displayedMonth;
            set => SetProperty(ref _displayedMonth, value);
        }

        private CultureInfo _culture = CultureInfo.CurrentUICulture;
        public CultureInfo Culture
        {
            get => _culture;
            private set => SetProperty(ref _culture, value);
        }

        #endregion Calendar Properties

        #region UI State Properties

        private bool _isShowUnattendedOnly = true;
        public bool IsShowUnattendedOnly
        {
            get => _isShowUnattendedOnly;
            set
            {
                if (SetProperty(ref _isShowUnattendedOnly, value))
                {
                    _ = OnShowUnattendedOnlyChangedAsync();
                }
            }
        }

        public ObservableRangeCollection<PlantEvent> PlantEvents { get; } = new();

        [ObservableProperty]
        private ObservableCollection<object> _tickedPlantEvents = [];

        [ObservableProperty]
        private bool _isSetRemindersDoneEnabled = false;

        [ObservableProperty]
        private string _selectAllButtonText = ConstStrings.SelectAll;

        private bool _isShowCalendar = false;
        public bool IsShowCalendar
        {
            get => _isShowCalendar;
            set
            {
                if (SetProperty(ref _isShowCalendar, value))
                {
                    if (value)
                    {
                        _ = ShowCalendarAsync();
                    }
                    else
                    {
                        IsCalendarRendered = false;
                    }
                    AdjustPlantsGridSpan();
                }
            }
        }

        [ObservableProperty]
        private bool _isCalendarRendered = false;

        [ObservableProperty]
        private bool _isPlantEventRefreshing = false;

        // Property to signal that calendar events need to be refreshed
        // Incrementing this value triggers ForceCalendarRefresh in code-behind
        [ObservableProperty]
        private int _calendarRefreshToken = 0;

        private void RequestCalendarRefresh() => CalendarRefreshToken++;

        #endregion UI State Properties

        #region Commands

        [RelayCommand]
        public void HideCalendar()
        {
            IsShowCalendar = false;
            IsCalendarRendered = false;
        }

        [RelayCommand]
        public void NavigateCalendar(int months)
        {
            DateTime baseDate = SelectedDate ?? DateTime.Today;
            DateTime newDate = baseDate.AddMonths(months);
            SelectedDate = newDate;
            DisplayedMonth = newDate;
        }

        [RelayCommand]
        public async Task TickedPlantEventsChanged(object args)
        {
            try
            {
                SelectAllButtonText = TickedPlantEvents.Count > 0 ? ConstStrings.Unselect : ConstStrings.SelectAll;
                IsSetRemindersDoneEnabled = TickedPlantEvents.Count > 0;

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
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
                Debug.WriteLine($"[Calendar] TickedPlantEventsChanged error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task SetSelectedRemindersDone()
        {
            if (IsBusy)
                return;

            try
            {
                DateTime updatedCareTime = DateTime.Now;

                bool isConfirmed = await _dialogService.Ask(
                    LocalizationManager.Instance[ConstStrings.Confirm] ?? ConstStrings.Confirm,
                    $"{LocalizationManager.Instance[ConstStrings.MarkDone]}: {updatedCareTime.ToShortTimeString()}?",
                    LocalizationManager.Instance[ConstStrings.Yes] ?? ConstStrings.Yes,
                    LocalizationManager.Instance[ConstStrings.No] ?? ConstStrings.No);

                if (!isConfirmed)
                    return;

                IsBusy = true;

                foreach (object item in TickedPlantEvents)
                {
                    if (item is PlantEvent plantEvent)
                    {
                        await UpdatePlantCareTimeAsync(plantEvent, updatedCareTime);
                    }
                }

                InvalidateCache();
                await MainThread.InvokeOnMainThreadAsync(() => TickedPlantEvents.Clear());
                await ReloadAllEventsAsync();
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

        [RelayCommand]
        public async Task RefreshPlantEvents()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                InvalidateCache();
                await ReloadAllEventsAsync();
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

        #endregion Commands

        #region Lifecycle Methods

        public override async Task LoadDataWhenViewAppearingAsync()
        {
            try
            {
                SyncCultureWithAppLanguage();
                await ReloadAllEventsAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
        }

        #endregion Lifecycle Methods

        #region Private Methods

        private async Task ShowCalendarAsync()
        {
            IsCalendarRendered = false;
            DisplayedMonth = SelectedDate ?? DateTime.Today;
            SyncThreadCulture();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    IsLoading = true;

                    PopulateAllEventsInCalendar();

                    List<PlantEvent> eventsToShow = FilterEvents(_cachedAllEvents ?? []);
                    PlantEvents.ReplaceRange(eventsToShow);
                    TickedPlantEvents.Clear();

                    // Small delay to ensure EventsInCalendar binding is processed before rendering
                    await Task.Delay(50);

                    // Setting IsCalendarRendered triggers ForceCalendarRefresh in code-behind
                    IsCalendarRendered = true;
                    Debug.WriteLine($"[Calendar] Rendered with {EventsInCalendar.Count} dates containing events");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Calendar] Failed to show calendar: {ex.Message}");
                    IsCalendarRendered = true;
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        private async Task OnShowUnattendedOnlyChangedAsync()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = true);

                if (_cachedAllEvents != null)
                {
                    List<PlantEvent> filteredEvents = FilterEvents(_cachedAllEvents);
                    await MainThread.InvokeOnMainThreadAsync(() => PlantEvents.ReplaceRange(filteredEvents));
                }
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
            }
        }

        private async Task ReloadAllEventsAsync()
        {
            if (!await _updateLock.WaitAsync(0))
            {
                Debug.WriteLine("[Calendar] Update already in progress, skipping");
                return;
            }

            try
            {
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = true);

                await GenerateAllEventsAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PopulateAllEventsInCalendar();
                    TickedPlantEvents.Clear();
                });

                await UpdateVisiblePlantEventsAsync();

                // Request calendar to refresh its display if it's visible
                if (IsCalendarRendered)
                {
                    await MainThread.InvokeOnMainThreadAsync(() => RequestCalendarRefresh());
                }
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
                _updateLock.Release();
            }
        }

        private void PopulateAllEventsInCalendar()
        {
            EventsInCalendar.Clear();

            if (_cachedAllEvents == null || _cachedAllEvents.Count == 0)
            {
                Debug.WriteLine("[Calendar] No events to display");
                return;
            }

            Dictionary<DateTime, List<object>> eventsByDate = _cachedAllEvents
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Cast<object>().ToList());

            foreach (KeyValuePair<DateTime, List<object>> kvp in eventsByDate)
            {
                EventsInCalendar[kvp.Key] = kvp.Value;
            }

            Debug.WriteLine($"[Calendar] Populated {EventsInCalendar.Count} dates with {_cachedAllEvents.Count} total events");
        }

        private async Task UpdateVisiblePlantEventsAsync()
        {
            List<PlantEvent> filteredEvents;

            if (_cachedAllEvents == null)
            {
                filteredEvents = [];
            }
            else
            {
                // No date selected - show all events with filter applied
                filteredEvents = FilterEvents(_cachedAllEvents);
            }

            await MainThread.InvokeOnMainThreadAsync(() => PlantEvents.ReplaceRange(filteredEvents));
        }

        private async Task GenerateAllEventsAsync()
        {
            if (_cachedAllEvents != null && DateTime.Now - _cacheTimestamp < _cacheLifetime)
            {
                return;
            }

            List<Plant> plants = await _plantService.GetAllPlantsAsync();
            _cachedAllEvents = GenerateAllEvents(plants);
            _cacheTimestamp = DateTime.Now;
        }

        private List<PlantEvent> FilterEvents(List<PlantEvent> events)
        {
            if (SelectedDate.HasValue)
            {
                // Specific date selected - show events for that date, respecting the filter
                events = events
                    .Where(e => e.Date.Date == SelectedDate.Value.Date)
                    .ToList();
            }

            if (!IsShowUnattendedOnly)
                return events;

            DateTime now = DateTime.Now;
            return events.Where(e => e.ScheduledTime <= now).OrderBy(e => e.ScheduledTime).ToList();
        }

        private static List<PlantEvent> GenerateAllEvents(List<Plant> plants)
        {
            var events = new List<PlantEvent>(plants.Count * 2);

            foreach (Plant plant in plants)
            {
                // Watering event
                DateTime waterTime = plant.LastWatered.AddHours(plant.WateringFrequencyInHours);
                events.Add(new PlantEvent
                {
                    PlantId = plant.Id,
                    ReminderType = CareType.Watering,
                    Name = plant.Name,
                    PhotoPath = plant.ThumbnailPath,
                    Date = waterTime.Date,
                    Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(waterTime)),
                    ScheduledTime = waterTime
                });

                // Fertilization event
                DateTime fertilizeTime = plant.LastFertilized.AddHours(plant.FertilizeFrequencyInHours);
                events.Add(new PlantEvent
                {
                    PlantId = plant.Id,
                    ReminderType = CareType.Fertilization,
                    Name = plant.Name,
                    PhotoPath = plant.ThumbnailPath,
                    Date = fertilizeTime.Date,
                    Color = ProgressToColorConverter.Convert(PlantState.GetCurrentStateValue(fertilizeTime)),
                    ScheduledTime = fertilizeTime
                });
            }

            events.Sort((a, b) => a.ScheduledTime.CompareTo(b.ScheduledTime));
            return events;
        }

        private async Task UpdatePlantCareTimeAsync(PlantEvent plantEvent, DateTime updatedTime)
        {
            switch (plantEvent.ReminderType)
            {
                case CareType.Watering:
                    await _plantService.UpdateLastWateringTime(plantEvent.PlantId, updatedTime);
                    break;
                case CareType.Fertilization:
                    await _plantService.UpdateLastFertilizationTime(plantEvent.PlantId, updatedTime);
                    break;
            }

            WeakReferenceMessenger.Default.Send(new PlantStateChangedMessage(
                plantEvent.PlantId,
                plantEvent.ReminderType,
                updatedTime));
        }

        private void SyncCultureWithAppLanguage()
        {
            Language appLanguage = App.AppLanguage;
            string expectedCultureName = LanguageHelper.GetCultureName(appLanguage);

            if (Culture.Name != expectedCultureName)
            {
                Culture = new CultureInfo(expectedCultureName);
                Debug.WriteLine($"[Calendar] Culture synchronized to {Culture.Name}");
            }
        }

        private static void SyncThreadCulture()
        {
            CultureInfo currentCulture = CultureInfo.CurrentUICulture;
            CultureInfo.DefaultThreadCurrentCulture = currentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = currentCulture;
        }

        #endregion Private Methods

        #region Message Handlers

        async void IRecipient<LanguageChangedMessage>.Receive(LanguageChangedMessage message)
        {
            if (string.IsNullOrEmpty(message?.CultureCode))
                return;

            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var newCulture = new CultureInfo(message.CultureCode);
                    CultureInfo.DefaultThreadCurrentCulture = newCulture;
                    CultureInfo.DefaultThreadCurrentUICulture = newCulture;
                    Culture = newCulture;

                    Debug.WriteLine($"[Calendar] Language changed to {message.CultureCode}");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Calendar] Language change error: {ex.Message}");
            }
        }

        #endregion Message Handlers

        #region Layout

        [ObservableProperty]
        private string _orientationState = "Portrait";

        public double Width { get; set; }
        public double Height { get; set; }

        [ObservableProperty]
        private int _photoWidth = 110;

        [ObservableProperty]
        private int _photoHeight = 120;

        [ObservableProperty]
        private int _photoSpan = 3;

        public void UpdateOrientation()
        {
            if (Width > 0 && Height > 0)
            {
                OrientationState = Width > Height ? "Landscape" : "Portrait";
                AdjustPlantsGridSpan();
            }
        }

        private void AdjustPlantsGridSpan()
        {
            int availableWidth = (int)Width - 10;

            // In landscape mode with calendar shown, subtract calendar width
            // In portrait mode, plants always have full width (calendar is below)
            bool isLandscape = Width > Height;
            if (isLandscape && IsShowCalendar)
            {
                availableWidth -= ConstantValues.CalendarWidth;
            }

            PhotoSpan = Math.Max(1, availableWidth / PhotoWidth);
        }

        #endregion Layout

        #region IDisposable

        void IDisposable.Dispose()
        {
            _updateLock?.Dispose();
            EventsInCalendar?.Clear();
            _cachedAllEvents?.Clear();
            _cachedAllEvents = null;
            WeakReferenceMessenger.Default.Unregister<LanguageChangedMessage>(this);
            Debug.WriteLine("[PlantCalendarViewModel] Disposed");
        }

        #endregion IDisposable
    }
}
