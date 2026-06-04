using System.Globalization;
using System.Windows.Data;

namespace Dot9.Converters;

/// <summary>Scales a 0..1 fraction (source) to a 0..100 percentage (target) and back.</summary>
public sealed class PercentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? d * 100.0 : 0.0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? d / 100.0 : 0.0;
}

/// <summary>
/// Two-way converter for binding a segmented RadioButton group to an enum property.
/// ConverterParameter names the enum member this option represents.
/// </summary>
public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null && parameter is not null
            && string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true && parameter is not null
            ? Enum.Parse(targetType, parameter.ToString()!)
            : System.Windows.Data.Binding.DoNothing;
}

/// <summary>Maps an enabled flag to a section opacity (enabled = 1.0, disabled = dimmed).</summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public double DisabledOpacity { get; set; } = 0.45;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? 1.0 : DisabledOpacity;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
