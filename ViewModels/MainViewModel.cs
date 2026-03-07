using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;
using WeeklyTimetable.Data;
using CommunityToolkit.Mvvm.Messaging;

namespace WeeklyTimetable.ViewModels;

public partial class MainViewModel : ObservableObject, IRecipient<WeeklyTimetable.Models.ScheduleChangedMessage>
{
    private const string STATE_KEY_V3 = "sched_v3";
    private const string STATE_KEY = "sched_v2";
    private static readonly string[] OrderedDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    private readonly IPersistenceService _persistenceService;
    private readonly IStreakService _streakService;
    private readonly INotificationService _notificationService;
    private readonly IAlarmService _alarmService;
    private readonly IAlarmSchedulerService _alarmScheduler;
    private readonly SemaphoreSlim _loadGate = new(1, 1);

    // Foreground alarm-check timer — fires every 30 s while the app is running.
    private System.Threading.Timer? _alarmCheckTimer;
    // Guard: track which block IDs have already triggered the alarm this session.
    private readonly HashSet<string> _alarmFiredKeys = new();

    private Dictionary<string, List<ScheduleBlock>> _fullSchedule = new();

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

    // Focus Mode (spec §8.16) — shows only current + next 2 blocks; not persisted
    [ObservableProperty]
    private bool _isFocusMode;

    /// <summary>True while the alarm is actively ringing — binds to the Stop Alarm banner visibility.</summary>
    [ObservableProperty]
    private bool _isAlarmActive;
    
    public bool IsLoaded { get; private set; }

    public ObservableCollection<ScheduleBlock> FilteredBlocks { get; } = new();
    public ObservableCollection<DayOverviewViewModel> WeekStats { get; } = new();
    public ObservableCollection<CategoryStat> ActiveCategories { get; } = new();

    /// <summary>
    /// Creates the main schedule view model and wires up dependencies for persistence, streaks, notifications, and alarm.
    /// </summary>
    /// <param name="persistenceService">Service used to save and load schedule state.</param>
    /// <param name="streakService">Service used to compute and store streak progress.</param>
    /// <param name="notificationService">Service used to schedule reminder notifications.</param>
    /// <param name="alarmService">Service used to start/stop the looping in-app alarm.</param>
    /// <remarks>
    /// Side effects: registers this instance with the weak-reference messenger for schedule refresh events.
    /// </remarks>
    public MainViewModel(IPersistenceService persistenceService, IStreakService streakService, INotificationService notificationService, IAlarmService alarmService, IAlarmSchedulerService alarmScheduler)
    {
        _persistenceService = persistenceService;
        _streakService = streakService;
        _notificationService = notificationService;
        _alarmService = alarmService;
        _alarmScheduler = alarmScheduler;
        
        ActiveDayStats = new DayOverviewViewModel();
        ActiveDay = DateTime.Today.DayOfWeek.ToString();

        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.RegisterAll(this);
    }

