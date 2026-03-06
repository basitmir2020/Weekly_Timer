using WeeklyTimetable.Models;
using WeeklyTimetable.Services;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

[QueryProperty(nameof(DayName), "DayName")]
public partial class EditBlockPage : ContentPage
{
    public string DayName { get; set; } = "Monday";

    private readonly IDatabaseService _db;

    public EditBlockPage(IDatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    public void LoadBlock(ScheduleBlock? existing, string dayName)
    {
        if (BindingContext is not EditBlockViewModel)
        {
            var vm = new EditBlockViewModel(_db, dayName, existing);
            BindingContext = vm;
        }
    }
}
