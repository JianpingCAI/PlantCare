using PlantCare.App.ViewModels;

namespace PlantCare.App.Messaging
{
    internal class IsNotificationEnabledMessage
    {
        public bool IsNotificationEnabled { get; set; }
        public CareType ReminderType { get; set; }
    }
}