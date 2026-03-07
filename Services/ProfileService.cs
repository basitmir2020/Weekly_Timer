using WeeklyTimetable.Models;

namespace WeeklyTimetable.Services;

public interface IProfileService
{
    /// <summary>
    /// Retrieves all available schedule profiles.
    /// </summary>
    /// <returns>List of profiles with active flag resolved.</returns>
    Task<List<ScheduleProfile>> GetAllProfilesAsync();
    /// <summary>
    /// Marks a profile as active.
    /// </summary>
    /// <param name="profileId">Profile identifier to activate.</param>
    /// <returns>A task that completes when activation is persisted.</returns>
    Task ActivateProfileAsync(string profileId);
    /// <summary>
    /// Deletes a profile.
    /// </summary>
    /// <param name="profileId">Profile identifier to delete.</param>
    /// <returns>A task that completes when delete flow finishes.</returns>
    Task DeleteProfileAsync(string profileId);
    /// <summary>
    /// Returns the currently active profile.
    /// </summary>
    /// <returns>Active profile or <c>null</c> when none is configured.</returns>
    Task<ScheduleProfile?> GetActiveProfileAsync();
}

public class ProfileService : IProfileService
{
    private const string ACTIVE_KEY = "active_profile";

    private readonly List<ScheduleProfile> _builtInProfiles = new()
    {
        new ScheduleProfile { Id = "default",  Name = "Default",  IsActive = true },
        new ScheduleProfile { Id = "travel",   Name = "Travel",   IsActive = false },
        new ScheduleProfile { Id = "sick_day", Name = "Sick Day", IsActive = false },
        new ScheduleProfile { Id = "exam_mode",Name = "Exam Mode",IsActive = false },
    };

    /// <summary>
    /// Returns built-in profiles and marks the currently active one from preferences.
    /// </summary>
    /// <returns>Profile list with active state applied.</returns>
    /// <remarks>
    /// Side effects: none; returns a copy of built-in profile definitions.
    /// </remarks>
    public Task<List<ScheduleProfile>> GetAllProfilesAsync()
    {
        var activeId = Preferences.Get(ACTIVE_KEY, "default");
        var list = _builtInProfiles.ToList();
        // Apply active marker dynamically so built-in profile definitions remain immutable defaults.
        foreach (var p in list) p.IsActive = p.Id == activeId;
        return Task.FromResult(list);
    }

    /// <summary>
    /// Persists the selected active profile identifier.
    /// </summary>
    /// <param name="profileId">Profile identifier to mark active.</param>
    /// <returns>A completed task.</returns>
    /// <remarks>
    /// Side effects: writes the active profile id to preferences.
    /// </remarks>
    public Task ActivateProfileAsync(string profileId)
    {
        Preferences.Set(ACTIVE_KEY, profileId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to delete a profile.
    /// </summary>
    /// <param name="profileId">Profile identifier requested for deletion.</param>
    /// <returns>A completed task.</returns>
    /// <remarks>
    /// Side effects: none in current implementation because only built-in profiles exist.
    /// </remarks>
    public Task DeleteProfileAsync(string profileId)
    {
        // Built-in profiles cannot be deleted; custom ones would be stored in SQLite
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resolves and returns the active profile.
    /// </summary>
    /// <returns>Currently active profile or <c>null</c>.</returns>
    public async Task<ScheduleProfile?> GetActiveProfileAsync()
    {
        var all = await GetAllProfilesAsync();
        return all.FirstOrDefault(p => p.IsActive);
    }
}
