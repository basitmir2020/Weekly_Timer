using System.Text.Json;

namespace WeeklyTimetable.Services;

public class PersistenceService : IPersistenceService
{
    /// <summary>
    /// Serializes and saves state data to preferences storage.
    /// </summary>
    /// <typeparam name="T">Type of data to serialize.</typeparam>
    /// <param name="key">Storage key used for retrieval.</param>
    /// <param name="data">Data object to persist.</param>
    /// <returns>A task that completes after save attempt finishes.</returns>
    /// <remarks>
    /// Side effects: writes serialized JSON to preferences.
    /// </remarks>
    public async Task SaveStateAsync<T>(string key, T data)
    {
        try
        {
            await Task.Run(() => 
            {
                // Serialization and write happen off the calling thread to avoid UI stalls.
                var json = JsonSerializer.Serialize(data);
                Preferences.Default.Set(key, json);
            });
        }
        catch (Exception ex)
        {
            // Log or handle serialization/storage errors
            Console.WriteLine($"Error saving state for key {key}: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads and deserializes state data from preferences storage.
    /// </summary>
    /// <typeparam name="T">Expected result type.</typeparam>
    /// <param name="key">Storage key to read.</param>
    /// <returns>Deserialized value or default when key is missing/invalid.</returns>
    /// <remarks>
    /// Side effects: reads preferences storage and performs JSON deserialization.
    /// </remarks>
    public async Task<T?> LoadStateAsync<T>(string key)
    {
        try
        {
            return await Task.Run(() => 
            {
                var json = Preferences.Default.Get<string?>(key, null);
                if (string.IsNullOrEmpty(json))
                    return default(T);

                return JsonSerializer.Deserialize<T>(json);
            });
        }
        catch (Exception ex)
        {
            // Log or handle deserialization errors (e.g., model changed)
            Console.WriteLine($"Error loading state for key {key}: {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Removes a single saved state entry.
    /// </summary>
    /// <param name="key">Storage key to remove.</param>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: deletes a key from preferences storage.
    /// </remarks>
    public void RemoveState(string key)
    {
        Preferences.Default.Remove(key);
    }

    /// <summary>
    /// Clears all saved preference-based state.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: removes all keys from preferences storage.
    /// </remarks>
    public void ClearAllState()
    {
        Preferences.Default.Clear();
    }
}
