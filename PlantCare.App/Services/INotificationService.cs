using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace PlantCare.App.Services;

/// <summary>
/// Delegate for notification action events that matches Plugin.LocalNotification's event signature
/// </summary>
public delegate void NotificationActionEventHandler(NotificationActionEventArgs e);

/// <summary>
/// Service for managing local notifications (wrapper around Plugin.LocalNotification)
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Gets a value indicating whether notifications are supported on the current platform
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Sends/schedules a local notification
    /// </summary>
    Task Show(NotificationRequest request);

    /// <summary>
    /// Cancels pending notifications by their IDs
    /// </summary>
    void Cancel(params int[] notificationIds);

    /// <summary>
    /// Cancels all pending notifications
    /// </summary>
    void CancelAll();

    /// <summary>
    /// Gets a list of pending notifications
    /// </summary>
    Task<IList<NotificationRequest>> GetPendingNotificationList();

    /// <summary>
    /// Gets a list of delivered notifications
    /// </summary>
    Task<IList<NotificationRequest>> GetDeliveredNotificationList();

    /// <summary>
    /// Checks if notifications are enabled/permitted
    /// </summary>
    Task<bool> AreNotificationsEnabled();

    /// <summary>
    /// Requests notification permission from the user with advanced options
    /// </summary>
    Task<bool> RequestNotificationPermission();

    /// <summary>
    /// Requests notification permission with iOS and Android specific options
    /// </summary>
    Task<bool> RequestNotificationPermission(NotificationPermission permission);

    /// <summary>
    /// Event raised when a notification action is tapped or dismissed
    /// </summary>
    event NotificationActionEventHandler? NotificationActionTapped;
}
