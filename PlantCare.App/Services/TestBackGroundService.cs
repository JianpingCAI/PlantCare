using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

//using BCrypt.Net;

namespace PlantCare.App.Services;

public class TestBackGroundService(ILogger logger) : BackgroundService
{
    private readonly ILogger _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("WateringCheckService running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking watering state.");

            //List<Plant> plants = await _plantService.GetAllPlantsAsync();

            //for (int i = 0; i < plants.Count; i++)
            //{
            //    Plant plant = plants[i];
            //    //var nextWateringTime = plant.LastWatered.AddDays(plant.WateringFrequencyInDays);
            //    //var timeLeft = nextWateringTime - DateTime.Now;

            //    //if (timeLeft <= TimeSpan.Zero)
            //    //{
            //    //    // Notify user to water the plant
            //    //    _logger.LogInformation($"Time to water the plant {plant.Name}.");
            //    //    // Add notification logic here
            //    //}

            //    //await ScheduleWateringNotification(plant, i);
            //}

            // Delay for a specific interval before checking again
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _logger.LogInformation("WateringCheckService stopped.");
    }
}