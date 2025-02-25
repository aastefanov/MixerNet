using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace MixerNet.Avalonia.Converters;

public class GainColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not float gain) return new BindingNotification(new InvalidCastException());

        return gain switch
        {
            < -40 => Colors.Yellow,
            < -10 => Colors.Green,
            _ => Colors.Red
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}