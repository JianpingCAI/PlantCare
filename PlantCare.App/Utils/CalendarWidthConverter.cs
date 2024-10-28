using System.Globalization;

namespace PlantCare.App.Utils;

public class CalendarWidthConverter : IValueConverter
{
    public object Convert(object? isShow, Type targetType, object? parameter, CultureInfo culture)
    {
        // Your condition here
        bool isConditionMet = (isShow as bool?) ?? false; 
        return isConditionMet ? ConstantValues.CalendarWidth : 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CalendarHeightConverter : IValueConverter
{
    public object Convert(object? isShow, Type targetType, object? parameter, CultureInfo culture)
    {
        // Your condition here
        bool isConditionMet = (isShow as bool?) ?? false;
        return isConditionMet ? ConstantValues.CalendarHeight : 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
