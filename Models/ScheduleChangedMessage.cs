using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WeeklyTimetable.Models;

public class ScheduleChangedMessage : ValueChangedMessage<bool>
{
    /// <summary>
    /// Creates a schedule-changed message used for cross-view-model refresh notifications.
    /// </summary>
    /// <param name="value">Boolean flag indicating whether subscribers should refresh.</param>
    public ScheduleChangedMessage(bool value) : base(value)
    {
    }
}
