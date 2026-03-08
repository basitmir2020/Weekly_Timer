using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISupabaseSyncService _supabase;
    private readonly IPersistenceService _persistence;
    private readonly IAlarmSoundPickerService _alarmSoundPicker;
    private bool _isMasterScheduleLoaded;

    [ObservableProperty] private bool _notificationsEnabled;
    [ObservableProperty] private bool _hapticEnabled = true;
    [ObservableProperty] private string _selectedTheme = "Dark";
    [ObservableProperty] private string _selectedFontSize = "Medium";
    [ObservableProperty] private string _selectedAlarmSoundName = "Default";
    
    // Master Schedule Editor
    [ObservableProperty] private string _selectedDay = "Monday";
    public List<string> DaysOfWeek { get; } = new() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    public ObservableCollection<TemplateBlockEditor> DayBlocks { get; } = new();
    private Dictionary<string, List<ScheduleBlock>> _masterSchedule = new();

    // Supabase Bindings
    [ObservableProperty] private string _supabaseEmail = "";
    [ObservableProperty] private string _supabasePassword = "";
    [ObservableProperty] private string _supabaseStatusMessage = "";

    public List<string> Themes { get; } = new() { "Dark", "Light", "System" };
    public List<string> FontSizes { get; } = new() { "Small", "Medium", "Large" };

    /// <summary>
    /// Creates the settings view model and hydrates persisted preferences and editable master schedule data.
    /// </summary>
    /// <param name="supabase">Cloud backup/restore service.</param>
    /// <param name="persistence">Local persistence service for schedule template data.</param>
    /// <param name="alarmSoundPicker">Platform service for selecting an alarm ringtone.</param>
    /// <remarks>
    /// Side effects: reads from preferences and starts asynchronous schedule loading.
    /// </remarks>
    public SettingsViewModel(ISupabaseSyncService supabase, IPersistenceService persistence, IAlarmSoundPickerService alarmSoundPicker)
    {
        _supabase = supabase;
        _persistence = persistence;
        _alarmSoundPicker = alarmSoundPicker;
        NotificationsEnabled = Preferences.Get("notif_enabled", true);
        HapticEnabled        = Preferences.Get("haptic_enabled", true);
        SelectedTheme        = Preferences.Get("theme", "Dark");
        SelectedFontSize     = Preferences.Get("font_size", "Medium");

        // Resolve the display name for the previously chosen alarm sound
        var savedUri = Preferences.Get("alarm_sound_uri", null);
        SelectedAlarmSoundName = _alarmSoundPicker.GetSoundName(savedUri);
    }

    public Task EnsureMasterScheduleLoadedAsync() => LoadMasterScheduleAsync(forceReload: false);

    /// <summary>
    /// Loads the editable master weekly schedule from storage or falls back to bundled defaults.
    /// </summary>
    /// <returns>A task that completes after in-memory schedule data is prepared.</returns>
    /// <remarks>
    /// Side effects: updates internal schedule cache and refreshes day block editor collection.
    /// </remarks>
    private async Task LoadMasterScheduleAsync(bool forceReload)
    {
        if (_isMasterScheduleLoaded && !forceReload)
            return;

        var sched = await _persistence.LoadStateAsync<Dictionary<string, List<ScheduleBlock>>>("sched_v3");
        if (sched != null && sched.Count > 0)
        {
            _masterSchedule = sched;
        }
        else
        {
            _masterSchedule = WeeklyTimetable.Data.ScheduleData.GetDefaultSchedule();
        }

        _isMasterScheduleLoaded = true;
        RefreshDayBlocks();
    }

    /// <summary>
    /// Reacts to day selection changes and reloads editable blocks for the selected day.
    /// </summary>
    /// <param name="value">Newly selected day name.</param>
    /// <returns>None.</returns>
    partial void OnSelectedDayChanged(string value)
    {
        if (!_isMasterScheduleLoaded)
            return;

        RefreshDayBlocks();
    }

    /// <summary>
    /// Rebuilds the day block editor collection from the currently selected schedule day.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: clears and repopulates <see cref="DayBlocks"/> and persists edits via per-item callbacks.
    /// </remarks>
    private void RefreshDayBlocks()
    {
        DayBlocks.Clear();
        if (_masterSchedule.TryGetValue(SelectedDay, out var blocks))
        {
            foreach (var b in blocks)
            {
                // Each editor item writes through to the shared schedule dictionary.
                DayBlocks.Add(new TemplateBlockEditor(b, PersistMasterScheduleChangesAsync));
            }
        }
    }

    /// <summary>
    /// Saves the edited master schedule and notifies listeners to refresh from the updated snapshot.
    /// </summary>
    /// <returns>A task that completes after the schedule has been normalized and persisted.</returns>
    /// <remarks>
    /// Side effects: sorts edited day blocks, recomputes durations, writes schedule state, and sends a refresh message.
    /// </remarks>
    private async Task PersistMasterScheduleChangesAsync()
    {
        NormalizeDayBlocks(SelectedDay);
        await _persistence.SaveStateAsync("sched_v3", _masterSchedule).ConfigureAwait(false);
        WeakReferenceMessenger.Default.Send(new ScheduleChangedMessage(true));
    }

    /// <summary>
    /// Keeps a day's blocks in chronological order and recalculates inferred durations after time edits.
    /// </summary>
    /// <param name="dayName">The day whose blocks should be normalized.</param>
    /// <returns>None.</returns>
    private void NormalizeDayBlocks(string dayName)
    {
        if (!_masterSchedule.TryGetValue(dayName, out var blocks) || blocks.Count == 0)
            return;

        blocks.Sort(CompareBlocksByTime);

        for (int i = 0; i < blocks.Count - 1; i++)
        {
            if (!TimeSpan.TryParse(blocks[i].Time, out var current) ||
                !TimeSpan.TryParse(blocks[i + 1].Time, out var next))
            {
                continue;
            }

            if (next < current)
                next = next.Add(TimeSpan.FromDays(1));

            blocks[i].DurationMinutes = (int)(next - current).TotalMinutes;
        }

        if (blocks.Count > 0)
        {
            blocks[^1].DurationMinutes = (int)TimeSpan.FromHours(7.5).TotalMinutes;
        }
    }

    private static int CompareBlocksByTime(ScheduleBlock a, ScheduleBlock b)
    {
        bool aValid = TimeSpan.TryParse(a.Time, out var aTime);
        bool bValid = TimeSpan.TryParse(b.Time, out var bTime);

        if (aValid && bValid) return aTime.CompareTo(bTime);
        if (aValid) return -1;
        if (bValid) return 1;
        return 0;
    }

    /// <summary>
    /// Signs in to Supabase and uploads locally stored schedule state as a cloud backup.
    /// </summary>
    /// <returns>A task that completes after backup flow status is updated.</returns>
    /// <remarks>
    /// Side effects: mutates <see cref="SupabaseStatusMessage"/> and performs network I/O via sync service.
    /// </remarks>
    [RelayCommand]
    private async Task BackupAsync()
    {
        SupabaseStatusMessage = "Signing in...";
        bool authed = await _supabase.SignInAsync(SupabaseEmail, SupabasePassword);
        if (!authed) { SupabaseStatusMessage = "Auth failed."; return; }
        
        SupabaseStatusMessage = "Backing up...";
        bool ok = await _supabase.BackupDataAsync();
        SupabaseStatusMessage = ok ? "Backup successful!" : "Backup failed.";
    }

    /// <summary>
    /// Signs in to Supabase and restores remote schedule state into local storage.
    /// </summary>
    /// <returns>A task that completes after restore flow status is updated.</returns>
    /// <remarks>
    /// Side effects: mutates <see cref="SupabaseStatusMessage"/> and writes restored data through sync service.
    /// </remarks>
    [RelayCommand]
    private async Task RestoreAsync()
    {
        SupabaseStatusMessage = "Signing in...";
        bool authed = await _supabase.SignInAsync(SupabaseEmail, SupabasePassword);
        if (!authed) { SupabaseStatusMessage = "Auth failed."; return; }
        
        SupabaseStatusMessage = "Restoring...";
        bool ok = await _supabase.RestoreDataAsync();
        SupabaseStatusMessage = ok ? "Restore successful! Restart app." : "Restore failed.";
    }

    /// <summary>
    /// Persists notification-enabled setting when it changes.
    /// </summary>
    /// <param name="value">New notification toggle value.</param>
    /// <returns>None.</returns>
    partial void OnNotificationsEnabledChanged(bool value) => Preferences.Set("notif_enabled", value);
    /// <summary>
    /// Persists haptic-enabled setting when it changes.
    /// </summary>
    /// <param name="value">New haptic toggle value.</param>
    /// <returns>None.</returns>
    partial void OnHapticEnabledChanged(bool value)        => Preferences.Set("haptic_enabled", value);
    /// <summary>
    /// Persists font size selection and applies it globally to the application resources.
    /// </summary>
    /// <param name="value">New font size token (Small, Medium, Large).</param>
    /// <returns>None.</returns>
    partial void OnSelectedFontSizeChanged(string value)
    {
        Preferences.Set("font_size", value);
        App.ApplyGlobalFontSize(value);
    }

    /// <summary>
    /// Opens the platform alarm sound picker and persists the chosen URI and display name.
    /// </summary>
    /// <returns>A task that completes after the picker result is saved.</returns>
    /// <remarks>
    /// Side effects: writes "alarm_sound_uri" preference and updates <see cref="SelectedAlarmSoundName"/>.
    /// </remarks>
    [RelayCommand]
    private async Task PickAlarmSoundAsync()
    {
        var uri = await _alarmSoundPicker.PickAlarmSoundAsync();
        if (uri == null) return; // user cancelled

        Preferences.Set("alarm_sound_uri", uri);
        SelectedAlarmSoundName = _alarmSoundPicker.GetSoundName(uri);
    }

    /// <summary>
    /// Persists selected theme and applies the corresponding runtime app theme.
    /// </summary>
    /// <param name="value">Theme token (Light, Dark, or System).</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: writes preferences and mutates <see cref="Application.UserAppTheme"/>.
    /// </remarks>
    partial void OnSelectedThemeChanged(string value)
    {
        Preferences.Set("theme", value);
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = value switch
            {
                "Light" => AppTheme.Light,
                "System" => AppTheme.Unspecified,
                _ => AppTheme.Dark
            };
        }
    }

    /// <summary>
    /// Confirms and clears all preference-based application data.
    /// </summary>
    /// <returns>A task that completes once the confirmation flow and clear operation finish.</returns>
    /// <remarks>
    /// Side effects: may clear all preference keys for the application.
    /// </remarks>
    [RelayCommand]
    private async Task ResetAllDataAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert("Reset All Data",
            "This will clear all your completion history, streaks, and check-ins. Are you sure?",
            "Reset", "Cancel");
        if (confirm)
        {
            Preferences.Clear();
        }
    }
}

