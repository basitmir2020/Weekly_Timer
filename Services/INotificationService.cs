namespace WeeklyTimetable.Services;

public interface INotificationService
{
    /// <summary>
    /// Ensures notification permissions are granted for the current platform.
    /// </summary>
    /// <returns><c>true</c> when notifications can be scheduled; otherwise <c>false</c>.</returns>
    Task<bool> RequestPermissionsAsync();
    /// <summary>
    /// Schedules a single local notification.
    /// </summary>
    /// <param name="title">Notification title text.</param>
    /// <param name="message">Notification body text.</param>
    /// <param name="scheduleTime">Exact local time when notification should fire.</param>
    /// <param name="notificationId">Unique identifier for cancellation/replacement.</param>
    /// <returns>A task that completes after the request is submitted.</returns>
    Task ScheduleNotificationAsync(string title, string message, DateTime scheduleTime, int notificationId);
    /// <summary>
    /// Cancels a previously scheduled notification by identifier.
    /// </summary>
    /// <param name="notificationId">Notification identifier to cancel.</param>
    /// <returns>A task that completes after cancellation request.</returns>
    Task CancelScheduledNotificationAsync(int notificationId);
    /// <summary>
    /// Cancels all scheduled notifications for the app.
    /// </summary>
    /// <returns>A task that completes after cancellation request.</returns>
    Task CancelAllNotificationsAsync();
}
