using System.Globalization;

namespace Construction.Maui.Converters;

/// <summary>Converts a null/non-null value to bool (default: true when not null).</summary>
public sealed class NullToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter as string == "invert";
        var notNull = value is not null;
        return invert ? !notNull : notNull;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Converts a string equality to bool (e.g. tab active state).</summary>
public sealed class StringEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var a = value as string;
        var b = parameter as string;
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
