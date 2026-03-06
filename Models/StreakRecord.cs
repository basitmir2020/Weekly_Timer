using SQLite;

namespace WeeklyTimetable.Models;

public class StreakRecord
{
    [PrimaryKey]
    public DateTime DateId { get; set; }    // The exact date
    public bool IsCompleted { get; set; }   // Whether the full day was completed
    public int CompletionPct { get; set; }  // Snapshot of % when saved
}
