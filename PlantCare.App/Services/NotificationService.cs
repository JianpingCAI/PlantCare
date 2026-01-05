using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace PlantCare.App.Services;

/// <summary>
/// Implementation of INotificationService using Plugin.LocalNotification v10.0.0+
/// Provides a wrapper around LocalNotificationCenter with proper lifecycle management
/// </summary>
public class NotificationService : INotificationService, IDisposable
{
    private bool _isSubscribed = false;
    private bool _disposed = false;

    public bool IsSupported
    {
        get
        {
            try
            {
                return LocalNotificationCenter.Current?.IsSupported ?? false;
            }
            catch
            {
                return false;
            }
        }
    }

    public event NotificationActionEventHandler? NotificationActionTapped;

    public NotificationService()
    {
        // Subscribe to notification events from the plugin
        try
        {
            if (IsSupported)
            {
                LocalNotificationCenter.Current.NotificationActionTapped += OnNotificationActionTapped;
                _isSubscribed = true;
            }
        }
        catch
        {
            // Plugin not initialized - silently continue
        }
    }

    public async Task Show(NotificationRequest request)
    {
        if (IsSupported)
        {
            await LocalNotificationCenter.Current.Show(request);
        }
    }

    public void Cancel(params int[] notificationIds)
    {
        if (IsSupported && notificationIds.Length > 0)
        {
            LocalNotificationCenter.Current.Cancel(notificationIds);
        }
    }

    public void CancelAll()
    {
        if (IsSupported)
        {
            LocalNotificationCenter.Current.CancelAll();
        }
    }

    public async Task<IList<NotificationRequest>> GetPendingNotificationList()
    {
        if (!IsSupported)
        {
            return [];
        }

        try
        {
            IList<NotificationRequest> pending = await LocalNotificationCenter.Current.GetPendingNotificationList();
            return pending ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IList<NotificationRequest>> GetDeliveredNotificationList()
    {
        if (!IsSupported)
        {
            return [];
        }

        try
        {
            IList<NotificationRequest> delivered = await LocalNotificationCenter.Current.GetDeliveredNotificationList();
            return delivered ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> AreNotificationsEnabled()
    {
        if (!IsSupported)
        {
            return false;
        }

        try
        {
            return await LocalNotificationCenter.Current.AreNotificationsEnabled();
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RequestNotificationPermission()
    {
        if (!IsSupported)
        {
            return false;
        }

        try
        {
            return await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RequestNotificationPermission(NotificationPermission permission)
    {
        if (!IsSupported)
        {
            return false;
        }

        try
        {
            return await LocalNotificationCenter.Current.RequestNotificationPermission(permission);
        }
        catch
        {
            return false;
        }
    }

    private void OnNotificationActionTapped(NotificationActionEventArgs e)
    {
        NotificationActionTapped?.Invoke(e);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Unsubscribe from plugin events to prevent memory leaks
        try
        {
            if (_isSubscribed && IsSupported)
            {
                LocalNotificationCenter.Current.NotificationActionTapped -= OnNotificationActionTapped;
                _isSubscribed = false;
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }

        // Clear event subscribers
        NotificationActionTapped = null;
    }
}
