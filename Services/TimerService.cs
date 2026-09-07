using System.Text.Json;
using WinUI3_Timer.Models;

namespace WinUI3_Timer.Services;

public static class TimerService
{
    private const string AppName = "WinUI3-Timer";
    private const string FileName = "timers.json";

    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName);

    public static async Task<List<TimerModel>> LoadTimersAsync()
    {
        try
        {
            var filePath = Path.Combine(DataFolder, FileName);
            if (!File.Exists(filePath))
                return [CreateDefaultTimer()];

            var json = await File.ReadAllTextAsync(filePath);
            var timers = JsonSerializer.Deserialize<List<TimerModel>>(json);
            if (timers == null || timers.Count == 0)
                return [CreateDefaultTimer()];

            foreach (var timer in timers)
            {
                if (timer.IsRunning && timer.LastStartTime.HasValue)
                {
                    var gap = DateTimeOffset.UtcNow - timer.LastStartTime.Value;
                    timer.Elapsed += gap;
                    timer.IsRunning = false;
                    timer.LastStartTime = null;
                }
            }

            return timers;
        }
        catch
        {
            return [CreateDefaultTimer()];
        }
    }

    public static async Task SaveTimersAsync(List<TimerModel> timers)
    {
        Directory.CreateDirectory(DataFolder);
        var filePath = Path.Combine(DataFolder, FileName);
        var json = JsonSerializer.Serialize(timers, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    private static TimerModel CreateDefaultTimer() => new() { Name = "Timer 1" };
}
