namespace PlantCare.App.Services;

public interface IToastService
{
    Task ShowAsync(string message, ToastType type = ToastType.Info, int durationMs = 3000);
}

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}
