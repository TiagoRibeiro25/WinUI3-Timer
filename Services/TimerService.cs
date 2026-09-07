using System.Text.Json;
using Windows.Storage;
using WinUI3_Timer.Models;

namespace WinUI3_Timer.Services;

public static class TimerService
{
    private const string FileName = "timers.json";

    public static async Task<List<TimerModel>> LoadTimersAsync()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(FileName) as StorageFile;
            if (file == null)
                return [CreateDefaultTimer()];

            var json = await File.ReadAllTextAsync(file.Path);
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
        var folder = ApplicationData.Current.LocalFolder;
        var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
        var json = JsonSerializer.Serialize(timers, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(file.Path, json);
    }

    private static TimerModel CreateDefaultTimer() => new() { Name = "Timer 1" };
}
