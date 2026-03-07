namespace WeeklyTimetable.Services;

public interface IPersistenceService
{
    /// <summary>
    /// Serializes and stores state data under a key.
    /// </summary>
    /// <typeparam name="T">Type of data being persisted.</typeparam>
    /// <param name="key">Storage key.</param>
    /// <param name="data">Data instance to save.</param>
    /// <returns>A task that completes after data is written.</returns>
    Task SaveStateAsync<T>(string key, T data);
    /// <summary>
    /// Loads and deserializes state data by key.
    /// </summary>
    /// <typeparam name="T">Expected data type.</typeparam>
    /// <param name="key">Storage key.</param>
    /// <returns>Deserialized state object or default value when absent/invalid.</returns>
    Task<T?> LoadStateAsync<T>(string key);
    /// <summary>
    /// Removes a single persisted key/value pair.
    /// </summary>
    /// <param name="key">Storage key to remove.</param>
    /// <returns>None.</returns>
    void RemoveState(string key);
    /// <summary>
    /// Clears all persisted key/value state entries.
    /// </summary>
    /// <returns>None.</returns>
    void ClearAllState();
}
