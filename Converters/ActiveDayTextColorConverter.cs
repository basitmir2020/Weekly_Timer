using System.Globalization;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Converters;

public class ActiveDayTextColorConverter : IValueConverter
{
    /// <summary>
    /// Maps day abbreviation text to active/inactive text color by comparing against current active day.
    /// </summary>
    /// <param name="value">Bound day abbreviation.</param>
    /// <param name="targetType">Requested target type.</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">Culture info for conversion.</param>
    /// <returns>Heading color when active; muted color otherwise.</returns>
    /// <remarks>
    /// Side effects: reads current main page binding context to resolve active day state.
    /// </remarks>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string dayAbbreviation)
        {
            // We need to resolve App.Current.MainPage's BindingContext to compare against ActiveDay
            if (Application.Current?.MainPage is NavigationPage navPage && navPage.CurrentPage.BindingContext is MainViewModel vm)
            {
                if (vm.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.FromArgb("#f1f5f9"); // TextHeading
                }
            }
            else if (Application.Current?.MainPage is Shell shell && shell.CurrentPage?.BindingContext is MainViewModel vm2)
            {
                 if (vm2.ActiveDay.StartsWith(dayAbbreviation, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.FromArgb("#f1f5f9"); 
                }
            }
        }
        return Color.FromArgb("#334155"); // TextMuted
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
