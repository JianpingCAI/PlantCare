using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using PlantCare.App.Services.DBService;
using PlantCare.App.ViewModels.Base;

namespace PlantCare.App.ViewModels;

public class PlantCareHistory
{
    public Guid PlantId { get; internal set; }

    public string Name { get; internal set; } = string.Empty;

    public List<DateTime> WateringTimestamps { get; internal set; } = [];
    public List<DateTime> FertilizationTimestamps { get; internal set; } = [];

    public string? PhotoPath { get; internal set; }
}

public class PlantCareHistoryWithPlot : PlantCareHistory
{
    public ISeries[] WateringTimestampsSeries { get; set; } = [];
    public ISeries[] FertilizationTimestampsSeries { get; set; } = [];

    public Axis[]? XAxesWatering { get; set; }
    public Axis[]? XAxesFertilization { get; set; }
}

public partial class CareHistoryViewModel : ViewModelBase
{
    private readonly IPlantService plantService;

    public CareHistoryViewModel(IPlantService plantService)
    {
        this.plantService = plantService;
    }

    [ObservableProperty]
    private List<PlantCareHistoryWithPlot> _careHistory = [];

    public override Task LoadDataWhenViewAppearingAsync()
    {
        return Task.Run(async () =>
        {
            CareHistory.Clear();

            List<PlantCareHistory> careHistoryList = await LoadWateringHistoryAsync();

            Axis[] xAxes1 = [new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM d")) { TextSize = 8 }];
            Axis[] xAxes2 = [new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM d")) { TextSize = 8 }];

            List<PlantCareHistoryWithPlot> careHistoryWithPlots = new(careHistoryList.Count);
            foreach (PlantCareHistory careHistory in careHistoryList)
            {
                List<DateTimePoint> wateringDatePoints = new(careHistory.WateringTimestamps.Count);
                for (int i = 0; i < careHistory.WateringTimestamps.Count; i++)
                {
                    DateTime currentTimestamp = careHistory.WateringTimestamps[i];
                    int interval = i != 0 ? currentTimestamp.Subtract(careHistory.WateringTimestamps[i - 1]).Days : 3;
                    wateringDatePoints.Add(new DateTimePoint(currentTimestamp, interval));
                }

                List<DateTimePoint> fertilizationDatePoints = new(careHistory.FertilizationTimestamps.Count);
                for (int i = 0; i < careHistory.FertilizationTimestamps.Count; i++)
                {
                    DateTime currentTimestamp = careHistory.FertilizationTimestamps[i];
                    int interval = i != 0 ? currentTimestamp.Subtract(careHistory.FertilizationTimestamps[i - 1]).Days : 3;
                    fertilizationDatePoints.Add(new DateTimePoint(currentTimestamp, interval));
                }

                careHistoryWithPlots.Add(new PlantCareHistoryWithPlot
                {
                    PlantId = careHistory.PlantId,
                    Name = careHistory.Name,
                    PhotoPath = careHistory.PhotoPath,
                    WateringTimestamps = careHistory.WateringTimestamps,
                    WateringTimestampsSeries = [new ColumnSeries<DateTimePoint> { Values = wateringDatePoints }],
                    FertilizationTimestampsSeries = [new ColumnSeries<DateTimePoint> { Values = fertilizationDatePoints }],
                    XAxesWatering = xAxes1,
                    XAxesFertilization = xAxes2
                });
            }

            CareHistory = careHistoryWithPlots;
        });
    }

    private Task<List<PlantCareHistory>> LoadWateringHistoryAsync()
    {
        return Task.Run(async () =>
        {
            List<PlantCareHistory> plantCareHistoryList = await plantService.GetAllPlantsWithCareHistoryAsync();

            return plantCareHistoryList;
        });
    }
}