    /// <summary>
    /// Loads schedule data, applies migration from legacy storage, refreshes UI state, and triggers notification/streak sync.
    /// </summary>
    /// <returns>A task that completes when initial data loading and state synchronization finishes.</returns>
    /// <remarks>
    /// Side effects: mutates in-memory schedule collections, persists migrated state, updates streak state, and schedules notifications.
    /// </remarks>
    public async Task LoadDataAsync()
    {
        if (IsLoaded)
            return;

        await _loadGate.WaitAsync();
        try
        {
            if (IsLoaded)
                return;

            // 1. Load full custom schedule if exists
            var customSchedule = await _persistenceService.LoadStateAsync<Dictionary<string, List<ScheduleBlock>>>(STATE_KEY_V3);
            if (customSchedule != null && customSchedule.Count > 0)
            {
                _fullSchedule = customSchedule;
            }
            else
            {
                // Fallback to defaults
                _fullSchedule = ScheduleData.GetDefaultSchedule();

                // 2. Load legacy completions from Persistence
                var state = await _persistenceService.LoadStateAsync<Dictionary<string, bool>>(STATE_KEY);
                if (state != null)
                {
                    // Map state onto blocks
                    foreach (var kvp in _fullSchedule)
                    {
                        // Legacy state keys are "DayName__Index"; map those values into the richer block model.
                        var dayName = kvp.Key;
                        var blocks = kvp.Value;
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            string stateKey = $"{dayName}__{i}";
                            blocks[i].IsCompleted = state.ContainsKey(stateKey) && state[stateKey];
                        }
                    }
                }
                
                // Save immediately to new v3 format
                await _persistenceService.SaveStateAsync(STATE_KEY_V3, _fullSchedule);
            }

            EnsureActiveDayIsValid();

            // 3. Setup Weekly Stats
            UpdateWeekStats();

            // 4. Force UI update for the Active Day on initial load
            ApplyActiveDay(ActiveDay);

            // Keep streak storage consistent with today's completion state.
            await SyncTodayStreakAsync();

            // 5. Calculate Current Streak
            if (!IsActiveDayToday())
            {
                CurrentStreak = await _streakService.GetCurrentStreakAsync();
            }

            // 6. Schedule notifications (best effort; should never block UI)
            _ = SafeScheduleNotificationsAsync();

            // 7. Start foreground alarm-check timer (fires every 30 s)
            _alarmFiredKeys.Clear();
            StartAlarmCheckTimer();
            
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during MainViewModel initialization: {ex.Message}");
        }
        finally
        {
            _loadGate.Release();
        }
    }

    /// <summary>
    /// Stops the ringing alarm when the user taps the Stop Alarm button.
    /// </summary>
    [RelayCommand]
    private void StopAlarm()
    {
        _alarmService.StopAlarm();
        _alarmScheduler.CancelAll();
        IsAlarmActive = false;

        if (Preferences.Get("haptic_enabled", true))
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    }

    /// <summary>
    /// Starts a background timer that checks every 30 seconds whether any today-block
    /// is starting within the next 5 minutes and should trigger the looping alarm.
    /// </summary>
    /// <remarks>
    /// Side effects: allocates a System.Threading.Timer that fires on the thread pool.
    /// </remarks>
    private void StartAlarmCheckTimer()
    {
        _alarmCheckTimer?.Dispose();
        _alarmCheckTimer = new System.Threading.Timer(
            _ => CheckAndFireAlarm(),
            null,
            TimeSpan.Zero,             // fire immediately on load
            TimeSpan.FromSeconds(30)); // then every 30 s
    }

    /// <summary>
    /// Evaluates today's schedule and triggers the alarm for any block whose start is
    /// within the 5-minute warning window and has not already been alerted this session.
    /// </summary>
    /// <remarks>
    /// Runs on the thread-pool timer callback — dispatches UI updates to the main thread.
    /// </remarks>
    private void CheckAndFireAlarm()
    {
        try
        {
            var todayName = DateTime.Today.DayOfWeek.ToString();
            if (!_fullSchedule.TryGetValue(todayName, out var blocks)) return;

            var now = DateTime.Now;

            foreach (var block in blocks)
            {
                if (block.IsCompleted) continue;
                if (!TimeSpan.TryParse(block.Time, out var blockTime)) continue;

                var blockStart = DateTime.Today.Add(blockTime);
                var minutesUntilStart = (blockStart - now).TotalMinutes;

                // Alarm window: between 5 minutes before start and the start time itself.
                if (minutesUntilStart is <= 5 and >= -1)
                {
                    // Unique key: day + time + label to avoid duplicate triggers
                    string key = $"{todayName}_{block.Time}_{block.Label}";
                    if (_alarmFiredKeys.Contains(key)) continue;

                    _alarmFiredKeys.Add(key);
                    _alarmService.StartAlarm();

                    MainThread.BeginInvokeOnMainThread(() => IsAlarmActive = true);
                    break; // only start one alarm at a time
                }
            }

            // Keep IsAlarmActive in sync if the alarm was externally stopped.
            if (!_alarmService.IsAlarmRinging && IsAlarmActive)
            {
                MainThread.BeginInvokeOnMainThread(() => IsAlarmActive = false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlarmCheck] Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles schedule change messages and reloads the view model when a global update is requested.
    /// </summary>
    /// <param name="message">Message payload indicating whether a reload should occur.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: resets the loaded flag and starts a new asynchronous load operation.
    /// </remarks>
    public void Receive(WeeklyTimetable.Models.ScheduleChangedMessage message)
    {
        if (message.Value)
        {
            MainThread.BeginInvokeOnMainThread(() => 
            {
                IsLoaded = false;
                _ = LoadDataAsync();
            });
        }
    }

    /// <summary>
    /// Runs notification scheduling in a guarded context so errors never break the main UI flow.
    /// </summary>
    /// <returns>A task that completes after scheduling attempt finishes.</returns>
    /// <remarks>
    /// Side effects: may queue notifications through <see cref="_notificationService"/>.
    /// </remarks>
    private async Task SafeScheduleNotificationsAsync()
    {
        try
        {
            await ScheduleNotificationsAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error while scheduling notifications: {ex.Message}");
        }
    }

    /// <summary>
    /// Ensures the currently selected active day exists in the loaded schedule.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: updates <see cref="ActiveDay"/> when it is missing or invalid.
    /// </remarks>
    private void EnsureActiveDayIsValid()
    {
        if (!string.IsNullOrWhiteSpace(ActiveDay) && _fullSchedule.ContainsKey(ActiveDay))
            return;

        var today = DateTime.Today.DayOfWeek.ToString();
        ActiveDay = _fullSchedule.ContainsKey(today)
            ? today
            : _fullSchedule.Keys.FirstOrDefault() ?? "Monday";
    }

    /// <summary>
    /// Cancels existing reminders and schedules start/end notifications for the next seven days.
    /// </summary>
    /// <returns>A task that completes when notification scheduling has been attempted.</returns>
    /// <remarks>
    /// Side effects: clears previous notifications and enqueues new local notifications.
    /// </remarks>
    private async Task ScheduleNotificationsAsync()
    {
        if (!Preferences.Get("notif_enabled", true))
        {
            await _notificationService.CancelAllNotificationsAsync();
            _alarmScheduler.CancelAll();
            return;
        }

        await _notificationService.CancelAllNotificationsAsync();
        _alarmScheduler.CancelAll();

        bool hasPermission = await _notificationService.RequestPermissionsAsync();

        if (hasPermission)
        {
            DateTime now = DateTime.Now;
            DateTime horizon = now.AddDays(7);
            int notificationId = 1000;
            
            var today = DateTime.Today;
            for (int dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                var targetDate = today.AddDays(dayOffset);
                string targetDayName = targetDate.DayOfWeek.ToString();
                
                if (_fullSchedule.TryGetValue(targetDayName, out var blocks))
                {
                    foreach (var block in blocks)
                    {
                        // Skip already completed blocks for today to avoid redundant alerts.
                        if (dayOffset == 0 && block.IsCompleted) continue;
                        // Guard against invalid user-edited times.
                        if (!TimeSpan.TryParse(block.Time, out TimeSpan blockTime)) continue;

                        var blockStartDateTime = targetDate.Add(blockTime);
                        var blockEndDateTime = blockStartDateTime.AddMinutes(block.DurationMinutes);

                        // Start alert 5 mins before
                        var startNotifyTime = blockStartDateTime.AddMinutes(-5);
                        if (startNotifyTime > now && startNotifyTime < horizon)
                        {
                            await _notificationService.ScheduleNotificationAsync(
                                "Upcoming Activity", 
                                $"{block.Icon} {block.Label} starts in 5 minutes.", 
                                startNotifyTime, notificationId);

                            _alarmScheduler.ScheduleAlarm(
                                notificationId++, 
                                startNotifyTime, 
                                "Activity Starting", 
                                $"{block.Icon} {block.Label} starts in 5 minutes.");
                        }

                        // End alert 5 mins before
                        var endNotifyTime = blockEndDateTime.AddMinutes(-5);
                        if (endNotifyTime > now && endNotifyTime < horizon)
                        {
                            await _notificationService.ScheduleNotificationAsync(
                                "Wrap Up", 
                                $"{block.Icon} {block.Label} ends in 5 minutes. Wrap up!", 
                                endNotifyTime, notificationId++);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Attempts to switch to a selected day while enforcing the product rule that only today's schedule is interactive.
    /// </summary>
    /// <param name="dayName">Requested day name from the UI.</param>
    /// <returns>A task that completes after any feedback toast is shown.</returns>
    /// <remarks>
    /// Side effects: updates active day/filter state and may display informational toasts.
    /// </remarks>
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

    /// <summary>
    /// Shows feedback that users cannot navigate backward from the current day.
    /// </summary>
    /// <returns>A task representing toast display completion.</returns>
    private static Task ShowPreviousDayToastAsync()
    {
        return ShowShortToastAsync("You cannot go to previous day.");
    }

    /// <summary>
    /// Shows feedback that users cannot navigate forward from the current day.
    /// </summary>
    /// <returns>A task representing toast display completion.</returns>
    private static Task ShowForwardDayToastAsync()
    {
        return ShowShortToastAsync("You cannot go to forward day.");
    }

    /// <summary>
    /// Displays a short toast message on the UI thread.
    /// </summary>
    /// <param name="message">Text to show in the toast.</param>
    /// <returns>A task representing toast display completion.</returns>
    /// <remarks>
    /// Side effects: dispatches work to the main thread and shows a transient toast.
    /// </remarks>
    private static async Task ShowShortToastAsync(string message)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Toast.Make(message, ToastDuration.Short).Show();
        });
    }

    /// <summary>
    /// Applies the selected day as active, resets filters, and recomputes day-level UI state.
    /// </summary>
    /// <param name="dayName">Day to activate.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: mutates active day, active filter, week highlight state, filtered list, and overview stats.
    /// </remarks>
    private void ApplyActiveDay(string dayName)
    {
        ActiveDay = dayName;
        ActiveFilter = "all"; // Reset filter
        
        foreach (var stat in WeekStats)
        {
            // Keep top week strip in sync with the selected day.
            stat.IsActive = (stat.DayName == dayName);
        }

        ApplyFilter();
        UpdateDayStats();
    }

    /// <summary>
    /// Determines whether the currently active day corresponds to today's date.
    /// </summary>
    /// <returns><c>true</c> when the active day is today; otherwise <c>false</c>.</returns>
    private bool IsActiveDayToday()
    {
        return string.Equals(ActiveDay, DateTime.Today.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Updates today's streak status when the active day is today and refreshes the displayed streak count.
    /// </summary>
    /// <returns>A task that completes after streak state is synchronized.</returns>
    /// <remarks>
    /// Side effects: writes streak records through the streak service and updates <see cref="CurrentStreak"/>.
    /// </remarks>
    private async Task SyncTodayStreakAsync()
    {
        if (!IsActiveDayToday())
            return;

        await _streakService.CheckAndAwardStreakAsync(IsDayCelebrated, ActiveDayStats.CompletionPct);
        CurrentStreak = await _streakService.GetCurrentStreakAsync();
    }

    /// <summary>
    /// Toggles a category filter for the active day and refreshes the visible blocks.
    /// </summary>
    /// <param name="categoryKey">Category key selected by the user.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: updates <see cref="ActiveFilter"/> and repopulates <see cref="FilteredBlocks"/>.
    /// </remarks>
    [RelayCommand]
    public void SetFilter(string categoryKey)
    {
        ActiveFilter = ActiveFilter == categoryKey ? "all" : categoryKey;
        ApplyFilter();
    }

    /// <summary>
    /// Reapplies filtering whenever focus mode changes.
    /// </summary>
    /// <param name="value">New focus mode value.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: refreshes filtered block output when state is valid.
    /// </remarks>
    partial void OnIsFocusModeChanged(bool value)
    {
        if (_fullSchedule.Count == 0 || string.IsNullOrWhiteSpace(ActiveDay) || !_fullSchedule.ContainsKey(ActiveDay))
            return;

        ApplyFilter();
    }

    /// <summary>
    /// Opens a modal detail sheet for the selected block so notes can be viewed or edited.
    /// </summary>
    /// <param name="block">Block selected from the active day list.</param>
    /// <returns>A task that completes after modal navigation is requested.</returns>
    /// <remarks>
    /// Side effects: pushes a modal page onto the navigation stack.
    /// </remarks>
    [RelayCommand]
    private async Task OpenBlockDetailAsync(ScheduleBlock block)
    {
        if (block == null) return;
        var page = new Views.BlockDetailSheet(block);
        await Shell.Current.Navigation.PushModalAsync(page);
    }

    /// <summary>
    /// Opens the block editor for the active day and handles insertion/sorting of newly created blocks.
    /// </summary>
    /// <returns>A task that completes after navigation to the edit page.</returns>
    /// <remarks>
    /// Side effects: may mutate active-day schedule, persist schedule changes, refresh UI collections, and sync streak state.
    /// </remarks>
    [RelayCommand]
    private async Task AddBlockAsync()
    {
        var currentBlocks = _fullSchedule.ContainsKey(ActiveDay) ? _fullSchedule[ActiveDay] : new List<ScheduleBlock>();
        var vm = new ViewModels.EditBlockViewModel(currentDayBlocks: currentBlocks);
        
        vm.OnSaved = (block, isNew) =>
        {
            if (isNew && _fullSchedule.ContainsKey(ActiveDay))
            {
                var targetList = _fullSchedule[ActiveDay];
                targetList.Add(block);

                // Sort chronologically by time
                targetList.Sort(CompareBlocksByTime);
                
                // Force UI to pick up the brand new list ordering
                _fullSchedule[ActiveDay] = new List<ScheduleBlock>(targetList);
                
                ApplyFilter();
                UpdateDayStats();
                UpdateWeekStats();
                _ = _persistenceService.SaveStateAsync(STATE_KEY_V3, _fullSchedule);
                _ = SyncTodayStreakAsync();
            }
        };

        var page = new Views.EditBlockPage { BindingContext = vm };
        await Shell.Current.Navigation.PushAsync(page);
    }

    /// <summary>
    /// Builds the filtered block list and category chips for the active day, including focus-mode projection.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: clears and repopulates <see cref="FilteredBlocks"/> and <see cref="ActiveCategories"/>.
    /// </remarks>
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
            // Recover from stale filter values when categories changed after edits.
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
            // Fallback keeps UI populated even when no upcoming blocks satisfy the strict predicate.
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

    /// <summary>
    /// Marks a block as completed after validating timing constraints for today's schedule.
    /// </summary>
    /// <param name="block">Block selected for completion.</param>
    /// <returns>A task that completes after persistence and UI refresh are done.</returns>
    /// <remarks>
    /// Side effects: mutates block completion state, persists schedule, updates stats, and may update streak records.
    /// </remarks>
    [RelayCommand]
    private async Task ToggleStepAsync(ScheduleBlock block)
    {
        // Naming suggestion: `CompleteBlockAsync` would better reflect one-way completion behavior.
        if (!_fullSchedule.ContainsKey(ActiveDay)) return;
        
        if (block.IsCompleted)
        {
            await ShowShortToastAsync("Cannot uncheck a completed block.");
            return;
        }

        // Strict validation: cannot complete if block is missed
        if (IsActiveDayToday() && TimeSpan.TryParse(block.Time, out TimeSpan blockTime))
        {
            var blockEndTime = blockTime.Add(TimeSpan.FromMinutes(block.DurationMinutes));
            // If the end time crosses midnight, handle it properly for today
            if (blockEndTime >= blockTime && DateTime.Now.TimeOfDay > blockEndTime)
            {
                await ShowShortToastAsync("Block missed, cannot complete.");
                return;
            }
        }

        block.IsCompleted = true;

        if (Preferences.Get("haptic_enabled", true))
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        
        var list = _fullSchedule[ActiveDay];
        int index = list.IndexOf(block);
        if (index != -1)
        {
            await _persistenceService.SaveStateAsync(STATE_KEY_V3, _fullSchedule);
        }

        UpdateDayStats();
        UpdateWeekStats(); // Update the top header dots
        await SyncTodayStreakAsync();
    }

    /// <summary>
    /// Resets all completion flags for the active day and recalculates all day/week aggregates.
    /// </summary>
    /// <returns>A task that completes after persistence and aggregate refreshes finish.</returns>
    /// <remarks>
    /// Side effects: mutates completion flags, saves schedule state, updates filters/stats, and updates streak state.
    /// </remarks>
    [RelayCommand]
    private async Task ResetDayAsync()
    {
        if (!_fullSchedule.ContainsKey(ActiveDay)) return;

        var list = _fullSchedule[ActiveDay];
        for (int i = 0; i < list.Count; i++)
        {
            // Explicit index-based loop avoids allocating an enumerator on hot UI paths.
            list[i].IsCompleted = false;
        }

        await _persistenceService.SaveStateAsync(STATE_KEY_V3, _fullSchedule);
        
        // Refresh UI
        var currentFilter = ActiveFilter;
        ApplyFilter();
        ActiveFilter = currentFilter;
        
        IsDayCelebrated = false;
        UpdateDayStats();
        UpdateWeekStats();
        await SyncTodayStreakAsync();
    }

    /// <summary>
    /// Recomputes aggregate progress metrics for the active day.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: replaces <see cref="ActiveDayStats"/> and updates celebration/completion flags.
    /// </remarks>
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

    /// <summary>
    /// Rebuilds week-level overview statistics used by the top day strip.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: clears and repopulates <see cref="WeekStats"/>.
    /// </remarks>
    private void UpdateWeekStats()
    {
        WeekStats.Clear();
        var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        
        foreach (var day in days)
        {
            // Only add days present in the current schedule snapshot.
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

    private static int CompareBlocksByTime(ScheduleBlock a, ScheduleBlock b)
    {
        // Invalid times are intentionally pushed after valid times to keep a predictable order.
        bool aValid = TimeSpan.TryParse(a.Time, out var aTime);
        bool bValid = TimeSpan.TryParse(b.Time, out var bTime);

        if (aValid && bValid) return aTime.CompareTo(bTime);
        if (aValid) return -1;
        if (bValid) return 1;
        return 0;
    }
}

public class CategoryStat
{
    public string Key { get; set; } = string.Empty;
    public int Count { get; set; }
    public string DisplayText => $"{Key.ToUpper()} ({Count})";
}
