using CommunityToolkit.Mvvm.ComponentModel;

namespace PlantCare.App.ViewModels
{
    public enum ReminderType
    {
        Watering,
        Fertilization
    }

    public static class ReminderTypeExtension
    {
        public static string GetActionName(this ReminderType reminderType)
        {
            string actionName = string.Empty;
            switch (reminderType)
            {
                case ReminderType.Watering:
                    {
                        actionName = "Water";
                    }
                    break;

                case ReminderType.Fertilization:
                    {
                        actionName = "Fertilize";
                    }
                    break;

                default:
                    break;
            }

            return actionName;
        }
    }

    public partial class ReminderItemViewModel(ReminderType reminderType, Guid plantId) : ObservableObject
    {
        private readonly ReminderType _reminderType = reminderType;
        private readonly Guid _plantId = plantId;

        [ObservableProperty]
        private bool _isSelected = false;

        [ObservableProperty]
        private string _photoPath = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private DateTime _expectedTime = DateTime.MinValue;

        public Guid PlantId => _plantId;

        internal ReminderType ReminderType => _reminderType;
    }
}