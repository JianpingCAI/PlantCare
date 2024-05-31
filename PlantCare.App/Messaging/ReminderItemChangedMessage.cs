
using PlantCare.App.ViewModels;

namespace PlantCare.App.Messaging
{
    internal class ReminderItemChangedMessage
    {
        public ReminderItemChangedMessage()
        {
        }

        public Guid PlantId { get; internal set; }
        public DateTime UpdatedTime { get; internal set; }
        public ReminderType ReminderType { get; internal set; }
    }
}