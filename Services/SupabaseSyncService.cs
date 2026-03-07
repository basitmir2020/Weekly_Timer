using Supabase;

namespace WeeklyTimetable.Services;

public interface ISupabaseSyncService
{
    /// <summary>
    /// Initializes Supabase client connection.
    /// </summary>
    /// <param name="url">Supabase project URL.</param>
    /// <param name="key">Supabase API key.</param>
    /// <returns>A task that completes after client initialization.</returns>
    Task InitializeAsync(string url, string key);
    /// <summary>
    /// Signs in with email/password credentials.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns><c>true</c> on successful authentication; otherwise <c>false</c>.</returns>
    Task<bool> SignInAsync(string email, string password);
    /// <summary>
    /// Uploads local schedule backup payload to Supabase.
    /// </summary>
    /// <returns><c>true</c> when backup succeeds; otherwise <c>false</c>.</returns>
    Task<bool> BackupDataAsync();
    /// <summary>
    /// Restores schedule backup payload from Supabase into local preferences.
    /// </summary>
    /// <returns><c>true</c> when restore succeeds; otherwise <c>false</c>.</returns>
    Task<bool> RestoreDataAsync();
}

public class SupabaseSyncService : ISupabaseSyncService
{
    private const string ScheduleStateKey = "sched_v3";
    private const string LegacyScheduleStateKey = "sched_v2";

    private Client? _client;

    /// <summary>
    /// Creates the Supabase sync service.
    /// </summary>
    public SupabaseSyncService()
    {
    }

    /// <summary>
    /// Initializes the Supabase client with realtime-enabled options.
    /// </summary>
    /// <param name="url">Supabase project URL.</param>
    /// <param name="key">Supabase API key.</param>
    /// <returns>A task that completes when client initialization finishes.</returns>
    /// <remarks>
    /// Side effects: allocates and initializes Supabase client instance.
    /// </remarks>
    public async Task InitializeAsync(string url, string key)
    {
        var options = new SupabaseOptions { AutoConnectRealtime = true };
        _client = new Client(url, key, options);
        await _client.InitializeAsync();
    }

    /// <summary>
    /// Authenticates a user with email/password against Supabase Auth.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <returns><c>true</c> when a valid user session is returned; otherwise <c>false</c>.</returns>
    public async Task<bool> SignInAsync(string email, string password)
    {
        if (_client == null) return false;
        try
        {
            var session = await _client.Auth.SignIn(email, password);
            return session?.User != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Backs up local schedule preference payload to the <c>user_backups</c> table.
    /// </summary>
    /// <returns><c>true</c> when upload succeeds; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Side effects: reads local preferences and writes backup row to Supabase.
    /// </remarks>
    public async Task<bool> BackupDataAsync()
    {
        if (_client?.Auth.CurrentUser == null) return false;
        var userId = _client.Auth.CurrentUser.Id;
        if (string.IsNullOrWhiteSpace(userId)) return false;

        try
        {
            // Serialize local preferences from current key, with legacy fallback.
            string schedData = Preferences.Get(ScheduleStateKey, Preferences.Get(LegacyScheduleStateKey, "{}"));

            // Assuming a table 'user_backups' with columns 'user_id', 'sched_json'
            var backup = new UserBackup
            {
                UserId = userId,
                SchedJson = schedData,
                UpdatedAt = DateTime.UtcNow
            };

            await _client.From<UserBackup>().Upsert(backup);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Restores schedule payload from Supabase into local preferences.
    /// </summary>
    /// <returns><c>true</c> when restore succeeds and payload is non-empty; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Side effects: writes restored schedule JSON into local preferences.
    /// </remarks>
    public async Task<bool> RestoreDataAsync()
    {
        if (_client?.Auth.CurrentUser == null) return false;
        var userId = _client.Auth.CurrentUser.Id;
        if (string.IsNullOrWhiteSpace(userId)) return false;

        try
        {
            var response = await _client.From<UserBackup>()
                                        .Where(b => b.UserId == userId)
                                        .Single();

            if (response != null && !string.IsNullOrEmpty(response.SchedJson))
            {
                Preferences.Set(ScheduleStateKey, response.SchedJson);
                Preferences.Set(LegacyScheduleStateKey, response.SchedJson);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}

[Postgrest.Attributes.Table("user_backups")]
public class UserBackup : Postgrest.Models.BaseModel
{
    [Postgrest.Attributes.PrimaryKey("user_id", false)]
    public string UserId { get; set; } = string.Empty;

    [Postgrest.Attributes.Column("sched_json")]
    public string SchedJson { get; set; } = string.Empty;

    [Postgrest.Attributes.Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
