using CommunityToolkit.Mvvm.ComponentModel;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data.Models;

namespace PlantCare.App.ViewModels;

public partial class PlantViewModelBase : ViewModelBase
{
    public Guid Id { get; set; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _photoPath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextWateringTime))]
    [NotifyPropertyChangedFor(nameof(HoursUntilNextWatering))]
    [NotifyPropertyChangedFor(nameof(WaterState))]
    private DateTime _lastWatered;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextWateringTime))]
    [NotifyPropertyChangedFor(nameof(HoursUntilNextWatering))]
    [NotifyPropertyChangedFor(nameof(WaterState))]
    public int _wateringFrequencyInHours;

    public DateTime NextWateringTime => LastWatered.AddHours(WateringFrequencyInHours);

    public double HoursUntilNextWatering => (LastWatered.AddHours(WateringFrequencyInHours) - DateTime.Now).TotalHours;

    private double _waterState;

    public double WaterState
    {
        get
        {
            return PlantState.GetCurrentStateValue(NextWateringTime);
        }

        set
        {
            SetProperty(ref _waterState, value);
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextFertilizeTime))]
    [NotifyPropertyChangedFor(nameof(HoursUntilNextFertilize))]
    [NotifyPropertyChangedFor(nameof(WaterState))]
    private DateTime _lastFertilized;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextFertilizeTime))]
    [NotifyPropertyChangedFor(nameof(HoursUntilNextFertilize))]
    [NotifyPropertyChangedFor(nameof(FertilizeState))]
    public int _fertilizeFrequencyInHours;

    public DateTime NextFertilizeTime => LastFertilized.AddHours(FertilizeFrequencyInHours);

    public double HoursUntilNextFertilize => (LastFertilized.AddHours(FertilizeFrequencyInHours) - DateTime.Now).TotalHours;

    public double _fertilizeState;

    public double FertilizeState
    {
        get
        {
            return PlantState.GetCurrentStateValue(NextFertilizeTime);
        }
        set
        {
            SetProperty(ref _fertilizeState, value);
        }
    }

    public void RefreshStates()
    {
        WaterState = PlantState.GetCurrentStateValue(NextWateringTime);
        FertilizeState = PlantState.GetCurrentStateValue(NextFertilizeTime);
    }
}