using PlantCare.App.ViewModels;
using System.ComponentModel;

namespace PlantCare.App.Views;

public partial class PlantCalendarView : ContentPageBase
{
    public PlantCalendarView(PlantCalendarViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

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
        if(BindingContext is PlantCalendarViewModel viewModel 
            && e.PropertyName == nameof(viewModel.OrientationState))
        {
            VisualStateManager.GoToState(MainGrid, viewModel.OrientationState);
        }
    }
}