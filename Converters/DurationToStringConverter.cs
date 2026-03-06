using System.Globalization;

namespace WeeklyTimetable.Converters;

public class DurationToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int minutes && minutes > 0)
        {
            int h = minutes / 60;
            int m = minutes % 60;
            if (h > 0 && m > 0) return $"{h}h {m}m";
            if (h > 0) return $"{h}h";
            return $"{m}m";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
