using PlantCare.App.ViewModels;

namespace PlantCare.App.Messaging
{
    internal class PlantEventStatusChangedMessage(Guid plantId, ReminderType reminderType, DateTime updatedTime)
    {
        public Guid PlantId { get; internal set; } = plantId;
        public DateTime UpdatedTime { get; internal set; } = updatedTime;
        public ReminderType ReminderType { get; internal set; } = reminderType;
    }
}