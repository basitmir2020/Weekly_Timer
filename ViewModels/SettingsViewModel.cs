using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISupabaseSyncService _supabase;
    private readonly IPersistenceService _persistence;

    [ObservableProperty] private bool _notificationsEnabled;
    [ObservableProperty] private bool _hapticEnabled = true;
    [ObservableProperty] private string _selectedTheme = "Dark";
    [ObservableProperty] private string _selectedFontSize = "Medium";
    
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

    public SettingsViewModel(ISupabaseSyncService supabase, IPersistenceService persistence)
    {
        _supabase = supabase;
        _persistence = persistence;
        NotificationsEnabled = Preferences.Get("notif_enabled", true);
        HapticEnabled        = Preferences.Get("haptic_enabled", true);
        SelectedTheme        = Preferences.Get("theme", "Dark");
        SelectedFontSize     = Preferences.Get("font_size", "Medium");

        _ = LoadMasterScheduleAsync();
    }

    private async Task LoadMasterScheduleAsync()
    {
        var sched = await _persistence.LoadStateAsync<Dictionary<string, List<ScheduleBlock>>>("sched_v3");
        if (sched != null && sched.Count > 0)
        {
            _masterSchedule = sched;
        }
        else
        {
            _masterSchedule = WeeklyTimetable.Data.ScheduleData.GetDefaultSchedule();
        }
        RefreshDayBlocks();
    }

    partial void OnSelectedDayChanged(string value)
    {
        RefreshDayBlocks();
    }

    private void RefreshDayBlocks()
    {
        DayBlocks.Clear();
        if (_masterSchedule.TryGetValue(SelectedDay, out var blocks))
        {
            foreach (var b in blocks)
            {
                DayBlocks.Add(new TemplateBlockEditor(b, async () => 
                {
                    await _persistence.SaveStateAsync("sched_v3", _masterSchedule);
                }));
            }
        }
    }

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

    partial void OnNotificationsEnabledChanged(bool value) => Preferences.Set("notif_enabled", value);
    partial void OnHapticEnabledChanged(bool value)        => Preferences.Set("haptic_enabled", value);
    partial void OnSelectedFontSizeChanged(string value)   => Preferences.Set("font_size", value);

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
    private readonly ScheduleBlock _block;
    private readonly Action _onChanged;

    public string Label => _block.Label;
    public string Icon => _block.Icon;
    public string Category => _block.Category;

    [ObservableProperty]
    private TimeSpan _timeValue;

    public TemplateBlockEditor(ScheduleBlock block, Action onChanged)
    {
        _block = block;
        _onChanged = onChanged;
        if (TimeSpan.TryParse(block.Time, out var ts))
            _timeValue = ts;
    }

    partial void OnTimeValueChanged(TimeSpan value)
    {
        _block.Time = value.ToString(@"hh\:mm");
        _onChanged?.Invoke();
    }
}
