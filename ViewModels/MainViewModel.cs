using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using WinUI3_Timer.Models;
using WinUI3_Timer.Services;

namespace WinUI3_Timer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly DispatcherTimer _tickTimer;
    private TimerModel? _selectedTimer;
    private string _currentTime = "00:00:00";

    public ObservableCollection<TimerModel> Timers { get; }
    public bool HasMultipleTimers => Timers.Count > 1;

    public TimerModel? SelectedTimer
    {
        get => _selectedTimer;
        set
        {
            _selectedTimer = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedTimer));
            OnPropertyChanged(nameof(SelectedTimerName));
            UpdateDisplay();
        }
    }

    public bool HasSelectedTimer => _selectedTimer != null;

    public string SelectedTimerName => _selectedTimer?.Name ?? "";

    public string CurrentTime
    {
        get => _currentTime;
        set { _currentTime = value; OnPropertyChanged(); }
    }

    public bool IsRunning => _selectedTimer?.IsRunning ?? false;

    public MainViewModel()
    {
        Timers = new ObservableCollection<TimerModel>(TimerService.LoadTimers());
        RefreshIndices();
        SelectedTimer = Timers.FirstOrDefault();

        _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _tickTimer.Tick += OnTick;
        _tickTimer.Start();
    }

    public void Start()
    {
        if (_selectedTimer == null || _selectedTimer.IsRunning) return;
        _selectedTimer.IsRunning = true;
        _selectedTimer.LastStartTime = DateTimeOffset.UtcNow;
        OnPropertyChanged(nameof(IsRunning));
        Save();
    }

    public void Stop()
    {
        if (_selectedTimer == null || !_selectedTimer.IsRunning) return;
        if (_selectedTimer.LastStartTime.HasValue)
        {
            _selectedTimer.Elapsed += DateTimeOffset.UtcNow - _selectedTimer.LastStartTime.Value;
        }
        _selectedTimer.IsRunning = false;
        _selectedTimer.LastStartTime = null;
        OnPropertyChanged(nameof(IsRunning));
        UpdateDisplay();
        Save();
    }

    public void Reset()
    {
        if (_selectedTimer == null) return;
        _selectedTimer.Elapsed = TimeSpan.Zero;
        _selectedTimer.IsRunning = false;
        _selectedTimer.LastStartTime = null;
        OnPropertyChanged(nameof(IsRunning));
        UpdateDisplay();
        Save();
    }

    public void AddTimer(string name)
    {
        var timer = new TimerModel { Name = name };
        Timers.Add(timer);
        RefreshIndices();
        OnPropertyChanged(nameof(HasMultipleTimers));
        SelectedTimer = timer;
        Save();
    }

    public void RenameTimer(string name)
    {
        if (_selectedTimer == null) return;
        _selectedTimer.Name = name;
        OnPropertyChanged(nameof(SelectedTimerName));
        Save();
    }

    public void DeleteTimer()
    {
        if (_selectedTimer == null || !HasMultipleTimers) return;
        var index = Timers.IndexOf(_selectedTimer);
        Timers.Remove(_selectedTimer);
        RefreshIndices();
        OnPropertyChanged(nameof(HasMultipleTimers));
        SelectedTimer = Timers[Math.Min(index, Timers.Count - 1)];
        Save();
    }

    private void RefreshIndices()
    {
        for (int i = 0; i < Timers.Count; i++)
            Timers[i].Index = i + 1;
    }

    public void Save() => TimerService.SaveTimers(Timers.ToList());

    private void OnTick(object? sender, object e) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (_selectedTimer == null)
        {
            CurrentTime = "00:00:00";
            return;
        }

        var elapsed = _selectedTimer.Elapsed;
        if (_selectedTimer.IsRunning && _selectedTimer.LastStartTime.HasValue)
        {
            elapsed += DateTimeOffset.UtcNow - _selectedTimer.LastStartTime.Value;
        }

        CurrentTime = elapsed.TotalHours >= 1
            ? elapsed.ToString(@"hh\:mm\:ss")
            : elapsed.ToString(@"mm\:ss");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
