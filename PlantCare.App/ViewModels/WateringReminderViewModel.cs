using CommunityToolkit.Mvvm.ComponentModel;

namespace PlantCare.App.ViewModels
{
    public enum ReminderType
    {
        Watering,
        Fertilization
    }

    public partial class ReminderItemViewModel : ObservableObject
    {
        private readonly ReminderType _reminderType;
        private readonly Guid _plantId;

        public ReminderItemViewModel(ReminderType reminderType, Guid plantId)
        {
            _reminderType = reminderType;
            _plantId = plantId;
        }

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