using Plugin.LocalNotification;

namespace WeeklyTimetable.Services;

public class NotificationService : INotificationService
{
    /// <summary>
    /// Checks and requests local-notification permissions from the OS.
    /// </summary>
    /// <returns><c>true</c> when notifications are enabled or assumed available after fallback.</returns>
    /// <remarks>
    /// Side effects: may display an OS permission prompt.
    /// </remarks>
    public async Task<bool> RequestPermissionsAsync()
    {
        try
        {
            // RequestNotificationPermissionAsync is available on the concrete instance
            var granted = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if (!granted)
            {
                granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
            return granted;
        }
        catch
        {
            return true; // Fallback — assume granted so schedule proceeds
        }
    }

    /// <summary>
    /// Builds and schedules a local notification for a specific time.
    /// </summary>
    /// <param name="title">Notification title text.</param>
    /// <param name="message">Notification body text.</param>
    /// <param name="scheduleTime">Local device date/time to notify.</param>
    /// <param name="notificationId">Unique notification identifier.</param>
    /// <returns>A completed task after request submission.</returns>
    /// <remarks>
    /// Side effects: registers a scheduled notification with platform notification center.
    /// </remarks>
    public Task ScheduleNotificationAsync(string title, string message, DateTime scheduleTime, int notificationId)
    {
        // Respect the user's notification preference from settings
        if (!Preferences.Get("notif_enabled", true))
            return Task.CompletedTask;

        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title          = title,
            Description    = message,
            ReturningData  = "WeeklyBlueprintTracker",
            CategoryType   = NotificationCategoryType.Alarm,
            Schedule       = new NotificationRequestSchedule { NotifyTime = scheduleTime }
        };

#if ANDROID
        request.Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions 
        { 
            Priority = Plugin.LocalNotification.AndroidOption.AndroidPriority.Max 
        };
#endif

        LocalNotificationCenter.Current.Show(request);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cancels one scheduled notification by id.
    /// </summary>
    /// <param name="notificationId">Notification identifier to cancel.</param>
    /// <returns>A completed task.</returns>
    /// <remarks>
    /// Side effects: removes scheduled notification from platform queue.
    /// </remarks>
    public Task CancelScheduledNotificationAsync(int notificationId)
    {
        LocalNotificationCenter.Current.Cancel(notificationId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cancels every scheduled notification for the app.
    /// </summary>
    /// <returns>A completed task.</returns>
    /// <remarks>
    /// Side effects: clears platform notification queue for this application.
    /// </remarks>
    public Task CancelAllNotificationsAsync()
    {
        LocalNotificationCenter.Current.CancelAll();
        return Task.CompletedTask;
    }
}
