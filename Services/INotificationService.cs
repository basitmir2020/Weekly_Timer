namespace WeeklyTimetable.Services;

public interface INotificationService
{
    Task<bool> RequestPermissionsAsync();
    Task ScheduleNotificationAsync(string title, string message, DateTime scheduleTime, int notificationId);
    Task CancelScheduledNotificationAsync(int notificationId);
    Task CancelAllNotificationsAsync();
}
