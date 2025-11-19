using Avalonia;
using NLog;
using System;
using System.IO;

namespace RazerController;

sealed class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Set up NLog configuration
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);
            
            // Check if DEBUG file exists to enable debug logging
            var debugFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DEBUG");
            if (File.Exists(debugFilePath))
            {
                // Enable DEBUG level logging
                foreach (var rule in LogManager.Configuration.LoggingRules)
                {
                    rule.EnableLoggingForLevel(LogLevel.Debug);
                    rule.EnableLoggingForLevel(LogLevel.Trace);
                }
                LogManager.ReconfigExistingLoggers();
                Logger.Info("DEBUG file detected - Debug logging enabled");
            }
            
            Logger.Info("===== WindowsOpenrazerThing Starting =====");
            Logger.Info($"Application Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
            Logger.Info($"Working Directory: {Environment.CurrentDirectory}");
            Logger.Info($"OS: {Environment.OSVersion}");
            Logger.Info($".NET Version: {Environment.Version}");
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            
            Logger.Info("===== WindowsOpenrazerThing Exiting =====");
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Fatal error during application startup");
            
            // Write error to a file that user can easily find
            try
            {
                var errorFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FATAL_ERROR.txt");
                File.WriteAllText(errorFile, 
                    $"FATAL ERROR - WindowsOpenrazerThing failed to start\n\n" +
                    $"Time: {DateTime.Now}\n" +
                    $"Error: {ex.Message}\n\n" +
                    $"Stack Trace:\n{ex.StackTrace}\n\n" +
                    $"Check the logs folder for more details.");
            }
            catch
            {
                // Ignore errors writing the error file
            }
            
            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Logger.Debug("Building Avalonia app configuration");
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
