namespace PlantCare.App.Services;
using Microsoft.Maui.Devices;

public class DeviceService
{
    public static bool IsLocalNotificationSupported()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
        {
            return true;
        }

        return false;
    }
}