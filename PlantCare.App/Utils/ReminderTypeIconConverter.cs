using PlantCare.App.ViewModels;
using System.Globalization;

namespace PlantCare.App.Utils;

public class ReminderTypeIconConverter : IValueConverter
{
    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CareType reminderType)
        {
            switch (reminderType)
            {
                case CareType.Watering:
                    return "\ue798";//"&#xe798;"; //water drop icon
                case CareType.Fertilization:
                    return "\ue761"; //"&#xe761;"; //fertilization icon
                default:
                    break;
            }
        }
        return string.Empty;
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue && int.TryParse(stringValue, out int result))
        {
            return result;
        }
        return 0;
    }
}