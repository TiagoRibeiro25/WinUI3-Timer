using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUI3_Timer.ViewModels;

namespace WinUI3_Timer.Pages;

public sealed partial class HomePage : Page
{
    public MainViewModel ViewModel { get; private set; } = null!;

    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is MainViewModel vm)
        {
            ViewModel = vm;
            DataContext = ViewModel;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e) { }
    private async void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
            await ViewModel.SaveAsync();
    }

    private void OnStartClick(object sender, RoutedEventArgs e) => ViewModel?.Start();
    private void OnStopClick(object sender, RoutedEventArgs e) => ViewModel?.Stop();
    private void OnResetClick(object sender, RoutedEventArgs e) => ViewModel?.Reset();
}
