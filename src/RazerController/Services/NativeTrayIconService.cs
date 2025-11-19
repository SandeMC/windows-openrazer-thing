using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Avalonia.Controls.ApplicationLifetimes;
using NLog;
using RazerController.ViewModels;

namespace RazerController.Services;

/// <summary>
/// Native Windows tray icon implementation using Shell_NotifyIcon API
/// </summary>
public class NativeTrayIconService : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private NotifyIcon? _notifyIcon;
    private readonly IClassicDesktopStyleApplicationLifetime? _desktop;
    private ContextMenuStrip? _contextMenu;

    public NativeTrayIconService()
    {
        Logger.Info("Creating NativeTrayIconService");
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
        }
    }

    public void Initialize()
    {
        try
        {
            Logger.Info("Initializing native tray icon");
            
            if (_desktop == null)
            {
                Logger.Warn("Cannot initialize tray icon - desktop is null");
                return;
            }

            _notifyIcon = new NotifyIcon();
            
            // Try to load icon
            Icon? icon = null;
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string iconPath = Path.Combine(appDir, "Assets", "avalonia-logo.ico");
                
                if (File.Exists(iconPath))
                {
                    icon = new Icon(iconPath);
                    Logger.Info($"Loaded tray icon from {iconPath}");
                }
                else
                {
                    Logger.Warn($"Icon file not found at {iconPath}");
                    // Use default application icon
                    icon = SystemIcons.Application;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Could not load tray icon, using default");
                icon = SystemIcons.Application;
            }

            _notifyIcon.Icon = icon;
            _notifyIcon.Text = "WindowsOpenrazerThing";
            _notifyIcon.Visible = true;

            // Double-click to show window
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

            // Build context menu
            BuildContextMenu();

            Logger.Info("Native tray icon initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error initializing native tray icon");
        }
    }

    private void BuildContextMenu()
    {
        try
        {
            _contextMenu = new ContextMenuStrip();

            // Show Window
            var showItem = new ToolStripMenuItem("Show Window");
            showItem.Click += (s, e) => ShowMainWindow();
            _contextMenu.Items.Add(showItem);

            // Refresh Devices
            var refreshItem = new ToolStripMenuItem("Refresh Devices");
            refreshItem.Click += (s, e) => RefreshDevices();
            _contextMenu.Items.Add(refreshItem);

            // Add device-specific items if a device is selected
            if (_desktop?.MainWindow?.DataContext is MainWindowViewModel vm && vm.SelectedDevice != null)
            {
                _contextMenu.Items.Add(new ToolStripSeparator());

                // RGB Effects submenu
                if (vm.SelectedDevice.SupportsRGB)
                {
                    var rgbMenu = new ToolStripMenuItem("RGB Effects");

                    if (vm.SelectedDevice.SupportsStaticColor)
                    {
                        var staticItem = new ToolStripMenuItem("Static Color");
                        staticItem.Click += (s, e) => SetRGBEffect("static");
                        rgbMenu.DropDownItems.Add(staticItem);
                    }

                    if (vm.SelectedDevice.SupportsSpectrum)
                    {
                        var spectrumItem = new ToolStripMenuItem("Spectrum");
                        spectrumItem.Click += (s, e) => SetRGBEffect("spectrum");
                        rgbMenu.DropDownItems.Add(spectrumItem);
                    }

                    if (vm.SelectedDevice.SupportsBreath)
                    {
                        var breathItem = new ToolStripMenuItem("Breath");
                        breathItem.Click += (s, e) => SetRGBEffect("breath");
                        rgbMenu.DropDownItems.Add(breathItem);
                    }

                    if (vm.SelectedDevice.SupportsNoneEffect)
                    {
                        var offItem = new ToolStripMenuItem("Turn Off");
                        offItem.Click += (s, e) => SetRGBEffect("off");
                        rgbMenu.DropDownItems.Add(offItem);
                    }

                    _contextMenu.Items.Add(rgbMenu);
                }

                // DPI submenu (for mice)
                if (vm.SelectedDevice.SupportsDPI)
                {
                    var dpiMenu = new ToolStripMenuItem("DPI");
                    int[] dpiStages = { 400, 800, 1600, 3200, 6400 };

                    foreach (int dpi in dpiStages)
                    {
                        var dpiItem = new ToolStripMenuItem($"{dpi} DPI");
                        dpiItem.Click += (s, e) => SetDPI(dpi);
                        dpiMenu.DropDownItems.Add(dpiItem);
                    }

                    _contextMenu.Items.Add(dpiMenu);
                }

                // Polling Rate submenu (for mice)
                if (vm.SelectedDevice.SupportsPollRate && vm.PollRateOptions.Count > 0)
                {
                    var pollRateMenu = new ToolStripMenuItem("Polling Rate");

                    foreach (int rate in vm.PollRateOptions)
                    {
                        var rateItem = new ToolStripMenuItem($"{rate} Hz");
                        rateItem.Click += (s, e) => SetPollRate(rate);
                        pollRateMenu.DropDownItems.Add(rateItem);
                    }

                    _contextMenu.Items.Add(pollRateMenu);
                }

                // Update tooltip with device info
                UpdateTooltip();
            }

            _contextMenu.Items.Add(new ToolStripSeparator());

            // Exit
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => _desktop?.Shutdown();
            _contextMenu.Items.Add(exitItem);

            if (_notifyIcon != null)
            {
                _notifyIcon.ContextMenuStrip = _contextMenu;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error building context menu");
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
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    vm.RefreshCommand.Execute(null);
                    // Rebuild menu after refresh
                    BuildContextMenu();
                });
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
            if (_notifyIcon == null) return;

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

                _notifyIcon.Text = deviceInfo.Length > 63 ? deviceInfo.Substring(0, 63) : deviceInfo;
            }
            else
            {
                _notifyIcon.Text = "WindowsOpenrazerThing";
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error updating tooltip");
        }
    }

    private void ShowMainWindow()
    {
        try
        {
            if (_desktop?.MainWindow != null)
            {
                _desktop.MainWindow.Show();
                _desktop.MainWindow.WindowState = Avalonia.Controls.WindowState.Normal;
                _desktop.MainWindow.Activate();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error showing main window");
        }
    }

    public void Dispose()
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            if (_contextMenu != null)
            {
                _contextMenu.Dispose();
                _contextMenu = null;
            }

            Logger.Info("Native tray icon disposed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error disposing native tray icon");
        }
    }
}
