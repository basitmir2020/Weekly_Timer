namespace WeeklyTimetable.Models;

public class DaySchedule
{
    public string DayName { get; set; } // e.g., "Monday"
    public List<ScheduleBlock> Blocks { get; set; } = new();
}
