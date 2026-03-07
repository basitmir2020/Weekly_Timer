using System.Globalization;

namespace WeeklyTimetable.Converters;

public class PercentToArcConverter : IValueConverter
{
    /// <summary>
    /// Converts percentage input to arc degrees for circular progress rendering.
    /// </summary>
    /// <param name="value">Percentage value as <see cref="double"/> or <see cref="int"/>.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Equivalent degree sweep in range 0..360 when valid.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double pct)
        {
            return (pct / 100) * 360;
        }
        else if (value is int pctInt)
        {
             return (pctInt / 100.0) * 360;
        }
        return 0.0;
    }

    /// <summary>
    /// Reverse conversion is not supported.
    /// </summary>
    /// <param name="value">Source value.</param>
    /// <param name="targetType">Target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">Culture info.</param>
    /// <returns>Never returns; always throws.</returns>
    /// <exception cref="NotImplementedException">Always thrown for one-way conversion.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
