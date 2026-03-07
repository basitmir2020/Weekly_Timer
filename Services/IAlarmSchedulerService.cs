namespace WeeklyTimetable.Services;

/// <summary>
/// Provides platform-specific scheduling of hard alarms that can wake the device 
/// and start a foreground service even if the app process is killed.
/// </summary>
public interface IAlarmSchedulerService
{
    /// <summary>
    /// Schedules an exact alarm at the specified time.
    /// </summary>
    /// <param name="id">Unique identifier for this alarm.</param>
    /// <param name="time">Execution time (Local).</param>
    /// <param name="title">Title for the alarm notification.</param>
    /// <param name="message">Body text for the alarm notification.</param>
    void ScheduleAlarm(int id, DateTime time, string title, string message);

    /// <summary>
    /// Cancels a previously scheduled alarm by ID.
    /// </summary>
    void CancelAlarm(int id);

    /// <summary>
    /// Cancels all scheduled alarms and stops any active alarm service.
    /// </summary>
    void CancelAll();
}
