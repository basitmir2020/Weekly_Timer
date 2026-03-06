using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IStreakService _streakService;
    private readonly IDatabaseService _databaseService;

    [ObservableProperty] private int _currentStreak;
    [ObservableProperty] private int _longestStreak;
    [ObservableProperty] private int _totalCompleteDays;
    [ObservableProperty] private ObservableCollection<HeatmapCell> _heatmapCells = new();
    [ObservableProperty] private ObservableCollection<MoodEntry> _moodTrend = new();
    [ObservableProperty] private ObservableCollection<CategoryBreakdown> _categoryBreakdown = new();
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private bool _hasWeeklyGoals;
    [ObservableProperty] private string _goalCountdownText = string.Empty;
    [ObservableProperty] private string _goalsProgressText = string.Empty;

    public ObservableCollection<GoalTrackerItem> WeeklyGoalItems { get; } = new();

    public AnalyticsViewModel(IStreakService streakService, IDatabaseService databaseService)
    {
        _streakService  = streakService;
        _databaseService = databaseService;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            CurrentStreak     = await _streakService.GetCurrentStreakAsync();
            LongestStreak     = await _streakService.GetLongestStreakAsync();

            // Build 12-week heatmap
            HeatmapCells.Clear();
            var streakRecords = await _streakService.GetLast30DaysAsync();
            var today = DateTime.Today;
            for (int i = 83; i >= 0; i--)
            {
                var date  = today.AddDays(-i);
                var entry = streakRecords.FirstOrDefault(s => s.Date.Date == date);
                HeatmapCells.Add(new HeatmapCell
                {
                    Date       = date,
                    Pct        = entry?.IsComplete == true ? 100 : 0,
                    CellColor  = entry?.IsComplete == true ? "#22c55e" : "#1e293b"
                });
            }

            // Mood trend (last 14 days)
            MoodTrend.Clear();
            var checkIns = await _databaseService.GetRecentCheckInsAsync(14);
            foreach (var ci in checkIns)
            {
                MoodTrend.Add(new MoodEntry
                {
                    Date   = ci.Date,
                    Energy = ci.MorningEnergy,
                    Mood   = ci.EveningMood
                });
            }

            TotalCompleteDays = streakRecords.Count(s => s.IsComplete);
            
            await LoadWeeklyGoalsAsync();
        }
        finally { IsLoading = false; }
    }

    private async Task LoadWeeklyGoalsAsync()
    {
        WeeklyGoalItems.Clear();
        HasWeeklyGoals = false;
        GoalCountdownText = string.Empty;
        GoalsProgressText = string.Empty;

        var weekStart = GetMonday(DateTime.Today).ToString("yyyy-MM-dd");
        var goal = await _databaseService.GetWeeklyGoalAsync(weekStart);
        if (goal == null)
            return;

        AddGoalIfPresent("DSA Topic", goal.DSATopic, goal.DSADone);
        AddGoalIfPresent("Web Dev", goal.WebDevFeature, goal.WebDevDone);
        AddGoalIfPresent("Habit", goal.HabitFocus, goal.HabitDone);

        if (WeeklyGoalItems.Count == 0)
            return;

        int completed = WeeklyGoalItems.Count(g => g.IsDone);
        int total = WeeklyGoalItems.Count;
        int daysLeft = GetDaysLeftInWeek(DateTime.Today);

        GoalsProgressText = $"{completed}/{total} completed";
        GoalCountdownText = daysLeft == 0
            ? "Last day to complete this week's goals"
            : $"{daysLeft} day{(daysLeft == 1 ? string.Empty : "s")} left this week";
        HasWeeklyGoals = true;
    }

    private void AddGoalIfPresent(string title, string goalText, bool isDone)
    {
        if (string.IsNullOrWhiteSpace(goalText))
            return;

        WeeklyGoalItems.Add(new GoalTrackerItem
        {
            Title = title,
            GoalText = goalText.Trim(),
            IsDone = isDone
        });
    }

    private static DateTime GetMonday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static int GetDaysLeftInWeek(DateTime date)
    {
        var weekEnd = GetMonday(date).AddDays(6);
        var days = (weekEnd.Date - date.Date).Days;
        return days < 0 ? 0 : days;
    }
}

public class HeatmapCell
{
    public DateTime Date { get; set; }
    public int Pct { get; set; }
    public string CellColor { get; set; } = "#1e293b";
    public string DateLabel => Date.ToString("MMM d");
}

public class MoodEntry
{
    public DateTime Date { get; set; }
    public int Energy { get; set; }
    public int Mood { get; set; }
    public string DateLabel => Date.ToString("MMM d");
}

public class CategoryBreakdown
{
    public string Category { get; set; } = string.Empty;
    public int Minutes { get; set; }
    public string Color { get; set; } = "#818cf8";
    public string Label => $"{Category} ({Minutes / 60}h {Minutes % 60}m)";
}

public class GoalTrackerItem
{
    public string Title { get; set; } = string.Empty;
    public string GoalText { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public string StatusText => IsDone ? "✓" : "○";
    public string StatusColor => IsDone ? "#34d399" : "#94a3b8";
}
