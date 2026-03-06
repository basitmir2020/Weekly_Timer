using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;
using WeeklyTimetable.Data;

namespace WeeklyTimetable.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string STATE_KEY = "sched_v2";
    private static readonly string[] OrderedDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    private readonly IPersistenceService _persistenceService;
    private readonly IDatabaseService _databaseService;
    private readonly IStreakService _streakService;
    private readonly INotificationService _notificationService;

    private Dictionary<string, List<ScheduleBlock>> _fullSchedule = new();
    
    // DayName__index -> bool
    private Dictionary<string, bool> _completedState = new();

    [ObservableProperty]
    private string _activeDay;

    [ObservableProperty]
    private string _activeFilter = "all";

    [ObservableProperty]
    private bool _isDayCelebrated;

    [ObservableProperty]
    private bool _hasAnyCompleted;

    [ObservableProperty]
    private DayOverviewViewModel _activeDayStats;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private bool _hasWeeklyGoals;

    [ObservableProperty]
    private string _goalCountdownText = string.Empty;

    [ObservableProperty]
    private string _goalsProgressText = string.Empty;

    // Focus Mode (spec §8.16) — shows only current + next 2 blocks; not persisted
    [ObservableProperty]
    private bool _isFocusMode;

    public ObservableCollection<ScheduleBlock> FilteredBlocks { get; } = new();
    public ObservableCollection<DayOverviewViewModel> WeekStats { get; } = new();
    public ObservableCollection<CategoryStat> ActiveCategories { get; } = new();
    public ObservableCollection<GoalTrackerItem> WeeklyGoalItems { get; } = new();

    public MainViewModel(IPersistenceService persistenceService, IDatabaseService databaseService, IStreakService streakService, INotificationService notificationService)
    {
        _persistenceService = persistenceService;
        _databaseService = databaseService;
        _streakService = streakService;
        _notificationService = notificationService;
        
        ActiveDayStats = new DayOverviewViewModel();
        ActiveDay = DateTime.Today.DayOfWeek.ToString();
    }

    public async Task LoadDataAsync()
    {
        try
        {
            // 1. Load the mock template structure
            _fullSchedule = ScheduleData.GetDefaultSchedule();

            // 2. Load completions from Persistence
            var state = await _persistenceService.LoadStateAsync<Dictionary<string, bool>>(STATE_KEY);
            if (state != null)
            {
                _completedState = state;
                
                // Map state onto blocks
                foreach (var kvp in _fullSchedule)
                {
                    var dayName = kvp.Key;
                    var blocks = kvp.Value;
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        string stateKey = $"{dayName}__{i}";
                        blocks[i].IsCompleted = _completedState.ContainsKey(stateKey) && _completedState[stateKey];
                    }
                }
            }

            EnsureActiveDayIsValid();

            // 3. Setup Weekly Stats
            UpdateWeekStats();

            // 4. Force UI update for the Active Day on initial load
            ApplyActiveDay(ActiveDay);

            // Keep streak storage consistent with today's completion state.
            await SyncTodayStreakAsync();

            // Refresh weekly goals snapshot for the home screen.
            await LoadWeeklyGoalsAsync();

            // 5. Calculate Current Streak
            if (!IsActiveDayToday())
            {
                CurrentStreak = await _streakService.GetCurrentStreakAsync();
            }

            // 6. Schedule notifications (best effort; should never block UI)
            _ = SafeScheduleNotificationsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during MainViewModel initialization: {ex.Message}");
        }
    }

    private async Task SafeScheduleNotificationsAsync()
    {
        try
        {
            await ScheduleNotificationsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error while scheduling notifications: {ex.Message}");
        }
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

    private void EnsureActiveDayIsValid()
    {
        if (!string.IsNullOrWhiteSpace(ActiveDay) && _fullSchedule.ContainsKey(ActiveDay))
            return;

        var today = DateTime.Today.DayOfWeek.ToString();
        ActiveDay = _fullSchedule.ContainsKey(today)
            ? today
            : _fullSchedule.Keys.FirstOrDefault() ?? "Monday";
    }

    private async Task ScheduleNotificationsAsync()
    {
        await _notificationService.CancelAllNotificationsAsync();
        bool hasPermission = await _notificationService.RequestPermissionsAsync();

        if (hasPermission && _fullSchedule.TryGetValue(ActiveDay, out var blocks))
        {
            DateTime now = DateTime.Now;
            int notificationId = 1000;

            foreach (var block in blocks)
            {
                if (block.IsCompleted) continue;
                if (!TimeSpan.TryParse(block.Time, out TimeSpan blockTime)) continue;

                var blockDateTime = DateTime.Today.Add(blockTime);
                if (blockDateTime < now) continue;

                var notifyTime = blockDateTime.AddMinutes(-5);
                if (notifyTime > now)
                {
                    string msg = $"{block.Icon} {block.Label} starts in 5 minutes.";
                    _notificationService.ScheduleNotificationAsync("Upcoming Activity", msg, notifyTime, notificationId++);
                }
            }
        }
    }

    [RelayCommand]
    private async Task SwitchDayAsync(string dayName)
    {
        var todayName = DateTime.Today.DayOfWeek.ToString();
        if (!string.Equals(dayName, todayName, StringComparison.OrdinalIgnoreCase))
        {
            ApplyActiveDay(todayName);
            int targetIndex = Array.IndexOf(OrderedDays, dayName);
            int todayIndex = Array.IndexOf(OrderedDays, todayName);

            if (targetIndex >= 0 && todayIndex >= 0)
            {
                if (targetIndex < todayIndex)
                {
                    await ShowPreviousDayToastAsync();
                }
                else if (targetIndex > todayIndex)
                {
                    await ShowForwardDayToastAsync();
                }
            }

            return;
        }

        ApplyActiveDay(todayName);
    }

    private static Task ShowPreviousDayToastAsync()
    {
        return ShowShortToastAsync("You cannot go to previous day.");
    }

    private static Task ShowForwardDayToastAsync()
    {
        return ShowShortToastAsync("You cannot go to forward day.");
    }

    private static async Task ShowShortToastAsync(string message)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Toast.Make(message, ToastDuration.Short).Show();
        });
    }

    private void ApplyActiveDay(string dayName)
    {
        ActiveDay = dayName;
        ActiveFilter = "all"; // Reset filter
        
        foreach (var stat in WeekStats)
        {
            stat.IsActive = (stat.DayName == dayName);
        }

        ApplyFilter();
        UpdateDayStats();
    }

    private bool IsActiveDayToday()
    {
        return string.Equals(ActiveDay, DateTime.Today.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task SyncTodayStreakAsync()
    {
        if (!IsActiveDayToday())
            return;

        await _streakService.CheckAndAwardStreakAsync(IsDayCelebrated, ActiveDayStats.CompletionPct);
        CurrentStreak = await _streakService.GetCurrentStreakAsync();
    }

    [RelayCommand]
    public void SetFilter(string categoryKey)
    {
        ActiveFilter = ActiveFilter == categoryKey ? "all" : categoryKey;
        ApplyFilter();
    }

    partial void OnIsFocusModeChanged(bool value)
    {
        if (_fullSchedule.Count == 0 || string.IsNullOrWhiteSpace(ActiveDay) || !_fullSchedule.ContainsKey(ActiveDay))
            return;

        ApplyFilter();
    }

    /// <summary>Open BlockDetailSheet to view notes for a block (spec §8.11)</summary>
    [RelayCommand]
    private async Task OpenBlockDetailAsync(ScheduleBlock block)
    {
        if (block == null) return;
        var page = new Views.BlockDetailSheet(block, _databaseService);
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    /// <summary>Navigate to EditBlockPage to add a new empty block (spec §8.14)</summary>
    [RelayCommand]
    private async Task AddBlockAsync()
    {
        var page = new Views.EditBlockPage(_databaseService);
        page.LoadBlock(null, ActiveDay);
        if (page.BindingContext is ViewModels.EditBlockViewModel vm)
        {
            vm.OnSaved = (block, isNew) =>
            {
                if (isNew && _fullSchedule.ContainsKey(ActiveDay))
                {
                    _fullSchedule[ActiveDay].Add(block);
                    ApplyFilter();
                    UpdateDayStats();
                    UpdateWeekStats();
                    _ = _persistenceService.SaveStateAsync(STATE_KEY, _completedState);
                    _ = SyncTodayStreakAsync();
                }
            };
        }
        await Shell.Current.Navigation.PushAsync(page);
    }

    private void ApplyFilter()
    {
        FilteredBlocks.Clear();
        ActiveCategories.Clear();

        if (!_fullSchedule.ContainsKey(ActiveDay)) return;

        var dayBlocks = _fullSchedule[ActiveDay];

        if (!IsFocusMode &&
            ActiveFilter != "all" &&
            !dayBlocks.Any(b => b.Category == ActiveFilter))
        {
            ActiveFilter = "all";
        }
        
        // Populate filter pills based on what categories exist today
        ActiveCategories.Add(new CategoryStat { Key = "all", Count = dayBlocks.Count });
        var categoriesToday = dayBlocks.GroupBy(b => b.Category)
                                       .Select(g => new CategoryStat { Key = g.Key, Count = g.Count() })
                                       .ToList();
                                       
        foreach (var c in categoriesToday) ActiveCategories.Add(c);

        // Focus Mode: show current block + next 2 upcoming uncompleted
        IEnumerable<ScheduleBlock> blocksToShow;
        if (IsFocusMode)
        {
            var now = DateTime.Now.TimeOfDay;
            var upcoming = dayBlocks
                .Where(b => !b.IsCompleted && TimeSpan.TryParse(b.Time, out var t) && t >= now)
                .Take(3)
                .ToList();
            blocksToShow = upcoming.Any() ? upcoming : dayBlocks.Take(3);
        }
        else
        {
            blocksToShow = ActiveFilter == "all"
                ? dayBlocks
                : dayBlocks.Where(b => b.Category == ActiveFilter);
        }

        foreach (var block in blocksToShow)
        {
            FilteredBlocks.Add(block);
        }
    }

    [RelayCommand]
    private async Task ToggleStepAsync(ScheduleBlock block)
    {
        if (!_fullSchedule.ContainsKey(ActiveDay)) return;
        
        // Note: CommunityToolkit doesn't auto-notify nested object property changes 
        // without ObservableProperty in the model, but since MAUI CheckBoxes two-way bind,
        // it may visibly update immediately. We force an update otherwise.
        block.IsCompleted = !block.IsCompleted;
        
        var list = _fullSchedule[ActiveDay];
        int index = list.IndexOf(block);
        if (index != -1)
        {
            string stateKey = $"{ActiveDay}__{index}";
            if (block.IsCompleted)
                _completedState[stateKey] = true;
            else
                _completedState.Remove(stateKey);
                
            await _persistenceService.SaveStateAsync(STATE_KEY, _completedState);
        }

        UpdateDayStats();
        UpdateWeekStats(); // Update the top header dots
        await SyncTodayStreakAsync();
    }

    [RelayCommand]
    private async Task ResetDayAsync()
    {
        if (!_fullSchedule.ContainsKey(ActiveDay)) return;

        var list = _fullSchedule[ActiveDay];
        for (int i = 0; i < list.Count; i++)
        {
            list[i].IsCompleted = false;
            string stateKey = $"{ActiveDay}__{i}";
            _completedState.Remove(stateKey);
        }

        await _persistenceService.SaveStateAsync(STATE_KEY, _completedState);
        
        // Refresh UI
        var currentFilter = ActiveFilter;
        ApplyFilter();
        ActiveFilter = currentFilter;
        
        IsDayCelebrated = false;
        UpdateDayStats();
        UpdateWeekStats();
        await SyncTodayStreakAsync();
    }

    private void UpdateDayStats()
    {
        if (!_fullSchedule.ContainsKey(ActiveDay)) return;
        
        var blocks = _fullSchedule[ActiveDay];
        int total = blocks.Count;
        int done = blocks.Count(b => b.IsCompleted);
        
        ActiveDayStats = new DayOverviewViewModel
        {
            TotalBlocks = total,
            CompletedBlocks = done,
            CompletionPct = total == 0 ? 0 : (int)Math.Round((double)done / total * 100),
            DayAbbreviation = ActiveDay.Substring(0, 3).ToUpper()
        };

        HasAnyCompleted = done > 0;
        
        // Trigger celebration only if 100% complete
        IsDayCelebrated = total > 0 && done == total;
    }

    private void UpdateWeekStats()
    {
        WeekStats.Clear();
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        
        foreach (var day in days)
        {
            if (_fullSchedule.TryGetValue(day, out var blocks))
            {
                int total = blocks.Count;
                int done = blocks.Count(b => b.IsCompleted);
                var pct = total == 0 ? 0 : (int)Math.Round((double)done / total * 100);

                WeekStats.Add(new DayOverviewViewModel
                {
                    DayName = day,
                    DayAbbreviation = day.Substring(0, 3).ToUpper(),
                    TotalBlocks = total,
                    CompletedBlocks = done,
                    CompletionPct = pct,
                    IsActive = day.Equals(ActiveDay, StringComparison.OrdinalIgnoreCase)
                });
            }
        }
    }
}

public class CategoryStat
{
    public string Key { get; set; }
    public int Count { get; set; }
    public string DisplayText => $"{Key.ToUpper()} ({Count})";
}

public class GoalTrackerItem
{
    public string Title { get; set; } = string.Empty;
    public string GoalText { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public string StatusText => IsDone ? "✓" : "○";
    public string StatusColor => IsDone ? "#34d399" : "#94a3b8";
}
