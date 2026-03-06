using WeeklyTimetable.Models;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Services;

public interface IStreakService
{
    Task CheckAndAwardStreakAsync(bool isDayCelebrated, int completionPct = 100);
    Task<int> GetCurrentStreakAsync();
    Task<int> GetLongestStreakAsync();
    Task<List<StreakDayEntry>> GetLast30DaysAsync();
}

public class StreakService : IStreakService
{
    private readonly IDatabaseService _db;

    public StreakService(IDatabaseService db) => _db = db;

    public async Task CheckAndAwardStreakAsync(bool isDayCelebrated, int completionPct = 100)
    {
        var today = DateTime.Today;

        if (isDayCelebrated)
        {
            await _db.SaveItemAsync(new StreakRecord
            {
                DateId        = today,
                IsCompleted   = true,
                CompletionPct = completionPct
            });
            return;
        }

        var all = await _db.GetAllAsync<StreakRecord>();
        var todayRecord = all.FirstOrDefault(r => r.DateId.Date == today);
        if (todayRecord != null)
        {
            await _db.DeleteItemAsync(todayRecord);
        }
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        try
        {
            var all    = await _db.GetAllAsync<StreakRecord>();
            var sorted = all.Where(s => s.IsCompleted).OrderByDescending(s => s.DateId).ToList();
            if (!sorted.Any()) return 0;

            int streak = 0;
            var check  = DateTime.Today;
            if (sorted[0].DateId.Date != check) check = check.AddDays(-1);

            foreach (var r in sorted)
            {
                if (r.DateId.Date == check) { streak++; check = check.AddDays(-1); }
                else break;
            }
            return streak;
        }
        catch { return 0; }
    }

    public async Task<int> GetLongestStreakAsync()
    {
        try
        {
            var all    = await _db.GetAllAsync<StreakRecord>();
            var dates  = all.Where(r => r.IsCompleted).Select(r => r.DateId.Date).OrderBy(d => d).ToList();
            if (!dates.Any()) return 0;

            int max = 1, current = 1;
            for (int i = 1; i < dates.Count; i++)
            {
                if (dates[i] == dates[i - 1].AddDays(1)) { current++; max = Math.Max(max, current); }
                else current = 1;
            }
            return max;
        }
        catch { return 0; }
    }

    public async Task<List<StreakDayEntry>> GetLast30DaysAsync()
    {
        try
        {
            var all = await _db.GetAllAsync<StreakRecord>();
            var result = new List<StreakDayEntry>();
            for (int i = 29; i >= 0; i--)
            {
                var date   = DateTime.Today.AddDays(-i);
                var record = all.FirstOrDefault(r => r.DateId.Date == date);
                result.Add(new StreakDayEntry { Date = date, IsComplete = record?.IsCompleted == true });
            }
            return result;
        }
        catch { return new List<StreakDayEntry>(); }
    }
}
