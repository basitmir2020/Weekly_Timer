using Plugin.LocalNotification;

namespace WeeklyTimetable.Services;

public class NotificationService : INotificationService
{
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

    public Task ScheduleNotificationAsync(string title, string message, DateTime scheduleTime, int notificationId)
    {
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

    public Task CancelScheduledNotificationAsync(int notificationId)
    {
        LocalNotificationCenter.Current.Cancel(notificationId);
        return Task.CompletedTask;
    }

    public Task CancelAllNotificationsAsync()
    {
        LocalNotificationCenter.Current.CancelAll();
        return Task.CompletedTask;
    }
}
