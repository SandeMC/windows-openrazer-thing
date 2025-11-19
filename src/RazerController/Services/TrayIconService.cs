using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using NLog;
using RazerController.Views;
using RazerController.ViewModels;
using RazerController.Models;
using RazerController.Native;

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
            
            // Add device-specific items for the first mouse device found
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Logger.Debug($"Building tray menu. Total devices: {vm.Devices.Count}");
                
                // Find the first mouse device
                var mouseDevice = vm.Devices.FirstOrDefault(d => d.Device.DeviceType == RazerDeviceType.Mouse);
                
                if (mouseDevice != null)
                {
                    Logger.Debug($"Found mouse device: {mouseDevice.Name}");
                }
                else
                {
                    Logger.Debug("No mouse device found for tray menu");
                }
                
                if (mouseDevice != null)
                {
                    menu.Items.Add(new NativeMenuItemSeparator());
                    
                    // RGB Effects submenu (renamed to RGB Mode as per requirements)
                    if (mouseDevice.SupportsRGB)
                    {
                        var rgbMenu = new NativeMenu();
                        
                        if (mouseDevice.SupportsStaticColor)
                        {
                            var staticItem = new NativeMenuItem("Static Color");
                            staticItem.Click += (s, e) => SetRGBEffect("static", mouseDevice);
                            rgbMenu.Items.Add(staticItem);
                        }
                        
                        if (mouseDevice.SupportsSpectrum)
                        {
                            var spectrumItem = new NativeMenuItem("Spectrum");
                            spectrumItem.Click += (s, e) => SetRGBEffect("spectrum", mouseDevice);
                            rgbMenu.Items.Add(spectrumItem);
                        }
                        
                        if (mouseDevice.SupportsBreath)
                        {
                            var breathItem = new NativeMenuItem("Breath");
                            breathItem.Click += (s, e) => SetRGBEffect("breath", mouseDevice);
                            rgbMenu.Items.Add(breathItem);
                        }
                        
                        if (mouseDevice.SupportsNoneEffect)
                        {
                            var offItem = new NativeMenuItem("Turn Off");
                            offItem.Click += (s, e) => SetRGBEffect("off", mouseDevice);
                            rgbMenu.Items.Add(offItem);
                        }
                        
                        var rgbMenuItem = new NativeMenuItem("RGB Mode");
                        rgbMenuItem.Menu = rgbMenu;
                        menu.Items.Add(rgbMenuItem);
                    }
                    
                    // DPI Stages submenu
                    if (mouseDevice.SupportsDPI)
                    {
                        var dpiMenu = new NativeMenu();
                        int[] dpiStages = { 400, 800, 1600, 3200, 6400 };
                        
                        foreach (int dpi in dpiStages)
                        {
                            var dpiItem = new NativeMenuItem($"{dpi} DPI");
                            dpiItem.Click += (s, e) => SetDPI(dpi, mouseDevice);
                            dpiMenu.Items.Add(dpiItem);
                        }
                        
                        var dpiMenuItem = new NativeMenuItem("DPI");
                        dpiMenuItem.Menu = dpiMenu;
                        menu.Items.Add(dpiMenuItem);
                    }
                    
                    // Polling Rate submenu
                    if (mouseDevice.SupportsPollRate)
                    {
                        var pollRateMenu = new NativeMenu();
                        
                        // Get supported polling rates from the device or use standard values
                        var supportedRates = mouseDevice.Device.GetSupportedPollRates();
                        int[] pollRates = supportedRates?.ToArray() ?? new[] { 125, 250, 500, 1000, 2000, 4000, 8000 };
                        
                        foreach (int rate in pollRates)
                        {
                            var rateItem = new NativeMenuItem($"{rate} Hz");
                            rateItem.Click += (s, e) => SetPollRate(rate, mouseDevice);
                            pollRateMenu.Items.Add(rateItem);
                        }
                        
                        var pollRateMenuItem = new NativeMenuItem("Polling Rate");
                        pollRateMenuItem.Menu = pollRateMenu;
                        menu.Items.Add(pollRateMenuItem);
                    }
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
    
    private void SetRGBEffect(string effect, DeviceModel device)
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    bool success = false;
                    switch (effect)
                    {
                        case "static":
                            success = device.Device.SetStaticColor(vm.RedValue, vm.GreenValue, vm.BlueValue);
                            break;
                        case "spectrum":
                            success = device.Device.SetSpectrumEffect();
                            break;
                        case "breath":
                            success = device.Device.SetBreathEffect(vm.RedValue, vm.GreenValue, vm.BlueValue);
                            break;
                        case "off":
                            success = device.Device.SetNoneEffect();
                            break;
                    }
                    Logger.Info($"Set RGB effect '{effect}' for {device.Name}: {(success ? "success" : "failed")}");
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, $"Error setting RGB effect: {effect}");
        }
    }
    
    private void SetDPI(int dpi, DeviceModel device)
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    bool success = device.Device.SetDPI(dpi);
                    if (success)
                    {
                        Logger.Info($"Set DPI to {dpi} for {device.Name}");
                        // Update the ViewModel if this is the selected device
                        if (vm.SelectedDevice?.Device == device.Device)
                        {
                            vm.DpiValue = dpi;
                        }
                    }
                    else
                    {
                        Logger.Warn($"Failed to set DPI to {dpi} for {device.Name}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Warn(ex, $"Error setting DPI: {dpi}");
        }
    }
    
    private void SetPollRate(int rate, DeviceModel device)
    {
        try
        {
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    bool success = device.Device.SetPollRate(rate);
                    if (success)
                    {
                        Logger.Info($"Set polling rate to {rate}Hz for {device.Name}");
                        // Update the ViewModel if this is the selected device
                        if (vm.SelectedDevice?.Device == device.Device)
                        {
                            vm.PollRate = rate;
                            // Update the selected index
                            for (int i = 0; i < vm.PollRateOptions.Count; i++)
                            {
                                if (vm.PollRateOptions[i] == rate)
                                {
                                    vm.SelectedPollRateIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.Warn($"Failed to set polling rate to {rate}Hz for {device.Name}");
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
