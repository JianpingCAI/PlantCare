using CommunityToolkit.Mvvm.ComponentModel;
using PlantCare.App.Services.DBService;
using PlantCare.App.ViewModels.Base;
using PlantCare.App.Views;
using PlantCare.Data.DbModels;

namespace PlantCare.App.ViewModels;

public class PlantCareHistory
{
    public Guid PlantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<DateTime> CareTimes { get; set; } = [];
}

public partial class WateringHistoryViewModel : ViewModelBase
{
    private readonly IPlantService plantService;

    public WateringHistoryViewModel(IPlantService plantService)
    {
        this.plantService = plantService;
    }

    [ObservableProperty]
    private List<PlantCareHistory> _careHistory = [];

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        CareHistory = await LoadWateringHistoryAsync();
    }

    private Task<List<PlantCareHistory>> LoadWateringHistoryAsync()
    {
        return Task.Run(async () =>
        {
            List<PlantCareHistory> plantCareHistoryList = await plantService.GetAllPlantsWithWateringHistoryAsync();

            return plantCareHistoryList;
        });
    }
}