using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using NLog;
using RazerController.Views;
using RazerController.ViewModels;

namespace RazerController.Services;

public class TrayIconService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private TrayIcon? _trayIcon;
    private readonly IClassicDesktopStyleApplicationLifetime? _desktop;

    public TrayIconService()
    {
        Logger.Info("Creating TrayIconService");
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
        }
    }

    public void Initialize()
    {
        try
        {
            Logger.Info("Initializing tray icon");
            
            if (_desktop == null)
            {
                Logger.Warn("Cannot initialize tray icon - desktop is null");
                return;
            }

            WindowIcon? icon = null;
            try
            {
                // Try to load icon from file in Assets folder
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string iconPath = Path.Combine(appDir, "Assets", "avalonia-logo.ico");
                
                if (File.Exists(iconPath))
                {
                    icon = new WindowIcon(iconPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Could not load tray icon");
            }

            _trayIcon = new TrayIcon
            {
                Icon = icon,
                ToolTipText = "Windows OpenRazer Thing"
            };

            var menu = new NativeMenu();

            var showItem = new NativeMenuItem("Show");
            showItem.Click += (s, e) => ShowMainWindow();
            menu.Items.Add(showItem);

            menu.Items.Add(new NativeMenuItemSeparator());

            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (s, e) => _desktop.Shutdown();
            menu.Items.Add(exitItem);

            _trayIcon.Menu = menu;
            _trayIcon.Clicked += (s, e) => ShowMainWindow();
            
            // When right-clicking to open menu, poll device values once
            _trayIcon.Menu.Opening += (s, e) => PollDeviceValuesOnce();

            _trayIcon.IsVisible = true;
            Logger.Info("Tray icon initialized");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error initializing tray icon");
        }
    }

    private void PollDeviceValuesOnce()
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                // Poll device values silently (no log noise)
                vm.RefreshDeviceValues();
            }
        }
        catch
        {
            // Silently fail - no log noise
        }
    }

    private void ShowMainWindow()
    {
        if (_desktop?.MainWindow != null)
        {
            _desktop.MainWindow.Show();
            _desktop.MainWindow.WindowState = WindowState.Normal;
            _desktop.MainWindow.Activate();
        }
    }

    public void Dispose()
    {
        if (_trayIcon != null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Dispose();
        }
    }
}
