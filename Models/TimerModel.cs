using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinUI3_Timer.Models;

public class TimerModel : INotifyPropertyChanged
{
    private int _index;
    private string _name = "New Timer";

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public long ElapsedTicks { get; set; }
    public bool IsRunning { get; set; }
    public DateTimeOffset? LastStartTime { get; set; }

    public int Index
    {
        get => _index;
        set { _index = value; OnPropertyChanged(); }
    }

    public TimeSpan Elapsed
    {
        get => TimeSpan.FromTicks(ElapsedTicks);
        set => ElapsedTicks = value.Ticks;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
