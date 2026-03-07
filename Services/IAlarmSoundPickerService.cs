namespace WeeklyTimetable.Services;

/// <summary>
/// Provides a platform-specific alarm sound picker and display-name resolver.
/// </summary>
public interface IAlarmSoundPickerService
{
    /// <summary>
    /// Opens the native OS ringtone/alarm sound picker.
    /// </summary>
    /// <returns>
    /// The URI string of the chosen sound, or <c>null</c> when the user cancels.
    /// </returns>
    Task<string?> PickAlarmSoundAsync();

    /// <summary>
    /// Returns a human-readable display name for the given URI.
    /// Falls back to "Default" when <paramref name="uri"/> is null or unresolvable.
    /// </summary>
    string GetSoundName(string? uri);
}
