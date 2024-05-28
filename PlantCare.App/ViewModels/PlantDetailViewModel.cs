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

    private void MapPlantData(Plant plant)
    {
        Id = plant.Id;
        Species = plant.Species;
        Name = plant.Name;
        Age = plant.Age;
        LastWatered = plant.LastWatered;
        PhotoPath = plant.PhotoPath;

        WateringFrequencyInHours = plant.WateringFrequencyInHours;
    }

    private PlantDbModel MapToPlantModel(PlantDetailViewModel plantDetailViewModel)
    {
        return new PlantDbModel
        {
            Id = plantDetailViewModel.Id,
            Species = plantDetailViewModel.Species,
            Name = plantDetailViewModel.Name,
            Age = plantDetailViewModel.Age,
            LastWatered = plantDetailViewModel.LastWatered,
            PhotoPath = plantDetailViewModel.PhotoPath,
            WateringFrequencyInHours = plantDetailViewModel.WateringFrequencyInHours
        };
    }

    internal async void NavidateBack()
    {
        await _navigationService.GoToPlantsOverview();
    }
}