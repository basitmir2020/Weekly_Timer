using System.Globalization;

namespace WeeklyTimetable.Converters;

public class BoolToOpacityConverter : IValueConverter
{
    /// <summary>
    /// Converts completion state to opacity used for dimming completed UI elements.
    /// </summary>
    /// <param name="value">Completion flag value.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns><c>0.45</c> for completed items; otherwise <c>1.0</c>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isCompleted && isCompleted)
        {
            return 0.45;
        }
        return 1.0;
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
