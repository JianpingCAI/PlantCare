//using PlantCare.App.Services;
//using Android.Content;
//using AndroidX.Work;

//public class WateringWorker : Worker
//{
//    private readonly IPlantService _plantService;
//    private readonly NotificationService _notificationService;

//    public WateringWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
//    {
//        var serviceProvider = MauiApplication.Current.Services;
//        _plantService = serviceProvider.GetRequiredService<IPlantService>();
//        _notificationService = serviceProvider.GetRequiredService<NotificationService>();
//    }

//    public override Result DoWork()
//    {
//        CheckAndNotifyWatering();
//        return Result.InvokeSuccess();
//    }

//    private async void CheckAndNotifyWatering()
//    {
//        var plants = await _plantService.GetAllPlantsAsync();
//        foreach (var plant in plants)
//        {
//            if ((plant.NextWateringDate - DateTime.Now).Days <= 0)
//            {
//                _notificationService.ScheduleNotification(plant);
//            }
//        }
//    }
//}