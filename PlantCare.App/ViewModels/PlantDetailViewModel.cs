namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PlantDetailViewModel : ViewModelBase, IQueryAttributable
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;

    public PlantDetailViewModel(IPlantService plantService, INavigationService navigationService)
    {
        _plantService = plantService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _species = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _age;

    [ObservableProperty]
    private DateTime _lastWatered;

    [ObservableProperty]
    private string _photoPath = string.Empty;

    [RelayCommand]
    private async Task ChangeStatus()
    {
        await Task.Delay(100);
        WeakReferenceMessenger.Default.Send(new StatusChangedMessage(Id, Name, WateredStatus.Watered));
    }

    [RelayCommand]
    private async Task NavigateToEditPlant()
    {
        PlantDbModel plant = MapToPlantModel(this);
        _navigationService.GoToEditPlant(plant);
    }

    // Implement IQueryAttributable
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.ContainsKey("PlantId"))
            return;

        var eventId = query["PlantId"].ToString();
        if (Guid.TryParse(eventId, out var selectedId))
        {
            Id = selectedId;

            //await GetPlantDetailAsync(selectedId);
        }
    }

    public override async Task LoadAsync()
    {
        await Loading(async () =>
        {
            await GetPlantDetailAsync(Id);
        });
    }

    private async Task GetPlantDetailAsync(Guid plantId)
    {
        //Plant plant = await _plantService.GetPlantByIdAsync(plantId);
        await Task.Delay(1000);

        PlantDbModel plant = new PlantDbModel
        {
            Name = "Plant3",
            Species = "species",
            PhotoPath = "https://picsum.photos/200/300"
        };

        MapPlantData(plant);
    }

    private void MapPlantData(PlantDbModel plant)
    {
        Id = plant.Id;
        Species = plant.Species;
        Name = plant.Name;
        Age = plant.Age;
        LastWatered = plant.LastWatered;
        PhotoPath = plant.PhotoPath;
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
        };
    }
}