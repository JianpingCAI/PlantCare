namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.Data.Models;
using PlantCare.App.Services;

using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;

public partial class PlantListOverviewViewModel : BaseViewModel
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<PlantListItemViewModel> plants = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private PlantListItemViewModel? _selectedPlant = null;

    public PlantListOverviewViewModel(IPlantService plantService, INavigationService navigationService)
    {
        _plantService = plantService;
        _navigationService = navigationService;
        LoadPlants();
    }

    [RelayCommand]
    private async void LoadPlants()
    {
        if (IsBusy)
            return;

        try
        {
            List<Plant> plants = await _plantService.GetAllPlantsAsync();
            List<PlantListItemViewModel> viewModels = [];
            if (plants.Count == 0)
            {
                viewModels.Add(MapToViewModel(new Plant
                {
                    Name = "Plant1",
                    Species = "species",
                    PhotoPath = "https://picsum.photos/200/300"
                }));
                viewModels.Add(MapToViewModel(new Plant
                {
                    Name = "Plant2",
                    Species = "species",
                    PhotoPath = "https://picsum.photos/200/300"
                }));
            }
            Plants = viewModels.ToObservableCollection();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async void SelectPlant()
    {
        if (IsBusy)
            return;

        try
        {
            // Navigate to details view with selected plant
            if (SelectedPlant is not null)
            {
                await _navigationService.GotoPlantDetail(_selectedPlant.Id);

                SelectedPlant = null;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddPlant()
    {
        if (IsBusy)
            return;

        try
        {
            //// Simulate opening a dialog to get new plant details
            //var newPlant = await DialogService.OpenAddPlantDialog();
            //if (newPlant != null)
            //{
            //    await _plantService.AddPlantAsync(newPlant);
            //    Plants.Add(newPlant); // Add to observable collection to update UI
            //}
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Search()
    {
        if (IsBusy)
            return;

        try
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                //var searchResults = await _plantService.SearchPlantsAsync(SearchText);
                //Plants.Clear();
                //foreach (var plant in searchResults)
                //{
                //    Plants.Add(plant);
                //}
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public static PlantListItemViewModel MapToViewModel(Plant plant)
    {
        return new PlantListItemViewModel
        {
            Id = plant.Id,
            Species = plant.Species,
            Name = plant.Name,
            Age = plant.Age,
            LastWatered = plant.LastWatered,
            PhotoPath = plant.PhotoPath
        };
    }
}