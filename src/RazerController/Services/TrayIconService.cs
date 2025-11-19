using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using NLog;
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
                string iconPath = Path.Combine(appDir, "Assets", "logo.ico");
                
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
                ToolTipText = "WindowsOpenrazerThing"
            };

            var menu = new NativeMenu();
            
            // Show Window item
            var showItem = new NativeMenuItem("Show Window");
            showItem.Click += (s, e) => ShowMainWindowAndSelectFirstDevice();
            menu.Items.Add(showItem);
            
            menu.Items.Add(new NativeMenuItemSeparator());
            
            // Exit item
            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (s, e) => _desktop.Shutdown();
            menu.Items.Add(exitItem);
            
            _trayIcon.Menu = menu;
            _trayIcon.Clicked += (s, e) => ShowMainWindowAndSelectFirstDevice();

            _trayIcon.IsVisible = true;
            Logger.Info("Tray icon initialized");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error initializing tray icon");
        }
    }



    private void ShowMainWindowAndSelectFirstDevice()
    {
        if (_desktop?.MainWindow != null)
        {
            _desktop.MainWindow.Show();
            _desktop.MainWindow.WindowState = WindowState.Normal;
            _desktop.MainWindow.Activate();
            
            // Auto-select the first device
            if (_desktop.MainWindow.DataContext is MainWindowViewModel vm && vm.Devices.Count > 0)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    vm.SelectedDevice = vm.Devices[0];
                    Logger.Info($"Auto-selected first device: {vm.Devices[0].Name}");
                });
            }
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
