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

    public ProfileViewModel(IProfileService profileService)
    {
        _profileService = profileService;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var all = await _profileService.GetAllProfilesAsync();
        Profiles.Clear();
        foreach (var p in all) Profiles.Add(p);
        ActiveProfile = Profiles.FirstOrDefault(p => p.IsActive);
    }

    [RelayCommand]
    private async Task ActivateProfileAsync(ScheduleProfile profile)
    {
        await _profileService.ActivateProfileAsync(profile.Id);
        foreach (var p in Profiles) p.IsActive = (p.Id == profile.Id);
        ActiveProfile = profile;
    }

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