public partial class TemplateBlockEditor : ObservableObject
{
    private const int SaveDebounceMilliseconds = 300;

    private readonly ScheduleBlock _block;
    private readonly Func<Task> _onChanged;
    private CancellationTokenSource? _saveDebounceCts;

    public string Label => _block.Label;
    public string Icon => _block.Icon;
    public string Category => _block.Category;

    [ObservableProperty]
    private TimeSpan _timeValue;

    /// <summary>
    /// Creates an editor wrapper for a single schedule block within the settings master schedule editor.
    /// </summary>
    /// <param name="block">Underlying schedule block being edited.</param>
    /// <param name="onChanged">Callback invoked whenever edited values should be persisted.</param>
    /// <remarks>
    /// Side effects: initializes editor state from block time and retains callback to trigger persistence.
    /// </remarks>
    public TemplateBlockEditor(ScheduleBlock block, Func<Task> onChanged)
    {
        _block = block;
        _onChanged = onChanged;
        if (TimeSpan.TryParse(block.Time, out var ts))
            _timeValue = ts;
    }

    /// <summary>
    /// Writes the edited time back into the schedule block and broadcasts a schedule-changed message.
    /// </summary>
    /// <param name="value">New time selected in the editor.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: mutates block time, invokes persistence callback, and sends a global refresh message.
    /// </remarks>
    partial void OnTimeValueChanged(TimeSpan value)
    {
        _block.Time = value.ToString(@"hh\:mm");

        _saveDebounceCts?.Cancel();
        _saveDebounceCts?.Dispose();
        _saveDebounceCts = new CancellationTokenSource();

        _ = NotifyChangedAsync(_saveDebounceCts.Token);
    }

    private async Task NotifyChangedAsync(CancellationToken cancellationToken)
    {
        try
        {
            // TimePicker emits rapid change events while scrolling; debounce to one persisted write.
            await Task.Delay(SaveDebounceMilliseconds, cancellationToken);

            if (_onChanged != null)
            {
                await _onChanged.Invoke().ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the user keeps scrolling and a newer selection supersedes this one.
        }
    }
}
