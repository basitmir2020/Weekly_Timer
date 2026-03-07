using WeeklyTimetable.Models;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

[QueryProperty(nameof(DayName), "DayName")]
public partial class EditBlockPage : ContentPage
{
    public string DayName { get; set; } = "Monday";

    /// <summary>
    /// Initializes the edit block page.
    /// </summary>
    public EditBlockPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Lazily creates and assigns an <see cref="EditBlockViewModel"/> for the requested block/day context.
    /// </summary>
    /// <param name="existing">Existing block when editing, or <c>null</c> for create flow.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: sets page <see cref="BindingContext"/> when one is not already assigned.
    /// </remarks>
    public void LoadBlock(ScheduleBlock? existing)
    {
        if (BindingContext is not EditBlockViewModel)
        {
            var vm = new EditBlockViewModel(existing);
            BindingContext = vm;
        }
    }
}
