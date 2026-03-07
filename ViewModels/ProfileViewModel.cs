using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeeklyTimetable.Models;
using WeeklyTimetable.Services;

namespace WeeklyTimetable.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileService _profileService;

    [ObservableProperty] private ObservableCollection<ScheduleProfile> _profiles = new();
    [ObservableProperty] private ScheduleProfile? _activeProfile;

    /// <summary>
    /// Creates the profile view model and begins loading available schedule profiles.
    /// </summary>
    /// <param name="profileService">Service used to query and manage profile state.</param>
    /// <remarks>
    /// Side effects: starts asynchronous profile loading.
    /// </remarks>
    public ProfileViewModel(IProfileService profileService)
    {
        _profileService = profileService;
        _ = LoadAsync();
    }

    /// <summary>
    /// Loads all profiles and resolves the currently active profile.
    /// </summary>
    /// <returns>A task that completes when profile collection is refreshed.</returns>
    /// <remarks>
    /// Side effects: clears and repopulates <see cref="Profiles"/> and updates <see cref="ActiveProfile"/>.
    /// </remarks>
    private async Task LoadAsync()
    {
        var all = await _profileService.GetAllProfilesAsync();
        Profiles.Clear();
        foreach (var p in all) Profiles.Add(p);
        ActiveProfile = Profiles.FirstOrDefault(p => p.IsActive);
    }

    /// <summary>
    /// Activates the selected profile and updates local active markers.
    /// </summary>
    /// <param name="profile">Profile chosen by the user.</param>
    /// <returns>A task that completes after activation state is persisted.</returns>
    /// <remarks>
    /// Side effects: updates persisted active profile id and mutates profile flags in memory.
    /// </remarks>
    [RelayCommand]
    private async Task ActivateProfileAsync(ScheduleProfile profile)
    {
        await _profileService.ActivateProfileAsync(profile.Id);
        foreach (var p in Profiles) p.IsActive = (p.Id == profile.Id);
        ActiveProfile = profile;
    }

    /// <summary>
    /// Confirms and deletes a profile through the profile service.
    /// </summary>
    /// <param name="profile">Profile requested for deletion.</param>
    /// <returns>A task that completes after confirmation and deletion attempt.</returns>
    /// <remarks>
    /// Side effects: may remove a profile from <see cref="Profiles"/>.
    /// </remarks>
    [RelayCommand]
    private async Task DeleteProfileAsync(ScheduleProfile profile)
    {
        bool confirm = await Shell.Current.DisplayAlert("Delete Profile",
            $"Delete \"{profile.Name}\"?", "Delete", "Cancel");
        if (confirm)
        {
            await _profileService.DeleteProfileAsync(profile.Id);
            Profiles.Remove(profile);
        }
    }
}
