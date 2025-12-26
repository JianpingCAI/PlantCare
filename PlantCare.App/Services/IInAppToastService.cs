namespace PlantCare.App.Services;

/// <summary>
/// Service for displaying in-app toast notifications with different styles.
/// </summary>
public interface IInAppToastService
{
    /// <summary>
    /// Shows an info toast message.
    /// </summary>
    Task ShowInfoAsync(string message, int durationMs = 3000);

    /// <summary>
    /// Shows a success toast message.
    /// </summary>
    Task ShowSuccessAsync(string message, int durationMs = 3000);

    /// <summary>
    /// Shows a warning toast message.
    /// </summary>
    Task ShowWarningAsync(string message, int durationMs = 3000);

    /// <summary>
    /// Shows an error toast message.
    /// </summary>
    Task ShowErrorAsync(string message, int durationMs = 4000);

    /// <summary>
    /// Shows a toast message with specified type.
    /// </summary>
    Task ShowAsync(string message, ToastType type, int durationMs = 3000);

    /// <summary>
    /// Dismisses any currently showing toast.
    /// </summary>
    Task DismissAsync();
}
