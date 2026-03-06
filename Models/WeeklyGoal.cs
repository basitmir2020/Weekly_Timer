using SQLite;

namespace WeeklyTimetable.Models;

public class WeeklyGoal
{
    [PrimaryKey]
    public string WeekStartDate { get; set; } = string.Empty;  // ISO date e.g. "2026-03-02"
    public string DSATopic { get; set; } = string.Empty;
    public string WebDevFeature { get; set; } = string.Empty;
    public string HabitFocus { get; set; } = string.Empty;
    public bool DSADone { get; set; }
    public bool WebDevDone { get; set; }
    public bool HabitDone { get; set; }
}
