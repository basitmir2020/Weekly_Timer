using SQLite;

namespace WeeklyTimetable.Models;

public class HabitCommitment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public string WeekStartDate { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    public int TargetFrequency { get; set; } = 7; // days per week
    public int CompletedCount { get; set; }
    
    // JSON or comma-separated string for daily checks (e.g. "1,0,1,1,0,0,1")
    public string DailyChecks { get; set; } = "0,0,0,0,0,0,0";
    
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
}
