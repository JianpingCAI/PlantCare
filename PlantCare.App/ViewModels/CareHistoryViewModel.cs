using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

    public string WateringFrequencyInfo { get; internal set; } = string.Empty;
    public string FertilizationFrequencyInfo { get; internal set; } = string.Empty;

    public Axis[]? XAxesWatering { get; set; }
    public Axis[]? XAxesFertilization { get; set; }
}

public partial class CareHistoryViewModel : ViewModelBase
{
    private readonly IPlantService _plantService;
    private readonly IDialogService _dialogService;

    public CareHistoryViewModel(IPlantService plantService, IDialogService dialogService)
    {
        _plantService = plantService;
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private ObservableCollection<PlantCareHistoryWithPlot> _careHistory = [];

    public override async Task LoadDataWhenViewAppearingAsync()
    {
        try
        {
            List<PlantCareHistory> careHistoryList = await _plantService.GetAllPlantsWithCareHistoryAsync();
            List<PlantCareHistoryWithPlot> careHistoryWithPlots = await Task.Run(() =>
            {
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
                        WateringFrequencyInfo = GetFrequencyInfo(careHistory.WateringTimestamps),
                        FertilizationFrequencyInfo = GetFrequencyInfo(careHistory.FertilizationTimestamps),
                        FertilizationTimestampsSeries = [new ColumnSeries<DateTimePoint> { Values = fertilizationDatePoints }],
                        XAxesWatering = [new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM d")) { TextSize = 8 }],
                        XAxesFertilization = [new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM d")) { TextSize = 8 }]
                    });
                }

                return careHistoryWithPlots;
            });

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CareHistory.Clear();

                foreach (var item in careHistoryWithPlots)
                {
                    CareHistory.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
    }

    private string GetFrequencyInfo(List<DateTime> timestamps)
    {
        if (timestamps.Count < 2)
        {
            return string.Empty;
        }

        TimeSpan averageInterval = MathTool.CalculateAverageInterval(timestamps);

        return $"{LocalizationManager.Instance[ConstStrings.Average] ?? ConstStrings.Average}: {averageInterval.Days} {LocalizationManager.Instance[ConstStrings.Days] ?? ConstStrings.Days} {averageInterval.Hours} {LocalizationManager.Instance[ConstStrings.Hours] ?? ConstStrings.Hours}";
    }
}