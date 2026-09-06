using System.Text.Json;
using Windows.Storage;
using WinUI3_Timer.Models;

namespace WinUI3_Timer.Services;

public static class TimerService
{
    private const string FileName = "timers.json";

    public static List<TimerModel> LoadTimers()
    {
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = folder.TryGetItemAsync(FileName).AsTask().GetAwaiter().GetResult() as StorageFile;
            if (file == null)
                return [CreateDefaultTimer()];

            var json = File.ReadAllText(file.Path);
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

    public static void SaveTimers(List<TimerModel> timers)
    {
        var folder = ApplicationData.Current.LocalFolder;
        var file = folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting)
            .AsTask().GetAwaiter().GetResult();
        var json = JsonSerializer.Serialize(timers, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(file.Path, json);
    }

    private static TimerModel CreateDefaultTimer() => new() { Name = "Timer 1" };
}
