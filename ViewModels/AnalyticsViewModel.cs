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
    private bool _isLoaded;

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
    [ObservableProperty] private double _goalCompletionPercent;

    [ObservableProperty] private double _executionScore;
    [ObservableProperty] private int _totalPlannedBlocks;
    [ObservableProperty] private int _totalCompletedBlocks;
    [ObservableProperty] private double _consistencyScore;

    [ObservableProperty] private string _topCategory = string.Empty;
    [ObservableProperty] private string _lowCategory = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _recommendations = new();
    [ObservableProperty] private ObservableCollection<InsightItem> _weeklyInsights = new();


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
    }

    public Task EnsureLoadedAsync() => LoadCoreAsync(forceRefresh: false);

    /// <summary>
    /// Loads analytics data for streak cards, heatmap, mood trend, and weekly goal summary.
    /// </summary>
    /// <returns>A task that completes when all analytics collections are populated.</returns>
    /// <remarks>
    /// Side effects: clears and repopulates observable collections and toggles <see cref="IsLoading"/>.
    /// </remarks>
    [RelayCommand]
    private Task LoadAsync()
    {
        return LoadCoreAsync(forceRefresh: true);
    }

    private async Task LoadCoreAsync(bool forceRefresh)
    {
        if (IsLoading)
            return;

        if (_isLoaded && !forceRefresh)
            return;

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
            
            await LoadExecutionMetricsAsync();
            await LoadWeeklyGoalsAsync();
            await GenerateSmartInsightsAsync();
            _isLoaded = true;
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
        GoalCompletionPercent = 0;

        var weekStart = GetMonday(DateTime.Today).ToString("yyyy-MM-dd");
        var goals = await _databaseService.GetWeeklyGoalItemsAsync(weekStart);
        
        foreach (var goal in goals)
        {
            WeeklyGoalItems.Add(new GoalTrackerItem
            {
                Title = goal.Title,
                GoalText = goal.Description,
                IsDone = goal.Status == GoalStatus.Completed
            });
        }

        if (WeeklyGoalItems.Count == 0)
            return;

        int completed = WeeklyGoalItems.Count(g => g.IsDone);
        int total = WeeklyGoalItems.Count;
        int daysLeft = GetDaysLeftInWeek(DateTime.Today);

        GoalCompletionPercent = (double)completed / total * 100;
        GoalsProgressText = $"{completed}/{total} completed";
        GoalCountdownText = daysLeft == 0
            ? "Last day to complete this week's goals"
            : $"{daysLeft} day{(daysLeft == 1 ? string.Empty : "s")} left this week";
        HasWeeklyGoals = true;
    }

    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: appends a new <see cref="GoalTrackerItem"/> to <see cref="WeeklyGoalItems"/>.
    /// </remarks>
    private async Task LoadExecutionMetricsAsync()
    {
        var weekStart = GetMonday(DateTime.Today).ToString("yyyy-MM-dd");
        // Simplified: use CategoryBreakdown logic to populate metrics
        // In a real app, we'd query ScheduleBlocks and CheckIns for this week
        
        TotalPlannedBlocks = 35;
        TotalCompletedBlocks = 28;
        ExecutionScore = (double)TotalCompletedBlocks / TotalPlannedBlocks * 100;
        ConsistencyScore = 80;

        CategoryBreakdown.Clear();
        CategoryBreakdown.Add(new CategoryBreakdown { Category = "work", Minutes = 1200, Color = "#818cf8" });
        CategoryBreakdown.Add(new CategoryBreakdown { Category = "study", Minutes = 600, Color = "#34d399" });
        CategoryBreakdown.Add(new CategoryBreakdown { Category = "exercise", Minutes = 180, Color = "#fbbf24" });
        CategoryBreakdown.Add(new CategoryBreakdown { Category = "routine", Minutes = 300, Color = "#f472b6" });
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
    private async Task GenerateSmartInsightsAsync()
    {
        Recommendations.Clear();
        WeeklyInsights.Clear();

        // 1. Category insights
        var sortedCategories = CategoryBreakdown.OrderByDescending(c => c.Minutes).ToList();
        TopCategory = sortedCategories.FirstOrDefault()?.Category ?? "N/A";
        LowCategory = sortedCategories.LastOrDefault()?.Category ?? "N/A";

        // 2. Execution Score insights
        if (ExecutionScore < 70)
        {
            Recommendations.Add("Consider reducing planned blocks for next week to build momentum.");
            WeeklyInsights.Add(new InsightItem { Icon = "⚠️", Title = "Over-commitment Risk", Description = "You missed about 30% of your planned blocks this week." });
        }
        else
        {
            Recommendations.Add("Great job! Try adding one high-priority goal for next week.");
            WeeklyInsights.Add(new InsightItem { Icon = "⭐", Title = "High Performance", Description = "Your execution score is top-tier this week!" });
        }

        // 3. Day-of-week reliability (Simulated)
        var strongestDay = "Wednesday";
        var weakestDay = "Friday";
        WeeklyInsights.Add(new InsightItem { Icon = "📅", Title = "Day Reliability", Description = $"{strongestDay} is your strongest day. {weakestDay} is where momentum tends to slip." });

        // 4. Energy/Mood correlation
        if (MoodTrend.Any(m => m.Energy < 3))
        {
            Recommendations.Add("Energy levels were low mid-week. Consider more 'Relax' blocks.");
            WeeklyInsights.Add(new InsightItem { Icon = "🔋", Title = "Energy Dip Detected", Description = "Low energy levels observed. Try light exercise to recover." });
        }

        Recommendations.Add($"Focus on your '{LowCategory}' category to balance your week.");
    }
}

public class InsightItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
