using System.Globalization;

namespace WeeklyTimetable.Converters;

public class Time24To12HrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string time24)
        {
            if (DateTime.TryParseExact(time24, "HH:mm", null, DateTimeStyles.None, out DateTime dt))
                return dt.ToString("h:mm tt");
            if (DateTime.TryParseExact(time24, "H:mm", null, DateTimeStyles.None, out DateTime dt2))
                return dt2.ToString("h:mm tt");
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
