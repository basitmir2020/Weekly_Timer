using CommunityToolkit.Mvvm.ComponentModel;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class StreakViewModel : ObservableObject
{
    private readonly IStreakService _streakService;

    [ObservableProperty] private int _currentStreak;
    [ObservableProperty] private int _longestStreak;
    [ObservableProperty] private int _totalCompleteDays;
    [ObservableProperty] private List<StreakDayEntry> _last30Days = new();

    /// <summary>
    /// Creates the streak dashboard view model and triggers initial streak data load.
    /// </summary>
    /// <param name="streakService">Service used to fetch streak aggregates and recent history.</param>
    /// <remarks>
    /// Side effects: starts asynchronous loading of streak metrics.
    /// </remarks>
    public StreakViewModel(IStreakService streakService)
    {
        _streakService = streakService;
        _ = LoadAsync();
    }

    /// <summary>
    /// Loads current streak values and last-30-day completion data.
    /// </summary>
    /// <returns>A task that completes once all streak properties are refreshed.</returns>
    /// <remarks>
    /// Side effects: updates summary counters and day list used by streak visualization.
    /// </remarks>
    private async Task LoadAsync()
    {
        CurrentStreak = await _streakService.GetCurrentStreakAsync();
        LongestStreak = await _streakService.GetLongestStreakAsync();
        Last30Days = await _streakService.GetLast30DaysAsync();
        TotalCompleteDays = Last30Days.Count(d => d.IsComplete);
    }
}

public class StreakDayEntry
{
    public DateTime Date { get; set; }
    public bool IsComplete { get; set; }
    public string DotColor => IsComplete ? "#22c55e" : "#1e293b";
}
