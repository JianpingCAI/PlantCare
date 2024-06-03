
using PlantCare.App.ViewModels;

namespace PlantCare.App.Messaging
{
    internal class PlantEventStatusChangedMessage
    {
        public PlantEventStatusChangedMessage()
        {
        }

        public Guid PlantId { get; internal set; }
        public DateTime UpdatedTime { get; internal set; }
        public ReminderType ReminderType { get; internal set; }
    }
}