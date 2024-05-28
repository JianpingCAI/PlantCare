using CommunityToolkit.Mvvm.ComponentModel;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data.Models;

namespace PlantCare.App.ViewModels;

public partial class PlantViewModelBase : ViewModelBase
{
    public Guid Id { get; set; }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _species = string.Empty;

    [ObservableProperty]
    private int _age;

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

    public double WaterState => Math.Max(0, (NextWateringTime - DateTime.Now).TotalMinutes / (WateringFrequencyInHours * 60.0));
}