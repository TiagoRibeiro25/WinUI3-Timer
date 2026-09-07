using Microsoft.UI.Xaml;

namespace WinUI3_Timer;

public partial class App : Application
{
    private Window? _window;

    private static readonly string LogFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinUI3-Timer");

    public App()
    {
        InitializeComponent();

       UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        WriteCrashLog(e.Exception);
    }

    private void OnDomainUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            WriteCrashLog(ex);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        WriteCrashLog(e.Exception);
    }

    private static void WriteCrashLog(Exception exception)
    {
        try
        {
            Directory.CreateDirectory(LogFolder);
            var filePath = Path.Combine(LogFolder, "crash.log");
            var message = $"""
                [{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Unhandled Exception
                {exception}
                """;
            File.AppendAllText(filePath, message + Environment.NewLine + Environment.NewLine);
        }
        catch
        {
            // Swallow logging failures
        }
    }
}
