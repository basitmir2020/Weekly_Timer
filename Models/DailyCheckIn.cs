using SQLite;

namespace WeeklyTimetable.Models;

public class DailyCheckIn
{
    [PrimaryKey]
    public DateTime Date { get; set; }
    public int MorningEnergy { get; set; }   // 1–5
    public int EveningMood { get; set; }     // 1–5
    public string? Notes { get; set; }
}
