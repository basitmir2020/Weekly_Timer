using System.Globalization;
using WeeklyTimetable.Models;

namespace WeeklyTimetable.Converters;

public class GoalStatusToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GoalStatus status)
        {
            return status.ToString().Replace("NotStarted", "Not Started").Replace("InProgress", "In Progress");
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
