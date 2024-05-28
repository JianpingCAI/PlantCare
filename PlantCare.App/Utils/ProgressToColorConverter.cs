using System.Globalization;

namespace PlantCare.App.Utils;

public class ProgressToColorConverter : IValueConverter
{
    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            if (progress < 0.2)
                return Colors.Red;
            else if (progress < 0.5)
                return Colors.Yellow;
            else
                return Colors.DeepSkyBlue;
        }
        return Colors.Transparent;
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}