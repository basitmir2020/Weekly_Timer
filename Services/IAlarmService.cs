namespace WeeklyTimetable.Services;

/// <summary>
/// Provides looping alarm audio and vibration that continues until the user manually stops it.
/// </summary>
public interface IAlarmService
{
    /// <summary>Gets whether the alarm is currently ringing.</summary>
    bool IsAlarmRinging { get; }

    /// <summary>
    /// Starts a looping alarm sound and vibration pattern.
    /// Safe to call when already ringing — no-op if alarm is active.
    /// </summary>
    void StartAlarm();

    /// <summary>
    /// Stops the alarm immediately.
    /// Calling when not ringing is a no-op.
    /// </summary>
    void StopAlarm();

    /// <summary>
    /// Plays the user's selected alarm sound once (non-looping).
    /// Used for transient alerts like timer completion.
    /// </summary>
    void PlayFocusEndSound();
}
