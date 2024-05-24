namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class PlantDetailViewModel : PlantViewModelBase, IQueryAttributable, IRecipient<PlantAddedOrChangedMessage>
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;

    public PlantDetailViewModel(IPlantService plantService, INavigationService navigationService)
    {
        _plantService = plantService;
        _navigationService = navigationService;
        WeakReferenceMessenger.Default.Register<PlantAddedOrChangedMessage>(this);
    }

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
        await _navigationService.GoToEditPlant(plant);
    }

    [RelayCommand]
    private async Task DeletePlant()
    {
        try
        {
            await _plantService.DeletePlantAsync(Id);
        }
        catch (Exception ex)
        {
            return;
        }

        try
        {
            WeakReferenceMessenger.Default.Send(new PlantDeletedMessage { PlantId = Id });
        }
        catch (Exception)
        {
            return;
        }

        await _navigationService.GoBack();
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
            WateringFrequencyInHours = plantDetailViewModel.HoursUntilNextWatering
        };
    }

    async void IRecipient<PlantAddedOrChangedMessage>.Receive(PlantAddedOrChangedMessage message)
    {
        //if (message.PlantId is null)
        //{
        //    return;
        //}
        //Id = message.PlantId.Value;
        //await LoadAsync();
    }
}