using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace PlantCare.App.Utils;

public class EnumDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return GetDescription(enumValue);
        }
        return null;
    }

    private static string GetDescription(Enum value)
    {
        FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[]?)fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes?.FirstOrDefault()?.Description ?? value.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}