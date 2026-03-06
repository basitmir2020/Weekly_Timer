using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISupabaseSyncService _supabase;

    [ObservableProperty] private bool _notificationsEnabled;
    [ObservableProperty] private bool _hapticEnabled = true;
    [ObservableProperty] private string _selectedTheme = "Dark";
    [ObservableProperty] private string _selectedFontSize = "Medium";
    
    // Supabase Bindings
    [ObservableProperty] private string _supabaseEmail = "";
    [ObservableProperty] private string _supabasePassword = "";
    [ObservableProperty] private string _supabaseStatusMessage = "";

    public List<string> Themes { get; } = new() { "Dark", "Light", "System" };
    public List<string> FontSizes { get; } = new() { "Small", "Medium", "Large" };

    public SettingsViewModel(ISupabaseSyncService supabase)
    {
        _supabase = supabase;
        NotificationsEnabled = Preferences.Get("notif_enabled", true);
        HapticEnabled        = Preferences.Get("haptic_enabled", true);
        SelectedTheme        = Preferences.Get("theme", "Dark");
        SelectedFontSize     = Preferences.Get("font_size", "Medium");
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
