namespace WeeklyTimetable.Models;

public class ScheduleProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Unique identifier
    public string Name { get; set; }                      // e.g. "Default", "Travel", "Exam Mode"
    public bool IsActive { get; set; }                    // Only one profile active at a time
    public Dictionary<string, List<ScheduleBlock>> Days { get; set; } // Full 7-day schedule
}
