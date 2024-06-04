﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.ViewModels;

public partial class PlantDetailViewModel(IPlantService plantService, INavigationService navigationService, IDialogService dialogService) : PlantViewModelBase, IQueryAttributable
{
    private readonly IPlantService _plantService = plantService;
    private readonly INavigationService _navigationService = navigationService;
    private readonly IDialogService _dialogService = dialogService;
    [ObservableProperty]
    private string _species = string.Empty;

    [ObservableProperty]
    private int _age;

    [RelayCommand]
    private async Task NavigateToEditPlant()
    {
        try
        {
            PlantDbModel plant = PlantDetailViewModel.MapToPlantModel(this);
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
        if (!query.TryGetValue("PlantId", out object? value))
            return;

        var plantId = value.ToString();
        if (Guid.TryParse(plantId, out var selectedId))
        {
            Id = selectedId;
        }
    }

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        await LoadPlantDetailAsync(Id);
    }

    private async Task LoadPlantDetailAsync(Guid plantId)
    {
        Plant? plant = await _plantService.GetPlantByIdAsync(plantId);
        if (plant is null)
        {
            return;
        }

        MapPlantData(plant);
    }

    internal async void NavigateBack()
    {
        await _navigationService.GoToPlantsOverview();
    }

    private void MapPlantData(Plant plant)
    {
        if (plant is null)
        {
            return;
        }

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

    private static PlantDbModel MapToPlantModel(PlantDetailViewModel plantDetailViewModel)
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