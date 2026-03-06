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
    public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
    {
        foreach (var widgetId in appWidgetIds)
        {
            UpdateAppWidget(context, appWidgetManager, widgetId);
        }
    }

    private void UpdateAppWidget(Context context, AppWidgetManager appWidgetManager, int appWidgetId)
    {
        var views = new RemoteViews(context.PackageName, Resource.Layout.widget_layout);
        
        views.SetTextViewText(Resource.Id.widgetLabel, "Keep the streak alive! 🔥");

        var intent = new Intent(context, typeof(MainActivity));
        var flags = Build.VERSION.SdkInt >= BuildVersionCodes.M 
            ? PendingIntentFlags.Immutable 
            : PendingIntentFlags.UpdateCurrent;
        var pendingIntent = PendingIntent.GetActivity(context, 0, intent, flags);
        views.SetOnClickPendingIntent(Resource.Id.widgetRoot, pendingIntent);

        appWidgetManager.UpdateAppWidget(appWidgetId, views);
    }
}
