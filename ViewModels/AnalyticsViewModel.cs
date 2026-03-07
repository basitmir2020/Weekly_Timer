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

    /// <summary>
    /// Creates the analytics view model and starts initial data hydration for charts and streak metrics.
    /// </summary>
    /// <param name="streakService">Service that provides streak history and aggregates.</param>
    /// <param name="databaseService">Service that provides check-ins and weekly goal records.</param>
    /// <remarks>
    /// Side effects: immediately starts asynchronous loading via <see cref="LoadAsync"/>.
    /// </remarks>
    public AnalyticsViewModel(IStreakService streakService, IDatabaseService databaseService)
    {
        _streakService  = streakService;
        _databaseService = databaseService;
        _ = LoadAsync();
    }

    /// <summary>
    /// Loads analytics data for streak cards, heatmap, mood trend, and weekly goal summary.
    /// </summary>
    /// <returns>A task that completes when all analytics collections are populated.</returns>
    /// <remarks>
    /// Side effects: clears and repopulates observable collections and toggles <see cref="IsLoading"/>.
    /// </remarks>
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
                // Fill a 12x7 grid from oldest to newest so UI columns map to weeks left-to-right.
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
                // Preserve query order (descending date) so the UI timeline remains consistent.
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

    /// <summary>
    /// Loads this week's saved goals and computes display-friendly completion and countdown text.
    /// </summary>
    /// <returns>A task that completes when weekly goal UI state is populated.</returns>
    /// <remarks>
    /// Side effects: mutates weekly goal collections and summary flags/text fields.
    /// </remarks>
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

    /// <summary>
    /// Adds a goal item to the weekly tracker only when the goal text has meaningful content.
    /// </summary>
    /// <param name="title">Category title shown in the analytics panel.</param>
    /// <param name="goalText">Raw goal text from persistence.</param>
    /// <param name="isDone">Completion flag for the goal.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: appends a new <see cref="GoalTrackerItem"/> to <see cref="WeeklyGoalItems"/>.
    /// </remarks>
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

    /// <summary>
    /// Calculates the Monday date for the week containing the provided date.
    /// </summary>
    /// <param name="date">Any date within the target week.</param>
    /// <returns>The Monday (date-only) for that week.</returns>
    private static DateTime GetMonday(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// Computes remaining days in the current week, clamping negative values to zero.
    /// </summary>
    /// <param name="date">Reference date used for the countdown.</param>
    /// <returns>Number of days left including the current date boundary behavior.</returns>
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
