using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Yield to allow the native page transition animation to complete first
        await Task.Yield();

        if (BindingContext is MainViewModel vm)
        {
            await vm.LoadDataAsync();
        }
    }
}
