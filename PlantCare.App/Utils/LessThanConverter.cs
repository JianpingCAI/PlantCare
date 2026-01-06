using System.Globalization;

namespace PlantCare.App.Utils;

public class LessThanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue && parameter is string paramString && double.TryParse(paramString, out double threshold))
        {
            return doubleValue < threshold;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
