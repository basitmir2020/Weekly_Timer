using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WeeklyTimetable.Models;

public class ScheduleChangedMessage : ValueChangedMessage<bool>
{
    public ScheduleChangedMessage(bool value) : base(value)
    {
    }
}
