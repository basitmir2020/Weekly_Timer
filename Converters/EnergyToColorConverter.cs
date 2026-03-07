using System.Globalization;

namespace WeeklyTimetable.Converters;

public class EnergyToColorConverter : IValueConverter
{
    /// <summary>
    /// Maps numeric energy level to a semantic color used in charts and badges.
    /// </summary>
    /// <param name="value">Energy level value.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Color corresponding to the energy level, or fallback muted color.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int level)
        {
            return level switch
            {
                1 => Color.FromArgb("#ef4444"), // Red — very low
                2 => Color.FromArgb("#f97316"), // Orange — low
                3 => Color.FromArgb("#f59e0b"), // Amber — neutral
                4 => Color.FromArgb("#22c55e"), // Green — good
                5 => Color.FromArgb("#818cf8"), // Purple — excellent
                _ => Color.FromArgb("#334155")
            };
        }
        return Color.FromArgb("#334155");
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
        => throw new NotImplementedException();
}
