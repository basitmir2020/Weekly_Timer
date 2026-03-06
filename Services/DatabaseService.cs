using SQLite;
using WeeklyTimetable.Models;

namespace WeeklyTimetable.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "weeklyblueprint.db3");
    }

    public async Task InitializeAsync()
    {
        if (_connection != null) return;
        _connection = new SQLiteAsyncConnection(_dbPath);
        await _connection.CreateTableAsync<StreakRecord>();
        await _connection.CreateTableAsync<DailyCheckIn>();
        await _connection.CreateTableAsync<WeeklyGoal>();
    }

    private async Task EnsureReady()
    {
        if (_connection == null) await InitializeAsync();
    }

    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await EnsureReady();
        return await _connection!.Table<T>().ToListAsync();
    }

    public async Task<T> GetByIdAsync<T>(object id) where T : new()
    {
        await EnsureReady();
        return await _connection!.GetAsync<T>(id);
    }

    public async Task<int> SaveItemAsync<T>(T item) where T : new()
    {
        await EnsureReady();
        return await _connection!.InsertOrReplaceAsync(item);
    }

    public async Task<int> DeleteItemAsync<T>(T item) where T : new()
    {
        await EnsureReady();
        return await _connection!.DeleteAsync(item);
    }

    public async Task<DailyCheckIn?> GetCheckInAsync(DateTime date)
    {
        await EnsureReady();
        return await _connection!.Table<DailyCheckIn>()
            .Where(c => c.Date == date.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveCheckInAsync(DailyCheckIn entry)
    {
        await EnsureReady();
        entry.Date = entry.Date.Date;
        return await _connection!.InsertOrReplaceAsync(entry);
    }

    public async Task<List<DailyCheckIn>> GetRecentCheckInsAsync(int days)
    {
        await EnsureReady();
        var cutoff = DateTime.Today.AddDays(-days);
        return await _connection!.Table<DailyCheckIn>()
            .Where(c => c.Date >= cutoff)
            .OrderByDescending(c => c.Date)
            .ToListAsync();
    }

    public async Task<WeeklyGoal?> GetWeeklyGoalAsync(string weekStart)
    {
        await EnsureReady();
        return await _connection!.Table<WeeklyGoal>()
            .Where(g => g.WeekStartDate == weekStart)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveWeeklyGoalAsync(WeeklyGoal goal)
    {
        await EnsureReady();
        return await _connection!.InsertOrReplaceAsync(goal);
    }
}
