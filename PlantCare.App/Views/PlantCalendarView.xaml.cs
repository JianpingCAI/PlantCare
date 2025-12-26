using PlantCare.App.ViewModels;
using System.ComponentModel;
using System.Diagnostics;

namespace PlantCare.App.Views;

public partial class PlantCalendarView : ContentPageBase
{
    public PlantCalendarView(PlantCalendarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (BindingContext is PlantCalendarViewModel viewModel)
        {
            viewModel.Width = width;
            viewModel.Height = height;
            viewModel.UpdateOrientation();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (BindingContext is not PlantCalendarViewModel viewModel)
            return;

        switch (e.PropertyName)
        {
            case nameof(viewModel.OrientationState):
                VisualStateManager.GoToState(MainGrid, viewModel.OrientationState);
                break;

            case nameof(viewModel.IsShowCalendar):
                // When hiding the calendar, re-apply visual state immediately
                if (!viewModel.IsShowCalendar)
                {
                    VisualStateManager.GoToState(MainGrid, viewModel.OrientationState);
                }
                break;

            case nameof(viewModel.IsCalendarRendered):
                // Re-apply visual state when calendar is rendered or hidden
                VisualStateManager.GoToState(MainGrid, viewModel.OrientationState);
                
                // Force calendar refresh after rendering to display events
                if (viewModel.IsCalendarRendered)
                {
                    ForceCalendarRefreshAsync();
                }
                break;

            case nameof(viewModel.CalendarRefreshToken):
                // Force calendar refresh when events are updated
                if (viewModel.IsCalendarRendered)
                {
                    ForceCalendarRefreshAsync();
                }
                break;
        }
    }

    private async void ForceCalendarRefreshAsync()
    {
        try
        {
            if (BindingContext is not PlantCalendarViewModel viewModel)
                return;

            // Wait for the calendar to finish rendering
            await Task.Delay(100);

            if (PluginCalendar == null || !viewModel.IsCalendarRendered)
                return;

            // Toggle month to force calendar re-render
            DateTime currentDate = PluginCalendar.ShownDate;
            PluginCalendar.ShownDate = currentDate.AddMonths(1);

            await Task.Delay(100);

            PluginCalendar.ShownDate = currentDate;

            Debug.WriteLine($"[PlantCalendarView] Calendar refreshed with {viewModel.EventsInCalendar?.Count ?? 0} event dates");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PlantCalendarView] Calendar refresh error: {ex.Message}");
        }
    }
}
