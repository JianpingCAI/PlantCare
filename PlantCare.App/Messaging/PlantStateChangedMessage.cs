using PlantCare.App.ViewModels;

namespace PlantCare.App.Messaging
{
    internal class PlantStateChangedMessage(Guid plantId, CareType reminderType, DateTime updatedTime)
    {
        public Guid PlantId { get; internal set; } = plantId;
        public DateTime UpdatedTime { get; internal set; } = updatedTime;
        public CareType ReminderType { get; internal set; } = reminderType;
    }
}