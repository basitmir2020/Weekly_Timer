using SQLite;

namespace WeeklyTimetable.Models;

public class WeeklyReflection
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public string WeekStartDate { get; set; } = string.Empty;
    
    public string Wins { get; set; } = string.Empty;
    public string Improvements { get; set; } = string.Empty;
    public int WeeklyScore { get; set; } // 1-10
}
