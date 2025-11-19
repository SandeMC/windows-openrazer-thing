using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using RazerController.Models;
using RazerController.Native;
using RazerController.Services;

namespace RazerController.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly RazerDeviceManager _deviceManager;
    private readonly WindowsMouseSettingsService _mouseSettingsService;
    private CancellationTokenSource? _pollingCancellation;
    private Task? _pollingTask;

    [ObservableProperty]
    private bool _isWindowActive = true;

    [ObservableProperty]
    private ObservableCollection<DeviceModel> _devices = new();

    [ObservableProperty]
    private DeviceModel? _selectedDevice;

    [ObservableProperty]
    private bool _isInitialized;

    partial void OnSelectedDeviceChanged(DeviceModel? value)
    {
        // Stop any existing polling (polling disabled per requirements)
        StopDevicePolling();
        
        if (value?.Device != null)
        {
            // Load all device values initially
            LoadDeviceValues(value);
            
            // No longer start automatic polling - user must use refresh button
        }
    }

    private void LoadDeviceValues(DeviceModel device)
    {
        // Load supported poll rates dynamically
        if (device.SupportsPollRate)
        {
            var supportedRates = device.Device.GetSupportedPollRates();
            if (supportedRates != null && supportedRates.Count > 0)
            {
                PollRateOptions.Clear();
                foreach (var rate in supportedRates)
                {
                    PollRateOptions.Add(rate);
                }
                PollRateMaxIndex = supportedRates.Count - 1;
                PollRateTicks = string.Join(",", Enumerable.Range(0, supportedRates.Count));
                Logger.Info($"Loaded {supportedRates.Count} supported poll rates");
            }
        }
        
        // Load current DPI value if supported
        if (device.SupportsDPI)
        {
            var currentDpi = device.Device.GetDPI();
            if (currentDpi.HasValue)
            {
                DpiValue = currentDpi.Value;
                Logger.Info($"Current DPI: {currentDpi.Value}");
            }
        }
        
        // Load current poll rate if supported
        if (device.SupportsPollRate)
        {
            var currentPollRate = device.Device.GetPollRate();
            if (currentPollRate.HasValue)
            {
                PollRate = currentPollRate.Value;
                // Set the selected index to match the poll rate
                for (int i = 0; i < PollRateOptions.Count; i++)
                {
                    if (PollRateOptions[i] == currentPollRate.Value)
                    {
                        SelectedPollRateIndex = i;
                        break;
                    }
                }
                Logger.Info($"Current poll rate: {currentPollRate.Value}Hz");
            }
        }
        
        // Load current brightness if supported
        if (device.SupportsBrightness)
        {
            var currentBrightness = device.Device.GetBrightness();
            if (currentBrightness.HasValue)
            {
                Brightness = currentBrightness.Value;
            }
        }
        
        // Load battery info if supported
        if (device.SupportsBattery)
        {
            BatteryLevel = device.Device.GetBatteryLevel();
            BatteryStatus = device.Device.GetBatteryStatus();
            IsCharging = device.Device.GetIsCharging();
            if (BatteryLevel.HasValue)
            {
                Logger.Info($"Battery: {BatteryLevel}%{(IsCharging ? " (Charging)" : "")}");
            }
        }
        else
        {
            BatteryLevel = null;
            BatteryStatus = null;
            IsCharging = false;
        }
    }

    [ObservableProperty]
    private string _statusMessage = "Click 'Initialize' to detect Razer devices";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewColor))]
    private byte _redValue = 255;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewColor))]
    private byte _greenValue = 255;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewColor))]
    private byte _blueValue = 255;

    [ObservableProperty]
    private byte _brightness = 255;

    [ObservableProperty]
    private int _dpiValue = 800;

    [ObservableProperty]
    private int _pollRate = 1000;
    
    [ObservableProperty]
    private int _selectedPollRateIndex = 0;
    
    [ObservableProperty]
    private int _pollRateSliderIndex = 0;
    
    [ObservableProperty]
    private int _pollRateMaxIndex = 0;
    
    [ObservableProperty]
    private string _pollRateTicks = "0";
    
    [ObservableProperty]
    private ObservableCollection<int> _pollRateOptions = new();
    
    partial void OnPollRateSliderIndexChanged(int value)
    {
        if (value >= 0 && value < PollRateOptions.Count)
        {
            SelectedPollRateIndex = value;
        }
    }
    
    partial void OnSelectedPollRateIndexChanged(int value)
    {
        PollRateSliderIndex = value;
    }
    
    [ObservableProperty]
    private int? _batteryLevel;
    
    [ObservableProperty]
    private string? _batteryStatus;
    
    [ObservableProperty]
    private bool _isCharging;
    
    [ObservableProperty]
    private int _windowsSensitivity = 10; // Default Windows sensitivity (1-20)
    
    [ObservableProperty]
    private bool _windowsMouseAcceleration = true;
    
    partial void OnWindowsMouseAccelerationChanged(bool value)
    {
        // When the checkbox is toggled by user, automatically apply the setting
        if (_mouseSettingsService != null)
        {
            SetMouseAccelerationCommand.Execute(null);
        }
    }

    public Color PreviewColor => Color.FromRgb(RedValue, GreenValue, BlueValue);

    public MainWindowViewModel()
    {
        Logger.Info("Initializing MainWindowViewModel");
        _deviceManager = new RazerDeviceManager();
        _mouseSettingsService = new WindowsMouseSettingsService();
        Logger.Info("RazerDeviceManager and MouseSettingsService created");
        
        // Load current Windows mouse settings
        LoadWindowsMouseSettings();
        
        // Auto-initialize devices on startup
        Task.Run(() => 
        {
            // Small delay to ensure UI is ready
            Task.Delay(500).Wait();
            Avalonia.Threading.Dispatcher.UIThread.Post(() => Initialize());
        });
    }
    
    private void LoadWindowsMouseSettings()
    {
        var sensitivity = _mouseSettingsService.GetMouseSensitivity();
        if (sensitivity.HasValue)
        {
            WindowsSensitivity = sensitivity.Value;
            Logger.Info($"Loaded Windows mouse sensitivity: {sensitivity.Value}");
        }
        
        var acceleration = _mouseSettingsService.GetMouseAcceleration();
        if (acceleration.HasValue)
        {
            WindowsMouseAcceleration = acceleration.Value;
            Logger.Info($"Loaded Windows mouse acceleration: {acceleration.Value}");
        }
    }

    [RelayCommand]
    private void Initialize()
    {
        try
        {
            Logger.Info("Initialize command invoked - attempting to detect Razer devices");
            bool success = _deviceManager.Initialize();
            if (success)
            {
                Logger.Info("Device manager initialized successfully");
                Devices.Clear();
                foreach (var device in _deviceManager.Devices)
                {
                    Devices.Add(new DeviceModel(device));
                    Logger.Info($"Found device: {device.DeviceTypeName} (Type: {device.DeviceType})");
                }

                IsInitialized = true;
                StatusMessage = $"Found {Devices.Count} Razer device(s)";
                Logger.Info($"Total devices found: {Devices.Count}");
                
                if (Devices.Count > 0)
                {
                    SelectedDevice = Devices[0];
                    Logger.Info($"Selected first device: {SelectedDevice.Name}");
                }
            }
            else
            {
                Logger.Warn("Device manager initialization failed");
                StatusMessage = "Failed to initialize. Make sure OpenRazer DLL is present.";
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during device initialization");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        _deviceManager.Refresh();
        Devices.Clear();
        foreach (var device in _deviceManager.Devices)
        {
            Devices.Add(new DeviceModel(device));
        }
        StatusMessage = $"Refreshed. Found {Devices.Count} device(s)";
    }
    
    [RelayCommand]
    private void RefreshDeviceValues()
    {
        if (SelectedDevice?.Device != null)
        {
            Logger.Info("Refreshing device values for selected device");
            LoadDeviceValues(SelectedDevice);
            StatusMessage = "Device values refreshed";
        }
    }

    [RelayCommand]
    private void SetStaticColor()
    {
        if (SelectedDevice?.Device == null) return;

        try
        {
            Logger.Info($"Setting static color to RGB({RedValue}, {GreenValue}, {BlueValue})");
            
            // First ensure brightness is set to enable RGB if it was off
            if (SelectedDevice.SupportsBrightness && Brightness == 0)
            {
                Brightness = 255;
                SelectedDevice.Device.SetBrightness(Brightness);
            }
            
            bool success = SelectedDevice.Device.SetStaticColor(RedValue, GreenValue, BlueValue);
            
            // Apply brightness after setting effect to ensure visibility
            if (success && SelectedDevice.SupportsBrightness)
            {
                SelectedDevice.Device.SetBrightness(Brightness);
            }
            
            if (success)
            {
                StatusMessage = $"Set color to RGB({RedValue}, {GreenValue}, {BlueValue})";
                Logger.Info("Static color set successfully");
            }
            else
            {
                StatusMessage = "Failed to set color - attribute may not be supported";
                Logger.Warn("Failed to set static color");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting static color");
            StatusMessage = $"Error setting color: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SetSpectrum()
    {
        if (SelectedDevice?.Device == null) return;

        try
        {
            Logger.Info("Setting spectrum effect");
            
            // First ensure brightness is set to enable RGB if it was off
            if (SelectedDevice.SupportsBrightness && Brightness == 0)
            {
                Brightness = 255;
                SelectedDevice.Device.SetBrightness(Brightness);
            }
            
            bool success = SelectedDevice.Device.SetSpectrumEffect();
            
            // Apply brightness after setting effect to ensure visibility
            if (success && SelectedDevice.SupportsBrightness)
            {
                SelectedDevice.Device.SetBrightness(Brightness);
            }
            
            StatusMessage = success ? "Set spectrum effect" : "Failed to set spectrum effect - attribute may not be supported";
            if (success) Logger.Info("Spectrum effect set successfully");
            else Logger.Warn("Failed to set spectrum effect");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting spectrum effect");
            StatusMessage = $"Error setting spectrum effect: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SetBreath()
    {
        if (SelectedDevice?.Device == null) return;

        try
        {
            Logger.Info($"Setting breath effect with RGB({RedValue}, {GreenValue}, {BlueValue})");
            
            // First ensure brightness is set to enable RGB if it was off
            if (SelectedDevice.SupportsBrightness && Brightness == 0)
            {
                Brightness = 255;
                SelectedDevice.Device.SetBrightness(Brightness);
            }
            
            bool success = SelectedDevice.Device.SetBreathEffect(RedValue, GreenValue, BlueValue);
            
            // Apply brightness after setting effect to ensure visibility
            if (success && SelectedDevice.SupportsBrightness)
            {
                SelectedDevice.Device.SetBrightness(Brightness);
            }
            
            StatusMessage = success ? "Set breath effect" : "Failed to set breath effect - attribute may not be supported";
            if (success) Logger.Info("Breath effect set successfully");
            else Logger.Warn("Failed to set breath effect");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting breath effect");
            StatusMessage = $"Error setting breath effect: {ex.Message}";
        }
    }

    [RelayCommand]
    private void TurnOff()
    {
        if (SelectedDevice?.Device == null) return;

        try
        {
            Logger.Info("Turning off lighting");
            bool success = SelectedDevice.Device.SetNoneEffect();
            
            // As a failsafe, also try setting brightness to 0
            if (!success || true) // Always try brightness as backup
            {
                Logger.Info("Also setting brightness to 0 as failsafe");
                SelectedDevice.Device.SetBrightness(0);
            }
            
            StatusMessage = success ? "Turned off lighting" : "Failed to turn off lighting - tried setting brightness to 0 as failsafe";
            if (success) Logger.Info("Lighting turned off successfully");
            else Logger.Warn("Failed to turn off lighting");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error turning off lighting");
            StatusMessage = $"Error turning off lighting: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SetBrightness()
    {
        if (SelectedDevice?.Device == null) return;

        bool success = SelectedDevice.Device.SetBrightness(Brightness);
        StatusMessage = success ? $"Set brightness to {Brightness}" : "Failed to set brightness";
    }

    [RelayCommand]
    private void SetDPI()
    {
        if (SelectedDevice?.Device == null) return;
        if (SelectedDevice.Device.DeviceType != RazerDeviceType.Mouse)
        {
            StatusMessage = "DPI can only be set on mice";
            return;
        }

        try
        {
            Logger.Info($"Setting DPI to {DpiValue}");
            bool success = SelectedDevice.Device.SetDPI(DpiValue);
            
            if (success)
            {
                Logger.Info($"DPI write command succeeded for {DpiValue}");
                
                // Verify by reading back the DPI
                var actualDpi = SelectedDevice.Device.GetDPI();
                if (actualDpi.HasValue)
                {
                    Logger.Info($"Verified DPI: {actualDpi.Value}");
                    if (actualDpi.Value == DpiValue)
                    {
                        StatusMessage = $"DPI set to {DpiValue}";
                    }
                    else
                    {
                        StatusMessage = $"DPI set, but device reports {actualDpi.Value} (requested {DpiValue})";
                        Logger.Warn($"DPI mismatch: requested {DpiValue}, device reports {actualDpi.Value}");
                    }
                }
                else
                {
                    StatusMessage = $"DPI command sent, but could not verify";
                    Logger.Warn("Failed to read back DPI after setting");
                }
            }
            else
            {
                StatusMessage = "Failed to set DPI";
                Logger.Warn($"Failed to set DPI to {DpiValue}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error setting DPI to {DpiValue}");
            StatusMessage = $"Error setting DPI: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SetPollRate()
    {
        if (SelectedDevice?.Device == null) return;
        if (SelectedDevice.Device.DeviceType != RazerDeviceType.Mouse)
        {
            StatusMessage = "Poll rate can only be set on mice";
            return;
        }

        try
        {
            // Get the poll rate from the selected index
            if (SelectedPollRateIndex >= 0 && SelectedPollRateIndex < PollRateOptions.Count)
            {
                int pollRate = PollRateOptions[SelectedPollRateIndex];
                Logger.Info($"Setting poll rate to {pollRate}Hz");
                bool success = SelectedDevice.Device.SetPollRate(pollRate);
                
                if (success)
                {
                    Logger.Info($"Poll rate write command succeeded for {pollRate}Hz");
                    
                    // Verify by reading back the poll rate
                    var actualPollRate = SelectedDevice.Device.GetPollRate();
                    if (actualPollRate.HasValue)
                    {
                        Logger.Info($"Verified poll rate: {actualPollRate.Value}Hz");
                        if (actualPollRate.Value == pollRate)
                        {
                            StatusMessage = $"Poll rate set to {pollRate}Hz";
                        }
                        else
                        {
                            StatusMessage = $"Poll rate set, but device reports {actualPollRate.Value}Hz (requested {pollRate}Hz)";
                            Logger.Warn($"Poll rate mismatch: requested {pollRate}Hz, device reports {actualPollRate.Value}Hz");
                        }
                    }
                    else
                    {
                        StatusMessage = $"Poll rate command sent, but could not verify";
                        Logger.Warn("Failed to read back poll rate after setting");
                    }
                }
                else
                {
                    StatusMessage = "Failed to set poll rate";
                    Logger.Warn($"Failed to set poll rate to {pollRate}Hz");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting poll rate");
            StatusMessage = $"Error setting poll rate: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void SetWindowsSensitivity()
    {
        try
        {
            Logger.Info($"Setting Windows mouse sensitivity to {WindowsSensitivity}");
            bool success = _mouseSettingsService.SetMouseSensitivity(WindowsSensitivity);
            
            if (success)
            {
                StatusMessage = $"Windows mouse sensitivity set to {WindowsSensitivity}";
                Logger.Info("Windows mouse sensitivity set successfully");
            }
            else
            {
                StatusMessage = "Failed to set Windows mouse sensitivity";
                Logger.Warn("Failed to set Windows mouse sensitivity");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting Windows mouse sensitivity");
            StatusMessage = $"Error setting sensitivity: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private void SetMouseAcceleration()
    {
        try
        {
            Logger.Info($"Setting Windows mouse acceleration to {WindowsMouseAcceleration}");
            bool success = _mouseSettingsService.SetMouseAcceleration(WindowsMouseAcceleration);
            
            if (success)
            {
                StatusMessage = $"Windows mouse acceleration {(WindowsMouseAcceleration ? "enabled" : "disabled")}";
                Logger.Info("Windows mouse acceleration set successfully");
            }
            else
            {
                StatusMessage = "Failed to set Windows mouse acceleration";
                Logger.Warn("Failed to set Windows mouse acceleration");
                // Reload the actual current value
                LoadWindowsMouseSettings();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting Windows mouse acceleration");
            StatusMessage = $"Error setting acceleration: {ex.Message}";
            // Reload the actual current value
            LoadWindowsMouseSettings();
        }
    }

    private void StartDevicePolling()
    {
        _pollingCancellation = new CancellationTokenSource();
        _pollingTask = Task.Run(async () =>
        {
            try
            {
                while (!_pollingCancellation.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, _pollingCancellation.Token); // Poll every 1 second
                    
                    // Only poll if window is active/in foreground
                    if (IsWindowActive && SelectedDevice?.Device != null)
                    {
                        // Update device values on UI thread
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            try
                            {
                                RefreshDeviceValuesInternal();
                            }
                            catch (Exception ex)
                            {
                                Logger.Debug(ex, "Error refreshing device values during polling");
                            }
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("Device polling cancelled");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in device polling loop");
            }
        }, _pollingCancellation.Token);
    }

    private void StopDevicePolling()
    {
        _pollingCancellation?.Cancel();
        _pollingTask?.Wait(TimeSpan.FromSeconds(1));
        _pollingCancellation?.Dispose();
        _pollingCancellation = null;
        _pollingTask = null;
    }

    private void RefreshDeviceValuesInternal()
    {
        if (SelectedDevice?.Device == null) return;

        try
        {
            // Refresh DPI if supported
            if (SelectedDevice.SupportsDPI)
            {
                var currentDpi = SelectedDevice.Device.GetDPI();
                if (currentDpi.HasValue && currentDpi.Value != DpiValue)
                {
                    DpiValue = currentDpi.Value;
                }
            }

            // Refresh poll rate if supported
            if (SelectedDevice.SupportsPollRate)
            {
                var currentPollRate = SelectedDevice.Device.GetPollRate();
                if (currentPollRate.HasValue && currentPollRate.Value != PollRate)
                {
                    PollRate = currentPollRate.Value;
                    // Update selected index
                    for (int i = 0; i < PollRateOptions.Count; i++)
                    {
                        if (PollRateOptions[i] == currentPollRate.Value)
                        {
                            SelectedPollRateIndex = i;
                            break;
                        }
                    }
                }
            }

            // Refresh brightness if supported
            if (SelectedDevice.SupportsBrightness)
            {
                var currentBrightness = SelectedDevice.Device.GetBrightness();
                if (currentBrightness.HasValue && currentBrightness.Value != Brightness)
                {
                    Brightness = currentBrightness.Value;
                }
            }

            // Refresh battery info if supported
            if (SelectedDevice.SupportsBattery)
            {
                var batteryLevel = SelectedDevice.Device.GetBatteryLevel();
                var batteryStatus = SelectedDevice.Device.GetBatteryStatus();
                var isCharging = SelectedDevice.Device.GetIsCharging();

                if (batteryLevel != BatteryLevel || batteryStatus != BatteryStatus || isCharging != IsCharging)
                {
                    BatteryLevel = batteryLevel;
                    BatteryStatus = batteryStatus;
                    IsCharging = isCharging;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error refreshing device values");
        }
    }
}
