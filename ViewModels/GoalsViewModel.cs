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

    public GoalsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        _ = LoadAsync();
    }

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

    [RelayCommand] private async Task ToggleDsa()  { DsaDone = !DsaDone;     await SaveGoalsAsync(); }
    [RelayCommand] private async Task ToggleWebDev(){ WebDevDone = !WebDevDone; await SaveGoalsAsync(); }
    [RelayCommand] private async Task ToggleHabit() { HabitDone = !HabitDone;  await SaveGoalsAsync(); }

    private static DateTime GetMonday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
