using System.Collections.Specialized;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinUI3_Timer.Models;
using WinUI3_Timer.Pages;
using WinUI3_Timer.ViewModels;

namespace WinUI3_Timer;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");

        ViewModel.Timers.CollectionChanged += OnTimersCollectionChanged;
        foreach (var timer in ViewModel.Timers)
            AddTimerNavItem(timer);

        if (ViewModel.SelectedTimer != null)
        {
            NavView.SelectedItem = FindNavItem(ViewModel.SelectedTimer);
            NavigateToSelectedTimer();
        }
    }

    private void OnTimersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (TimerModel timer in e.NewItems)
                AddTimerNavItem(timer);
        }
        if (e.OldItems != null)
        {
            foreach (TimerModel timer in e.OldItems)
                RemoveTimerNavItem(timer);
        }
    }

    private void AddTimerNavItem(TimerModel timer)
    {
        var item = new NavigationViewItem
        {
            Content = $"{timer.Index}  {timer.Name}",
            Icon = new FontIcon { Glyph = "\uE71D", FontSize = 14 },
            Tag = timer
        };
        item.RightTapped += OnNavItemRightTapped;
        timer.PropertyChanged += (_, _) =>
        {
            item.Content = $"{timer.Index}  {timer.Name}";
        };
        NavView.MenuItems.Add(item);
    }

    private void RemoveTimerNavItem(TimerModel timer)
    {
        var item = FindNavItem(timer);
        if (item != null)
            NavView.MenuItems.Remove(item);
    }

    private NavigationViewItem? FindNavItem(TimerModel timer)
    {
        foreach (var obj in NavView.MenuItems)
        {
            if (obj is NavigationViewItem item && item.Tag is TimerModel t && t.Id == timer.Id)
                return item;
        }
        return null;
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        NavFrame.GoBack();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is TimerModel timer)
        {
            ViewModel.SelectedTimer = timer;
            NavigateToSelectedTimer();
        }
    }

    private void NavigateToSelectedTimer()
    {
        NavFrame.Navigate(typeof(HomePage), ViewModel);
    }

    private void OnNavItemRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is NavigationViewItem item && item.Tag is TimerModel timer)
        {
            ViewModel.SelectedTimer = timer;
            NavView.SelectedItem = item;

            var flyout = new MenuFlyout();

            var renameItem = new MenuFlyoutItem { Text = "Rename", Icon = new FontIcon { Glyph = "\uE8AC" } };
            renameItem.Click += async (_, _) => await OnRenameTimerAsync();
            flyout.Items.Add(renameItem);

            if (ViewModel.HasMultipleTimers)
            {
                var deleteItem = new MenuFlyoutItem { Text = "Delete", Icon = new FontIcon { Glyph = "\uE74D" } };
                deleteItem.Click += async (_, _) => await OnDeleteTimerAsync();
                flyout.Items.Add(deleteItem);
            }

            flyout.ShowAt(item, e.GetPosition(item));
        }
    }

    private async void OnAddTimerClick(object sender, RoutedEventArgs e)
    {
        var name = await ShowNameDialog("New Timer", "Timer Name");
        if (!string.IsNullOrWhiteSpace(name))
        {
            ViewModel.AddTimer(name);
            NavView.SelectedItem = FindNavItem(ViewModel.SelectedTimer!);
        }
    }

    private async Task OnRenameTimerAsync()
    {
        var name = await ShowNameDialog(ViewModel.SelectedTimerName, "Rename Timer");
        if (!string.IsNullOrWhiteSpace(name))
        {
            ViewModel.RenameTimer(name);
        }
    }

    private async Task OnDeleteTimerAsync()
    {
        if (!ViewModel.HasMultipleTimers) return;

        var dialog = new ContentDialog
        {
            Title = "Delete Timer",
            Content = $"Are you sure you want to delete \"{ViewModel.SelectedTimerName}\"?",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Secondary,
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.DeleteTimer();
            if (ViewModel.SelectedTimer != null)
                NavView.SelectedItem = FindNavItem(ViewModel.SelectedTimer);
        }
    }

    private async Task<string?> ShowNameDialog(string defaultName, string title)
    {
        var textBox = new TextBox
        {
            Text = defaultName,
            SelectionStart = 0,
            SelectionLength = defaultName.Length
        };

        var dialog = new ContentDialog
        {
            Title = title,
            Content = textBox,
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? textBox.Text : null;
    }
}
