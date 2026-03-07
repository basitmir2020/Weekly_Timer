using WeeklyTimetable.Models;
using WeeklyTimetable.Services;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

[QueryProperty(nameof(DayName), "DayName")]
public partial class EditBlockPage : ContentPage
{
    public string DayName { get; set; } = "Monday";

    private readonly IDatabaseService _db;

    /// <summary>
    /// Initializes the edit block page with required database dependency.
    /// </summary>
    /// <param name="db">Database service used by edit block view model.</param>
    public EditBlockPage(IDatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    /// <summary>
    /// Lazily creates and assigns an <see cref="EditBlockViewModel"/> for the requested block/day context.
    /// </summary>
    /// <param name="existing">Existing block when editing, or <c>null</c> for create flow.</param>
    /// <param name="dayName">Day name for the block.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: sets page <see cref="BindingContext"/> when one is not already assigned.
    /// </remarks>
    public void LoadBlock(ScheduleBlock? existing, string dayName)
    {
        if (BindingContext is not EditBlockViewModel)
        {
            var vm = new EditBlockViewModel(_db, dayName, existing);
            BindingContext = vm;
        }
    }
}
