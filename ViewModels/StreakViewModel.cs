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

    public StreakViewModel(IStreakService streakService)
    {
        _streakService = streakService;
        _ = LoadAsync();
    }

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
