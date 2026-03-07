using Android.App;
using Android.Content;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.Platforms.Android;

/// <summary>
/// Android implementation of <see cref="IAlarmSchedulerService"/> using AlarmManager.
/// </summary>
public sealed class AlarmSchedulerService : IAlarmSchedulerService
{
    private readonly Context _context;

    public AlarmSchedulerService()
    {
        _context = global::Android.App.Application.Context;
    }

    public void ScheduleAlarm(int id, DateTime time, string title, string message)
    {
        var intent = new Intent(_context, typeof(AlarmReceiver));
        intent.PutExtra("id", id);
        intent.PutExtra("title", title);
        intent.PutExtra("message", message);

        var pendingIntent = PendingIntent.GetBroadcast(_context, id, intent, 
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var alarmManager = _context.GetSystemService(Context.AlarmService) as AlarmManager;
        if (alarmManager == null) return;

        long triggerMs = new DateTimeOffset(time).ToUnixTimeMilliseconds();

        // Use SetExactAndAllowWhileIdle to ensure it fires even in Doze mode.
        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.M)
        {
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerMs, pendingIntent);
        }
        else
        {
            alarmManager.SetExact(AlarmType.RtcWakeup, triggerMs, pendingIntent);
        }
    }

    public void CancelAlarm(int id)
    {
        var intent = new Intent(_context, typeof(AlarmReceiver));
        var pendingIntent = PendingIntent.GetBroadcast(_context, id, intent, 
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        var alarmManager = _context.GetSystemService(Context.AlarmService) as AlarmManager;
        alarmManager?.Cancel(pendingIntent);
    }

    public void CancelAll()
    {
        // Stopping the service if it's running
        var stopIntent = new Intent(_context, typeof(AlarmForegroundService));
        _context.StopService(stopIntent);
    }
}
