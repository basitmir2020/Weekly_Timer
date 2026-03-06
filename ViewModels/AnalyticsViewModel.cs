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
        }
        finally { IsLoading = false; }
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
