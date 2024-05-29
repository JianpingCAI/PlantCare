using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.ViewModels;

public partial class PlantDetailViewModel : PlantViewModelBase, IQueryAttributable
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public PlantDetailViewModel(IPlantService plantService, INavigationService navigationService, IDialogService dialogService)
    {
        _plantService = plantService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private string _species = string.Empty;

    [ObservableProperty]
    private int _age;

    [RelayCommand]
    private async Task NavigateToEditPlant()
    {
        try
        {
            PlantDbModel plant = MapToPlantModel(this);
            await _navigationService.GoToEditPlant(plant);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeletePlant()
    {
        try
        {
            bool isConfirmed = await _dialogService.Ask("Confirm", $"Are you sure to delete {Name}?");

            if (!isConfirmed)
            {
                return;
            }

            await _plantService.DeletePlantAsync(Id);

            WeakReferenceMessenger.Default.Send(new PlantDeletedMessage { PlantId = Id });

            await _navigationService.GoToPlantsOverview();
        }
        catch (Exception ex)
        {
            await _dialogService.Notify("Error", ex.Message);
        }
    }

    void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.ContainsKey("PlantId"))
            return;

        var plantId = query["PlantId"].ToString();
        if (Guid.TryParse(plantId, out var selectedId))
        {
            Id = selectedId;
        }
    }

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        await LoadingDataWhenViewAppearing(async () =>
        {
            await GetPlantDetailAsync(Id);
        });
    }

    private async Task GetPlantDetailAsync(Guid plantId)
    {
        Plant plant = await _plantService.GetPlantByIdAsync(plantId);
        if (plant is null)
        {
            return;
        }

        MapPlantData(plant);
    }

    internal async void NavidateBack()
    {
        await _navigationService.GoToPlantsOverview();
    }

    private void MapPlantData(Plant plant)
    {
        Id = plant.Id;
        Species = plant.Species;
        Name = plant.Name;
        Age = plant.Age;
        PhotoPath = plant.PhotoPath;

        LastWatered = plant.LastWatered;
        WateringFrequencyInHours = plant.WateringFrequencyInHours;

        LastFertilized = plant.LastFertilized;
        FertilizeFrequencyInHours = plant.FertilizeFrequencyInHours;
    }

    private PlantDbModel MapToPlantModel(PlantDetailViewModel plantDetailViewModel)
    {
        return new PlantDbModel
        {
            Id = plantDetailViewModel.Id,
            Species = plantDetailViewModel.Species,
            Name = plantDetailViewModel.Name,
            Age = plantDetailViewModel.Age,
            PhotoPath = plantDetailViewModel.PhotoPath,

            LastWatered = plantDetailViewModel.LastWatered,
            WateringFrequencyInHours = plantDetailViewModel.WateringFrequencyInHours,
            LastFertilized = plantDetailViewModel.LastFertilized,
            FertilizeFrequencyInHours = plantDetailViewModel.FertilizeFrequencyInHours
        };
    }
}