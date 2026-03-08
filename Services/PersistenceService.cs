using System.Text.Json;

namespace WeeklyTimetable.Services;

public class PersistenceService : IPersistenceService
{
    private readonly SemaphoreSlim _preferencesGate = new(1, 1);

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
            var json = await Task.Run(() => JsonSerializer.Serialize(data)).ConfigureAwait(false);

            await _preferencesGate.WaitAsync().ConfigureAwait(false);
            try
            {
                Preferences.Default.Set(key, json);
            }
            finally
            {
                _preferencesGate.Release();
            }
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
            string? json;
            await _preferencesGate.WaitAsync().ConfigureAwait(false);
            try
            {
                json = Preferences.Default.Get<string?>(key, null);
            }
            finally
            {
                _preferencesGate.Release();
            }

            if (string.IsNullOrEmpty(json))
                return default;

            return await Task.Run(() => JsonSerializer.Deserialize<T>(json)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log or handle deserialization errors (e.g., model changed)
            Console.WriteLine($"Error loading state for key {key}: {ex.Message}");
            return default;
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
