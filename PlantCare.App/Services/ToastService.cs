using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace PlantCare.App.Services;

public class ToastService : IToastService
{
    public Task ShowAsync(string message, ToastType type = ToastType.Info, int durationMs = 3000)
    {
        ToastDuration duration = durationMs <= 2000 ? ToastDuration.Short : ToastDuration.Long;
        var toast = Toast.Make(message, duration);
        return toast.Show();
    }
}
