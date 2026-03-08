using SQLite;

namespace WeeklyTimetable.Models;

public class WeeklyGoalItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public string WeekStartDate { get; set; } = string.Empty; // ISO date e.g. "2026-03-02"
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
    public GoalType Type { get; set; } = GoalType.Project;
    
    public double ProgressPercent { get; set; } // 0.0 to 1.0 or 0 to 100
    
    public string DueDay { get; set; } = string.Empty; // e.g. "Friday"
    public string LinkedScheduleCategory { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
}
