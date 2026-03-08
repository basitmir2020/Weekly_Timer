using WeeklyTimetable.ViewModels;

namespace WeeklyTimetable.Views;

public partial class MainPage : ContentPage
{
    /// <summary>
    /// Initializes the main page and assigns the injected main view model.
    /// </summary>
    /// <param name="viewModel">Main view model driving schedule interactions.</param>
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Loads main data when page appears, after a short delay to avoid competing with transition animation.
    /// </summary>
    /// <returns>None.</returns>
    /// <remarks>
    /// Side effects: triggers asynchronous data loading through <see cref="MainViewModel"/>.
    /// </remarks>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel vm)
        {
            Dispatcher.Dispatch(async () => await vm.LoadDataAsync());
        }
    }
}
