namespace PlantCare.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlantCare.App.Services;

using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using PlantCare.App.ViewModels.Base;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.Data.Models;

public partial class PlantListOverviewViewModel : ViewModelBase, IRecipient<PlantAddedOrChangedMessage>, IRecipient<PlantDeletedMessage>
{
    private readonly IPlantService _plantService;
    private readonly INavigationService _navigationService;

    public PlantListOverviewViewModel(IPlantService plantService, INavigationService navigationService)
    {
        _plantService = plantService;
        _navigationService = navigationService;

        WeakReferenceMessenger.Default.Register<PlantAddedOrChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlantDeletedMessage>(this);
    }

    [ObservableProperty]
    private ObservableCollection<PlantListItemViewModel> plants = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private PlantListItemViewModel? _selectedPlant = null;

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
                await _navigationService.GoToPlantDetail(_selectedPlant.Id);

                SelectedPlant = null;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Override
    public override async Task LoadAsync()
    {
        if (Plants.Count == 0)
        {
            await Loading(GetPlants);
        }
    }

    private async Task GetPlants()
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
        else
        {
            foreach (var plant in plants)
            {
                viewModels.Add(MapToViewModel(plant));
            }
        }

        Plants.Clear();
        Plants = viewModels.ToObservableCollection();
    }

    [RelayCommand]
    private void AddPlant()
    {
        if (IsBusy)
            return;

        try
        {
            _navigationService.GoToAddPlant();
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

    async void IRecipient<PlantAddedOrChangedMessage>.Receive(PlantAddedOrChangedMessage message)
    {
        Plants.Clear();

        await GetPlants();
    }

    void IRecipient<PlantDeletedMessage>.Receive(PlantDeletedMessage message)
    {
        PlantListItemViewModel? deletedPlant = Plants.FirstOrDefault(e => e.Id == message.PlantId);
        if (deletedPlant != null)
        {
            Plants.Remove(deletedPlant);
        }
    }
}