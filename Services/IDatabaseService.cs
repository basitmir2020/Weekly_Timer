using WeeklyTimetable.Models;

namespace WeeklyTimetable.Services;

public interface IDatabaseService
{
    /// <summary>
    /// Initializes database connection and ensures required schema exists.
    /// </summary>
    /// <returns>A task that completes when initialization finishes.</returns>
    Task InitializeAsync();

    // Generic CRUD
    /// <summary>
    /// Retrieves all rows for the specified model type.
    /// </summary>
    /// <typeparam name="T">Table-mapped model type.</typeparam>
    /// <returns>All persisted rows for <typeparamref name="T"/>.</returns>
    Task<List<T>> GetAllAsync<T>() where T : new();
    /// <summary>
    /// Loads a row by primary key for the specified model type.
    /// </summary>
    /// <typeparam name="T">Table-mapped model type.</typeparam>
    /// <param name="id">Primary key value.</param>
    /// <returns>Matching entity.</returns>
    Task<T> GetByIdAsync<T>(object id) where T : new();
    /// <summary>
    /// Inserts or updates a row for the specified model type.
    /// </summary>
    /// <typeparam name="T">Table-mapped model type.</typeparam>
    /// <param name="item">Entity instance to save.</param>
    /// <returns>Rows affected count.</returns>
    Task<int> SaveItemAsync<T>(T item) where T : new();
    /// <summary>
    /// Deletes a persisted row for the specified model type.
    /// </summary>
    /// <typeparam name="T">Table-mapped model type.</typeparam>
    /// <param name="item">Entity instance to delete.</param>
    /// <returns>Rows affected count.</returns>
    Task<int> DeleteItemAsync<T>(T item) where T : new();

    // Specific helpers
    /// <summary>
    /// Gets the daily check-in for the requested date.
    /// </summary>
    /// <param name="date">Date to search.</param>
    /// <returns>Check-in record or <c>null</c>.</returns>
    Task<DailyCheckIn?> GetCheckInAsync(DateTime date);
    /// <summary>
    /// Saves a daily check-in entry.
    /// </summary>
    /// <param name="entry">Entry to persist.</param>
    /// <returns>Rows affected count.</returns>
    Task<int> SaveCheckInAsync(DailyCheckIn entry);
    /// <summary>
    /// Returns recent check-ins constrained by lookback window.
    /// </summary>
    /// <param name="days">Number of days to look back.</param>
    /// <returns>List of recent check-in rows.</returns>
    Task<List<DailyCheckIn>> GetRecentCheckInsAsync(int days);

    /// <summary>
    /// Retrieves the weekly goal record for a given week key.
    /// </summary>
    /// <param name="weekStart">Week start key.</param>
    /// <returns>Weekly goal or <c>null</c>.</returns>
    Task<WeeklyGoal?> GetWeeklyGoalAsync(string weekStart);
    /// <summary>
    /// Saves a weekly goal record.
    /// </summary>
    /// <param name="goal">Weekly goal entity to persist.</param>
    /// <returns>Rows affected count.</returns>
    Task<int> SaveWeeklyGoalAsync(WeeklyGoal goal);
}
