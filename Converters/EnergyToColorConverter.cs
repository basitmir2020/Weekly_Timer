using System.Globalization;

namespace WeeklyTimetable.Converters;

public class EnergyToColorConverter : IValueConverter
{
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
