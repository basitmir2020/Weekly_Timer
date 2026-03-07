using Microsoft.Maui.ApplicationModel;
using System;
using System.Threading.Tasks;
using WeeklyTimetable.Models;
using System.Collections.Generic;

namespace WeeklyTimetable.Services;

public interface INativeCalendarSyncService
{
    /// <summary>
    /// Attempts to synchronize today's schedule blocks into the device calendar.
    /// </summary>
    /// <param name="blocks">Schedule blocks to export as calendar entries.</param>
    /// <returns><c>true</c> when sync operation completes without error; otherwise <c>false</c>.</returns>
    Task<bool> SyncTodayScheduleAsync(IEnumerable<ScheduleBlock> blocks);
}

public class NativeCalendarSyncService : INativeCalendarSyncService
{
    /// <summary>
    /// Requests calendar permission and iterates through blocks to prepare calendar event payloads.
    /// </summary>
    /// <param name="blocks">Schedule blocks to sync for today.</param>
    /// <returns><c>true</c> on success path; <c>false</c> when permission is denied or an error occurs.</returns>
    /// <remarks>
    /// Side effects: may trigger OS permission prompt; current implementation does not persist events yet.
    /// </remarks>
    public async Task<bool> SyncTodayScheduleAsync(IEnumerable<ScheduleBlock> blocks)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.CalendarWrite>();
        }

        if (status != PermissionStatus.Granted)
            return false;

        // In a full implementation, we would use native Android/iOS intents or Plugin.Maui.CalendarStore 
        // to insert the events. Here we simulate the platform-specific calendar binding.
        
        try
        {
            foreach(var block in blocks)
            {
                // Parse time
                if (DateTime.TryParseExact(block.Time, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
                {
                    // Compute start/end timestamps for a future platform-specific event insert call.
                    var start = DateTime.Today.Add(parsedTime.TimeOfDay);
                    var end = start.AddMinutes(block.DurationMinutes > 0 ? block.DurationMinutes : 60);

                    // Insert logic using native platform hooks would go here.
                    // string eventTitle = $"{block.Icon} {block.Label}";
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
