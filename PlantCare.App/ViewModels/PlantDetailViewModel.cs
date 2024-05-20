﻿namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using PlantCare.App.Services;
using PlantCare.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PlantDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IPlantService _plantService;

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _species = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private string _lastWatered = string.Empty;

    [ObservableProperty]
    private string _photoPath = string.Empty;

    public PlantDetailViewModel(IPlantService plantService)
    {
        _plantService = plantService;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var eventId = query["PlantId"].ToString();
        if (Guid.TryParse(eventId, out var selectedId))
        {
            Id = selectedId;

            await GetPlantDetailAsync(selectedId);
        }
    }

    private async Task GetPlantDetailAsync(Guid plantId)
    {
        //Plant plant = await _plantService.GetPlantByIdAsync(plantId);
        await Task.Delay(1000);

        Plant plant = new Plant
        {
            Name = "Plant3",
            Species = "species",
            PhotoPath = "https://picsum.photos/200/300"
        };

        MapData(plant);
    }

    private void MapData(Plant plant)
    {
        Id = plant.Id;
        Species = plant.Species;
        Name = plant.Name;
        Age = plant.Age;
        LastWatered = plant.LastWatered;
        PhotoPath = plant.PhotoPath;
    }
}