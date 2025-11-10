using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NLog;
using RazerController.Views;

namespace RazerController.Services;

public class TrayIconService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private TrayIcon? _trayIcon;
    private readonly IClassicDesktopStyleApplicationLifetime? _desktop;

    public TrayIconService()
    {
        Logger.Debug("Creating TrayIconService");
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            Logger.Debug("Desktop application lifetime obtained");
        }
        else
        {
            Logger.Warn("Desktop application lifetime not available");
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

            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon("avares://RazerController/Assets/avalonia-logo.ico"),
                ToolTipText = "Razer Controller"
            };
            Logger.Debug("TrayIcon object created");

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

            _trayIcon.IsVisible = true;
            Logger.Info("Tray icon initialized and made visible");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error initializing tray icon");
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
