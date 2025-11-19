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
            _trayIcon.Menu = menu;
            _trayIcon.Clicked += (s, e) => ShowMainWindow();
            
            // Build the initial menu
            BuildTrayMenu();
            
            // When right-clicking to open menu, rebuild it dynamically based on current device
            _trayIcon.Menu.Opening += (s, e) => 
            {
                PollDeviceValuesOnce();
                UpdateTooltip();
                BuildTrayMenu();
            };

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
                vm.RefreshDeviceValuesCommand.Execute(null);
            }
        }
        catch
        {
            // Silently fail - no log noise
        }
    }

    private void BuildTrayMenu()
    {
        try
        {
            if (_trayIcon?.Menu == null) return;
            
            var menu = _trayIcon.Menu;
            menu.Items.Clear();
            
            // Show Window item
            var showItem = new NativeMenuItem("Show Window");
            showItem.Click += (s, e) => ShowMainWindow();
            menu.Items.Add(showItem);
            
            // Refresh Devices item
            var refreshItem = new NativeMenuItem("Refresh Devices");
            refreshItem.Click += (s, e) => RefreshDevices();
            menu.Items.Add(refreshItem);
            
            // Add device-specific items if a device is selected
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm && vm.SelectedDevice != null)
            {
                menu.Items.Add(new NativeMenuItemSeparator());
                
                // RGB Effects submenu
                if (vm.SelectedDevice.SupportsRGB)
                {
                    var rgbMenu = new NativeMenu();
                    
                    if (vm.SelectedDevice.SupportsStaticColor)
                    {
                        var staticItem = new NativeMenuItem("Static Color");
                        staticItem.Click += (s, e) => SetRGBEffect("static");
                        rgbMenu.Items.Add(staticItem);
                    }
                    
                    if (vm.SelectedDevice.SupportsSpectrum)
                    {
                        var spectrumItem = new NativeMenuItem("Spectrum");
                        spectrumItem.Click += (s, e) => SetRGBEffect("spectrum");
                        rgbMenu.Items.Add(spectrumItem);
                    }
                    
                    if (vm.SelectedDevice.SupportsBreath)
                    {
                        var breathItem = new NativeMenuItem("Breath");
                        breathItem.Click += (s, e) => SetRGBEffect("breath");
                        rgbMenu.Items.Add(breathItem);
                    }
                    
                    if (vm.SelectedDevice.SupportsNoneEffect)
                    {
                        var offItem = new NativeMenuItem("Turn Off");
                        offItem.Click += (s, e) => SetRGBEffect("off");
                        rgbMenu.Items.Add(offItem);
                    }
                    
                    var rgbMenuItem = new NativeMenuItem("RGB Effects");
                    rgbMenuItem.Menu = rgbMenu;
                    menu.Items.Add(rgbMenuItem);
                }
                
                // DPI Stages submenu (for mice)
                if (vm.SelectedDevice.SupportsDPI)
                {
                    var dpiMenu = new NativeMenu();
                    int[] dpiStages = { 400, 800, 1600, 3200, 6400 };
                    
                    foreach (int dpi in dpiStages)
                    {
                        var dpiItem = new NativeMenuItem($"{dpi} DPI");
                        dpiItem.Click += (s, e) => SetDPI(dpi);
                        dpiMenu.Items.Add(dpiItem);
                    }
                    
                    var dpiMenuItem = new NativeMenuItem("DPI");
                    dpiMenuItem.Menu = dpiMenu;
                    menu.Items.Add(dpiMenuItem);
                }
                
                // Polling Rate submenu (for mice)
                if (vm.SelectedDevice.SupportsPollRate && vm.PollRateOptions.Count > 0)
                {
                    var pollRateMenu = new NativeMenu();
                    
                    foreach (int rate in vm.PollRateOptions)
                    {
                        var rateItem = new NativeMenuItem($"{rate} Hz");
                        rateItem.Click += (s, e) => SetPollRate(rate);
                        pollRateMenu.Items.Add(rateItem);
                    }
                    
                    var pollRateMenuItem = new NativeMenuItem("Polling Rate");
                    pollRateMenuItem.Menu = pollRateMenu;
                    menu.Items.Add(pollRateMenuItem);
                }
            }
            
            menu.Items.Add(new NativeMenuItemSeparator());
            
            // Exit item
            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (s, e) => _desktop?.Shutdown();
            menu.Items.Add(exitItem);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error building tray menu");
        }
    }
    
    private void SetRGBEffect(string effect)
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    switch (effect)
                    {
                        case "static":
                            vm.SetStaticColorCommand.Execute(null);
                            break;
                        case "spectrum":
                            vm.SetSpectrumCommand.Execute(null);
                            break;
                        case "breath":
                            vm.SetBreathCommand.Execute(null);
                            break;
                        case "off":
                            vm.TurnOffCommand.Execute(null);
                            break;
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, $"Error setting RGB effect: {effect}");
        }
    }
    
    private void SetDPI(int dpi)
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    vm.DpiValue = dpi;
                    vm.SetDPICommand.Execute(null);
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, $"Error setting DPI: {dpi}");
        }
    }
    
    private void SetPollRate(int rate)
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    // Find the index of this rate in the options
                    for (int i = 0; i < vm.PollRateOptions.Count; i++)
                    {
                        if (vm.PollRateOptions[i] == rate)
                        {
                            vm.SelectedPollRateIndex = i;
                            vm.SetPollRateCommand.Execute(null);
                            break;
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, $"Error setting poll rate: {rate}");
        }
    }
    
    private void RefreshDevices()
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => vm.RefreshCommand.Execute(null));
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, "Error refreshing devices from tray");
        }
    }

    private void UpdateTooltip()
    {
        try
        {
            if (_trayIcon == null) return;
            
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm && vm.SelectedDevice != null)
            {
                string deviceInfo = $"WindowsOpenrazerThing\n{vm.SelectedDevice.Name}";
                
                if (vm.SelectedDevice.SupportsDPI && vm.DpiValue > 0)
                {
                    deviceInfo += $"\nDPI: {vm.DpiValue}";
                }
                
                if (vm.SelectedDevice.SupportsPollRate && vm.PollRate > 0)
                {
                    deviceInfo += $"\nPoll Rate: {vm.PollRate}Hz";
                }
                
                if (vm.SelectedDevice.SupportsBattery && vm.BatteryLevel.HasValue)
                {
                    deviceInfo += $"\nBattery: {vm.BatteryLevel}%{(vm.IsCharging ? " (Charging)" : "")}";
                }
                
                _trayIcon.ToolTipText = deviceInfo;
            }
            else
            {
                _trayIcon.ToolTipText = "WindowsOpenrazerThing";
            }
        }
        catch
        {
            // Silently fail
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
