namespace WeeklyTimetable.Services;

/// <summary>
/// Stub implementation of <see cref="IAlarmSchedulerService"/> for non-Android platforms.
/// </summary>
public sealed class StubAlarmSchedulerService : IAlarmSchedulerService
{
    public void ScheduleAlarm(int id, DateTime time, string title, string message) { }
    public void CancelAlarm(int id) { }
    public void CancelAll() { }
}
