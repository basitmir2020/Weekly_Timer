using System.Globalization;
using WeeklyTimetable.Models;

namespace WeeklyTimetable.Converters;

public class EnumToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GoalPriority priority)
        {
            return priority switch
            {
                GoalPriority.High => Color.FromArgb("#FF5252"), // Red
                GoalPriority.Medium => Color.FromArgb("#FFAB40"), // Orange
                GoalPriority.Low => Color.FromArgb("#4CAF50"), // Green
                _ => Colors.Gray
            };
        }
        
        if (value is GoalStatus status)
        {
            return status switch
            {
                GoalStatus.Completed => Color.FromArgb("#4CAF50"),
                GoalStatus.InProgress => Color.FromArgb("#2196F3"),
                GoalStatus.Blocked => Color.FromArgb("#F44336"),
                GoalStatus.Deferred => Color.FromArgb("#9C27B0"),
                GoalStatus.Dropped => Color.FromArgb("#757575"),
                _ => Color.FromArgb("#9E9E9E")
            };
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
