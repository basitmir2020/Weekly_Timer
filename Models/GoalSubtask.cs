using SQLite;

namespace WeeklyTimetable.Models;

public class GoalSubtask
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public int GoalItemId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
}
