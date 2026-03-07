using System.Globalization;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Converters;

public class ActiveDayOpacityConverter : IValueConverter
{
    /// <summary>
    /// Returns full opacity when the bound day abbreviation matches the active day; otherwise returns zero opacity.
    /// </summary>
    /// <param name="value">Bound day abbreviation text.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns><c>1.0</c> when active; otherwise <c>0.0</c>.</returns>
    /// <remarks>
    /// Side effects: reads current main page binding context to resolve active day.
    /// </remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string dayAbbreviation)
        {
            if (Application.Current?.MainPage is NavigationPage navPage && navPage.CurrentPage.BindingContext is MainViewModel vm)
            {
                if (vm.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                    return 1.0;
            }
            else if (Application.Current?.MainPage is Shell shell && shell.CurrentPage?.BindingContext is MainViewModel vm2)
            {
                 if (vm2.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                    return 1.0;
            }
        }
        return 0.0;
    }

    /// <summary>
    /// Reverse conversion is not supported for this one-way UI converter.
    /// </summary>
    /// <param name="value">Source value.</param>
    /// <param name="targetType">Target type.</param>
    /// <param name="parameter">Optional parameter.</param>
    /// <param name="culture">Culture info.</param>
    /// <returns>Never returns; always throws.</returns>
    /// <exception cref="NotImplementedException">Always thrown because converter is one-way only.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
