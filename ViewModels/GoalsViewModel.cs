using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class GoalsViewModel : ObservableObject
{
    private readonly IDatabaseService _databaseService;

    [ObservableProperty] private string _dsaTopic = string.Empty;
    [ObservableProperty] private string _webDevFeature = string.Empty;
    [ObservableProperty] private string _habitFocus = string.Empty;
    [ObservableProperty] private bool _dsaDone;
    [ObservableProperty] private bool _webDevDone;
    [ObservableProperty] private bool _habitDone;

    private string WeekStart => GetMonday(DateTime.Today).ToString("yyyy-MM-dd");

    /// <summary>
    /// Creates the weekly goals view model and loads persisted goal values for the current week.
    /// </summary>
    /// <param name="databaseService">Database service used to read and save weekly goals.</param>
    /// <remarks>
    /// Side effects: starts asynchronous loading of current week goal data.
    /// </remarks>
    public GoalsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        _ = LoadAsync();
    }

    /// <summary>
    /// Loads the current week's goal record and maps persisted values into bindable properties.
    /// </summary>
    /// <returns>A task that completes after the weekly goal has been loaded.</returns>
    /// <remarks>
    /// Side effects: updates goal text and completion flag properties.
    /// </remarks>
    private async Task LoadAsync()
    {
        try
        {
            var goal = await _databaseService.GetWeeklyGoalAsync(WeekStart);
            if (goal != null)
            {
                DsaTopic       = goal.DSATopic ?? string.Empty;
                WebDevFeature  = goal.WebDevFeature ?? string.Empty;
                HabitFocus     = goal.HabitFocus ?? string.Empty;
                DsaDone        = goal.DSADone;
                WebDevDone     = goal.WebDevDone;
                HabitDone      = goal.HabitDone;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading goals: {ex.Message}");
        }
    }

    /// <summary>
    /// Persists current in-memory goal values for the active week.
    /// </summary>
    /// <returns>A task that completes when the goal record is saved.</returns>
    /// <remarks>
    /// Side effects: writes weekly goal data to persistent storage.
    /// </remarks>
    [RelayCommand]
    private async Task SaveGoalsAsync()
    {
        var goal = new WeeklyGoal
        {
            WeekStartDate = WeekStart,
            DSATopic      = DsaTopic,
            WebDevFeature = WebDevFeature,
            HabitFocus    = HabitFocus,
            DSADone       = DsaDone,
            WebDevDone    = WebDevDone,
            HabitDone     = HabitDone
        };
        await _databaseService.SaveWeeklyGoalAsync(goal);
    }

    /// <summary>
    /// Toggles DSA completion and persists the weekly goal record.
    /// </summary>
    /// <returns>A task that completes when persistence is done.</returns>
    [RelayCommand] private async Task ToggleDsa()  { DsaDone = !DsaDone;     await SaveGoalsAsync(); }
    /// <summary>
    /// Toggles Web Dev completion and persists the weekly goal record.
    /// </summary>
    /// <returns>A task that completes when persistence is done.</returns>
    [RelayCommand] private async Task ToggleWebDev(){ WebDevDone = !WebDevDone; await SaveGoalsAsync(); }
    /// <summary>
    /// Toggles habit completion and persists the weekly goal record.
    /// </summary>
    /// <returns>A task that completes when persistence is done.</returns>
    [RelayCommand] private async Task ToggleHabit() { HabitDone = !HabitDone;  await SaveGoalsAsync(); }

    /// <summary>
    /// Calculates Monday for the week that contains the provided date.
    /// </summary>
    /// <param name="date">Date used to locate the target week.</param>
    /// <returns>Date-only Monday value for the same week.</returns>
    private static DateTime GetMonday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
