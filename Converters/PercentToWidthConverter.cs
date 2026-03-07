using System.Globalization;

namespace WeeklyTimetable.Converters;

public class PercentToWidthConverter : IValueConverter
{
    /// <summary>
    /// Converts an integer percentage into a width value bounded by configured maximum width.
    /// </summary>
    /// <param name="value">Percentage value as integer.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Scaled width value.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double maxW = 35.0; // Based on the UI WidthRequest=35
        if (value is int pct)
        {
            return (pct / 100.0) * maxW;
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
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
