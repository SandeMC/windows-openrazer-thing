using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using NLog;
using RazerController.ViewModels;
using RazerController.Views;
using RazerController.Services;

namespace RazerController;

public partial class App : Application
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private TrayIconService? _trayIconService;

    public override void Initialize()
    {
        try
        {
            Logger.Info("Initializing Avalonia application");
            AvaloniaXamlLoader.Load(this);
            Logger.Info("Avalonia XAML loaded successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during Avalonia initialization");
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Logger.Info("Framework initialization completed, setting up application");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Logger.Info("Setting up desktop application lifetime");
                
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                
                Logger.Info("Creating main window");
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                Logger.Info("Main window created successfully");

                // Initialize tray icon
                Logger.Info("Initializing system tray icon");
                _trayIconService = new TrayIconService();
                _trayIconService.Initialize();
                Logger.Info("System tray icon initialized");

                // Handle window close to minimize to tray
                desktop.MainWindow.Closing += (s, e) =>
                {
                    Logger.Debug("Main window closing - minimizing to tray");
                    e.Cancel = true;
                    desktop.MainWindow.Hide();
                };

                desktop.ShutdownRequested += (s, e) =>
                {
                    Logger.Info("Application shutdown requested");
                    _trayIconService?.Dispose();
                };
                
                Logger.Info("Application setup completed successfully");
            }
            else
            {
                Logger.Warn("Application lifetime is not classic desktop style");
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Fatal error during framework initialization");
            throw;
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}