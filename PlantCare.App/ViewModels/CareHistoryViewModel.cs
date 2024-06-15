using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace PlantCare.App.ViewModels;

public record class TimeStampRecord(DateTime Timestamp, Guid PlantId, Guid HistoryId);

public class PlantCareHistory
{
    public Guid PlantId { get; internal set; }

    public string Name { get; internal set; } = string.Empty;

    public List<TimeStampRecord> WateringTimestamps { get; internal set; } = [];
    public List<TimeStampRecord> FertilizationTimestamps { get; internal set; } = [];

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
    private readonly INavigationService _navigationService;

    public CareHistoryViewModel(
        IPlantService plantService,
        IDialogService dialogService,
        INavigationService navigationService)
    {
        _plantService = plantService;
        _dialogService = dialogService;
        _navigationService = navigationService;
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
                        DateTime currentTimestamp = careHistory.WateringTimestamps[i].Timestamp;
                        double interval = i != 0 ? currentTimestamp.Subtract(careHistory.WateringTimestamps[i - 1].Timestamp).TotalDays : 1.0;
                        wateringDatePoints.Add(new DateTimePoint(currentTimestamp, Math.Round(interval, 1)));
                    }

                    List<DateTimePoint> fertilizationDatePoints = new(careHistory.FertilizationTimestamps.Count);
                    for (int i = 0; i < careHistory.FertilizationTimestamps.Count; i++)
                    {
                        DateTime currentTimestamp = careHistory.FertilizationTimestamps[i].Timestamp;
                        double interval = i != 0 ? currentTimestamp.Subtract(careHistory.FertilizationTimestamps[i - 1].Timestamp).TotalDays : 1.0;
                        fertilizationDatePoints.Add(new DateTimePoint(currentTimestamp, Math.Round(interval, 1)));
                    }

                    careHistoryWithPlots.Add(new PlantCareHistoryWithPlot
                    {
                        PlantId = careHistory.PlantId,
                        Name = careHistory.Name,
                        PhotoPath = careHistory.PhotoPath,

                        WateringTimestamps = careHistory.WateringTimestamps,
                        WateringFrequencyInfo = GetFrequencyInfo(careHistory.WateringTimestamps.Select(x => x.Timestamp).ToList()),

                        FertilizationTimestamps = careHistory.FertilizationTimestamps,
                        FertilizationFrequencyInfo = GetFrequencyInfo(careHistory.FertilizationTimestamps.Select(x => x.Timestamp).ToList()),

                        WateringTimestampsSeries = [new ColumnSeries<DateTimePoint> { Values = wateringDatePoints, Fill = new SolidColorPaint(SKColors.DeepSkyBlue) }],
                        FertilizationTimestampsSeries = [new ColumnSeries<DateTimePoint> { Values = fertilizationDatePoints, Fill = new SolidColorPaint(SKColors.DeepSkyBlue),
            Stroke = new SolidColorPaint(SKColors.DeepSkyBlue, 1)}],

                        XAxesWatering = [new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM d")) { TextSize = 8 }],
                        XAxesFertilization = [new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("MMM d")) { TextSize = 8 }],
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

    #region Select Related

    [ObservableProperty]
    private bool _isWateringHistory = true;

    [ObservableProperty]
    private bool _isFertilizationHistory = false;

    [ObservableProperty]
    private PlantCareHistoryWithPlot? _selectedPlant = null;

    [RelayCommand]
    public async Task SelectPlant()
    {
        if (IsBusy) return;

        IsBusy = true;

        try
        {
            if (SelectedPlant != null)
            {
                List<TimeStampRecord> timestampRecords = IsWateringHistory ? SelectedPlant.WateringTimestamps : SelectedPlant.FertilizationTimestamps;

                await _navigationService.GoToCareHistory(SelectedPlant.Name, IsWateringHistory ? CareType.Watering : CareType.Fertilization, timestampRecords);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion Select Related
}