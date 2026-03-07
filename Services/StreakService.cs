using WeeklyTimetable.Models;
using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Services;

public interface IStreakService
{
    /// <summary>
    /// Awards or revokes today's streak entry based on current completion state.
    /// </summary>
    /// <param name="isDayCelebrated">Whether the day is considered fully completed.</param>
    /// <param name="completionPct">Completion percent saved with the streak record.</param>
    /// <returns>A task that completes after streak state is updated.</returns>
    Task CheckAndAwardStreakAsync(bool isDayCelebrated, int completionPct = 100);
    /// <summary>
    /// Computes the current consecutive completed-day streak.
    /// </summary>
    /// <returns>Current streak length in days.</returns>
    Task<int> GetCurrentStreakAsync();
    /// <summary>
    /// Computes the longest historical consecutive completed-day streak.
    /// </summary>
    /// <returns>Longest streak length in days.</returns>
    Task<int> GetLongestStreakAsync();
    /// <summary>
    /// Returns completion markers for the last 30 days.
    /// </summary>
    /// <returns>List of day entries used for streak heatmap/dot views.</returns>
    Task<List<StreakDayEntry>> GetLast30DaysAsync();
}

public class StreakService : IStreakService
{
    private readonly IDatabaseService _db;

    /// <summary>
    /// Creates the streak service with database access dependency.
    /// </summary>
    /// <param name="db">Database service used to read/write streak records.</param>
    public StreakService(IDatabaseService db) => _db = db;

    /// <summary>
    /// Saves today's streak completion when a day is celebrated, otherwise removes today's existing record.
    /// </summary>
    /// <param name="isDayCelebrated">Whether the day should count as completed for streak purposes.</param>
    /// <param name="completionPct">Completion percentage stored with the record.</param>
    /// <returns>A task that completes after database write/delete finishes.</returns>
    /// <remarks>
    /// Side effects: inserts, updates, or deletes today's streak record.
    /// </remarks>
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
            // Keep streak storage aligned with current day completion when completion is revoked.
            await _db.DeleteItemAsync(todayRecord);
        }
    }

    /// <summary>
    /// Calculates the current streak by walking backward day-by-day from today (or yesterday when today is incomplete).
    /// </summary>
    /// <returns>Current consecutive streak length.</returns>
    public async Task<int> GetCurrentStreakAsync()
    {
        try
        {
            // Performance note: for large histories, a date-filtered query would avoid loading all rows.
            var all    = await _db.GetAllAsync<StreakRecord>();
            var sorted = all.Where(s => s.IsCompleted).OrderByDescending(s => s.DateId).ToList();
            if (!sorted.Any()) return 0;

            int streak = 0;
            var check  = DateTime.Today;
            if (sorted[0].DateId.Date != check) check = check.AddDays(-1);

            foreach (var r in sorted)
            {
                // Count only contiguous dates; stop immediately on the first gap.
                if (r.DateId.Date == check) { streak++; check = check.AddDays(-1); }
                else break;
            }
            return streak;
        }
        catch { return 0; }
    }

    /// <summary>
    /// Calculates the historical maximum streak from all completed streak records.
    /// </summary>
    /// <returns>Longest consecutive streak length.</returns>
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
                // Increment while dates remain consecutive; otherwise restart current run length.
                if (dates[i] == dates[i - 1].AddDays(1)) { current++; max = Math.Max(max, current); }
                else current = 1;
            }
            return max;
        }
        catch { return 0; }
    }

    /// <summary>
    /// Builds a fixed 30-day history list including missing days as incomplete entries.
    /// </summary>
    /// <returns>Chronological list from oldest (30 days ago) to today.</returns>
    public async Task<List<StreakDayEntry>> GetLast30DaysAsync()
    {
        try
        {
            var all = await _db.GetAllAsync<StreakRecord>();
            var result = new List<StreakDayEntry>();
            for (int i = 29; i >= 0; i--)
            {
                // Fill every day explicitly so charts have stable spacing even when records are sparse.
                var date   = DateTime.Today.AddDays(-i);
                var record = all.FirstOrDefault(r => r.DateId.Date == date);
                result.Add(new StreakDayEntry { Date = date, IsComplete = record?.IsCompleted == true });
            }
            return result;
        }
        catch { return new List<StreakDayEntry>(); }
    }
}
