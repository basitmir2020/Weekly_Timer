using SQLite;
using WeeklyTimetable.Models;

namespace WeeklyTimetable.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;

    /// <summary>
    /// Creates the SQLite database service and computes the database file path in app data storage.
    /// </summary>
    /// <remarks>
    /// Side effects: none until initialization methods are called.
    /// </remarks>
    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "weeklyblueprint.db3");
    }

    /// <summary>
    /// Initializes the SQLite connection and ensures required tables exist.
    /// </summary>
    /// <returns>A task that completes when table creation operations finish.</returns>
    /// <remarks>
    /// Side effects: creates database file/connection and schema tables if missing.
    /// </remarks>
    public async Task InitializeAsync()
    {
        if (_connection != null) return;
        _connection = new SQLiteAsyncConnection(_dbPath);
        await _connection.CreateTableAsync<StreakRecord>();
        await _connection.CreateTableAsync<DailyCheckIn>();
        await _connection.CreateTableAsync<WeeklyGoal>();
    }

    /// <summary>
    /// Guarantees that the database connection is initialized before any query/write operation runs.
    /// </summary>
    /// <returns>A task that completes when the connection is ready.</returns>
    private async Task EnsureReady()
    {
        if (_connection == null) await InitializeAsync();
    }

    /// <summary>
    /// Returns all rows for a table-backed model type.
    /// </summary>
    /// <typeparam name="T">Model type mapped to a SQLite table.</typeparam>
    /// <returns>List of all rows for the model type.</returns>
    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        await EnsureReady();
        return await _connection!.Table<T>().ToListAsync();
    }

    /// <summary>
    /// Loads a single item by primary key.
    /// </summary>
    /// <typeparam name="T">Model type mapped to a SQLite table.</typeparam>
    /// <param name="id">Primary key value.</param>
    /// <returns>Matching model instance.</returns>
    public async Task<T> GetByIdAsync<T>(object id) where T : new()
    {
        await EnsureReady();
        return await _connection!.GetAsync<T>(id);
    }

    /// <summary>
    /// Inserts or replaces a model record.
    /// </summary>
    /// <typeparam name="T">Model type mapped to a SQLite table.</typeparam>
    /// <param name="item">Entity to persist.</param>
    /// <returns>SQLite rows affected count.</returns>
    /// <remarks>
    /// Side effects: writes to local database.
    /// </remarks>
    public async Task<int> SaveItemAsync<T>(T item) where T : new()
    {
        await EnsureReady();
        return await _connection!.InsertOrReplaceAsync(item);
    }

    /// <summary>
    /// Deletes a model record.
    /// </summary>
    /// <typeparam name="T">Model type mapped to a SQLite table.</typeparam>
    /// <param name="item">Entity instance to delete.</param>
    /// <returns>SQLite rows affected count.</returns>
    /// <remarks>
    /// Side effects: removes data from local database.
    /// </remarks>
    public async Task<int> DeleteItemAsync<T>(T item) where T : new()
    {
        await EnsureReady();
        return await _connection!.DeleteAsync(item);
    }

    /// <summary>
    /// Retrieves the check-in record for a specific date.
    /// </summary>
    /// <param name="date">Date to query (date component is used).</param>
    /// <returns>Check-in record for the day, or <c>null</c> if none exists.</returns>
    public async Task<DailyCheckIn?> GetCheckInAsync(DateTime date)
    {
        await EnsureReady();
        return await _connection!.Table<DailyCheckIn>()
            .Where(c => c.Date == date.Date)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Saves a daily check-in after normalizing the stored value to date-only.
    /// </summary>
    /// <param name="entry">Check-in entry to create or update.</param>
    /// <returns>SQLite rows affected count.</returns>
    /// <remarks>
    /// Side effects: mutates <paramref name="entry"/> date and writes to local database.
    /// </remarks>
    public async Task<int> SaveCheckInAsync(DailyCheckIn entry)
    {
        await EnsureReady();
        // Normalize to midnight so comparisons and uniqueness are stable across time-of-day values.
        entry.Date = entry.Date.Date;
        return await _connection!.InsertOrReplaceAsync(entry);
    }

    /// <summary>
    /// Returns recent check-ins newer than the specified day window.
    /// </summary>
    /// <param name="days">Number of days to look back from today.</param>
    /// <returns>Descending date-ordered list of recent check-ins.</returns>
    public async Task<List<DailyCheckIn>> GetRecentCheckInsAsync(int days)
    {
        await EnsureReady();
        var cutoff = DateTime.Today.AddDays(-days);
        return await _connection!.Table<DailyCheckIn>()
            .Where(c => c.Date >= cutoff)
            .OrderByDescending(c => c.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the weekly goal record for a given week-start key.
    /// </summary>
    /// <param name="weekStart">Week start key formatted as <c>yyyy-MM-dd</c>.</param>
    /// <returns>Weekly goal record, or <c>null</c> when no row exists.</returns>
    public async Task<WeeklyGoal?> GetWeeklyGoalAsync(string weekStart)
    {
        await EnsureReady();
        return await _connection!.Table<WeeklyGoal>()
            .Where(g => g.WeekStartDate == weekStart)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Saves a weekly goal record.
    /// </summary>
    /// <param name="goal">Goal entity to create or update.</param>
    /// <returns>SQLite rows affected count.</returns>
    /// <remarks>
    /// Side effects: writes weekly goal data to local database.
    /// </remarks>
    public async Task<int> SaveWeeklyGoalAsync(WeeklyGoal goal)
    {
        await EnsureReady();
        return await _connection!.InsertOrReplaceAsync(goal);
    }
}
