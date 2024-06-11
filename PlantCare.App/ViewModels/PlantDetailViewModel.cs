using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.Data;
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
            Plant plant = MapToDataModel(this);
            await _navigationService.GoToEditPlant(plant);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[Consts.Error] ?? Consts.Error, ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeletePlant()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            bool isConfirmed = await _dialogService.Ask(
                LocalizationManager.Instance[Consts.Confirm] ?? "Confirm",
                $"{LocalizationManager.Instance[Consts.ConfirmDelete]}: {Name}?",
                LocalizationManager.Instance[Consts.Yes] ?? "Yes",
                LocalizationManager.Instance[Consts.No] ?? "No");

            if (!isConfirmed)
            {
                return;
            }
            IsLoading = true;

            await Task.Run(async () =>
            {
                await _plantService.DeletePlantAsync(Id);

                WeakReferenceMessenger.Default.Send(new PlantDeletedMessage { PlantId = Id });
            });
            await _navigationService.GoToPlantsOverview();
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[Consts.Error] ?? Consts.Error, ex.Message);
        }
        finally
        {
            IsBusy = false;
            IsLoading = false;
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
        try
        {
            await LoadPlantDetailAsync(Id);
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[Consts.Error] ?? Consts.Error, ex.Message);
        }
    }

    private async Task LoadPlantDetailAsync(Guid plantId)
    {
        Plant? plant = await _plantService.GetPlantByIdAsync(plantId);
        if (plant is null)
        {
            return;
        }

        MapToViewModel(plant);
    }

    internal async void NavigateBack()
    {
        await _navigationService.GoToPlantsOverview();
    }

    private void MapToViewModel(Plant plant)
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

    private static Plant MapToDataModel(PlantDetailViewModel viewModel)
    {
        return new Plant
        {
            Id = viewModel.Id,
            Species = viewModel.Species,
            Name = viewModel.Name,
            Age = viewModel.Age,
            PhotoPath = viewModel.PhotoPath,

            LastWatered = viewModel.LastWatered,
            WateringFrequencyInHours = viewModel.WateringFrequencyInHours,
            LastFertilized = viewModel.LastFertilized,
            FertilizeFrequencyInHours = viewModel.FertilizeFrequencyInHours
        };
    }
}