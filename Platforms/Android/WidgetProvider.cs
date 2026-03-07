using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Widget;
using Microsoft.Maui.Storage;

namespace WeeklyTimetable.Platforms.Android;

[BroadcastReceiver(Exported = true, Label = "Weekly Blueprint")]
[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
[MetaData("android.appwidget.provider", Resource = "@xml/widget_info")]
public class WidgetProvider : AppWidgetProvider
{
    /// <summary>
    /// Handles Android widget update broadcasts and refreshes all widget instances.
    /// </summary>
    /// <param name="context">Android context.</param>
    /// <param name="appWidgetManager">Widget manager used for updates.</param>
    /// <param name="appWidgetIds">Widget instance ids to refresh.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: updates remote views for each app-widget instance.
    /// </remarks>
    public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
    {
        foreach (var widgetId in appWidgetIds)
        {
            UpdateAppWidget(context, appWidgetManager, widgetId);
        }
    }

    /// <summary>
    /// Builds and pushes the widget layout content for a single widget instance.
    /// </summary>
    /// <param name="context">Android context.</param>
    /// <param name="appWidgetManager">Widget manager used to apply updates.</param>
    /// <param name="appWidgetId">Target widget instance id.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: updates text and click action on home-screen widget.
    /// </remarks>
    private void UpdateAppWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
    {
        var views = new RemoteViews(context.PackageName, Resource.Layout.widget_layout);
        
        views.SetTextViewText(Resource.Id.widgetLabel, "Keep the streak alive! 🔥");

        var intent = new Intent(context, typeof(MainActivity));
        var flags = Build.VERSION.SdkInt >= BuildVersionCodes.M 
            ? PendingIntentFlags.Immutable 
            : PendingIntentFlags.UpdateCurrent;
        // Immutable flag is required on modern Android versions for secure pending intents.
        var pendingIntent = PendingIntent.GetActivity(context, 0, intent, flags);
        views.SetOnClickPendingIntent(Resource.Id.widgetRoot, pendingIntent);

        appWidgetManager.UpdateAppWidget(appWidgetId, views);
    }
}
