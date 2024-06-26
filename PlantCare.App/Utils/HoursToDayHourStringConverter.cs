﻿using System.Globalization;

namespace PlantCare.App.Utils;

public class HoursToDayHourStringConverter : IValueConverter
{
    object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double hours)
        {
            int days = ((int)hours) / 24;
            double remainHours = hours - days * 24;

            return $"{days} {LocalizationManager.Instance["Days"]} {remainHours:0.00} {LocalizationManager.Instance["Hours"]}";
        }
        return string.Empty;
    }

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}