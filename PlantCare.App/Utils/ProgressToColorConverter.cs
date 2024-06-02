using System.Globalization;

namespace PlantCare.App.Utils;

public class ProgressToColorConverter : IValueConverter
{
    private static readonly double _oneDay = 1.0 / 7;
    private static readonly double _threeDays = 3.0 / 7;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            if (progress < 0.01)
                return Colors.Red;
            else if (progress <= _oneDay)
                return Colors.Orange;
            else if (progress < _threeDays)
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