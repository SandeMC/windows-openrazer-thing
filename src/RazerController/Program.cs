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
            // Detect if running as a single file (PublishSingleFile)
            var processPath = Environment.ProcessPath;
            var isSingleFile = !string.IsNullOrEmpty(processPath) && 
                               File.Exists(processPath) &&
                               Path.GetExtension(processPath).Equals(".exe", StringComparison.OrdinalIgnoreCase);
            
            // Set up NLog configuration
            string logDirectory;
            string logFileName;
            
            if (isSingleFile)
            {
                // For single-file exe: log next to the exe with the same name
                var exeDir = Path.GetDirectoryName(processPath);
                var exeName = Path.GetFileNameWithoutExtension(processPath);
                logDirectory = exeDir ?? AppDomain.CurrentDomain.BaseDirectory;
                logFileName = Path.Combine(logDirectory, $"{exeName}.log");
                
                // Update NLog configuration to use the exe name for logging
                var config = LogManager.Configuration;
                if (config != null)
                {
                    var fileTarget = config.FindTargetByName<NLog.Targets.FileTarget>("logfile");
                    if (fileTarget != null)
                    {
                        fileTarget.FileName = logFileName;
                        fileTarget.ArchiveFileName = Path.Combine(logDirectory, $"{exeName}-{{#}}.log");
                    }
                    LogManager.Configuration = config;
                }
            }
            else
            {
                // For folder structure: use logs subdirectory
                logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logDirectory);
            }
            
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
