using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using RazerController.Models;
using RazerController.Native;

namespace RazerController.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly RazerDeviceManager _deviceManager;

    [ObservableProperty]
    private ObservableCollection<DeviceModel> _devices = new();

    [ObservableProperty]
    private DeviceModel? _selectedDevice;

    [ObservableProperty]
    private bool _isInitialized;

    partial void OnSelectedDeviceChanged(DeviceModel? value)
    {
        if (value?.Device != null)
        {
            // Load current DPI value if supported
            if (value.SupportsDPI)
            {
                var currentDpi = value.Device.GetDPI();
                if (currentDpi.HasValue)
                {
                    DpiValue = currentDpi.Value;
                    Logger.Info($"Loaded current DPI: {currentDpi.Value}");
                }
                else
                {
                    Logger.Warn("Failed to read current DPI from device");
                }
            }
            
            // Load current poll rate if supported
            if (value.SupportsPollRate)
            {
                var currentPollRate = value.Device.GetPollRate();
                if (currentPollRate.HasValue)
                {
                    PollRate = currentPollRate.Value;
                    // Set the selected index to match the poll rate
                    for (int i = 0; i < PollRateOptions.Length; i++)
                    {
                        if (PollRateOptions[i] == currentPollRate.Value)
                        {
                            SelectedPollRateIndex = i;
                            break;
                        }
                    }
                    Logger.Info($"Loaded current poll rate: {currentPollRate.Value}Hz");
                }
                else
                {
                    Logger.Warn("Failed to read current poll rate from device");
                }
            }
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
    private int _selectedPollRateIndex = 2; // Default to 1000 Hz (index 2)
    
    // Standard poll rates supported by most Razer devices
    // Note: Some newer devices support 2000Hz, 4000Hz, and 8000Hz, but these require
    // device-specific driver support. The OpenRazer driver defaults to 500Hz for
    // unsupported rates, so we limit options to the most commonly supported rates.
    public int[] PollRateOptions { get; } = new[] { 125, 500, 1000 };

    public Color PreviewColor => Color.FromRgb(RedValue, GreenValue, BlueValue);

    public MainWindowViewModel()
    {
        Logger.Info("Initializing MainWindowViewModel");
        _deviceManager = new RazerDeviceManager();
        Logger.Info("RazerDeviceManager created");
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
    private void SetStaticColor()
    {
        if (SelectedDevice?.Device == null) return;

        try
        {
            Logger.Info($"Setting static color to RGB({RedValue}, {GreenValue}, {BlueValue})");
            bool success = SelectedDevice.Device.SetStaticColor(RedValue, GreenValue, BlueValue);
            
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
            bool success = SelectedDevice.Device.SetSpectrumEffect();
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
            bool success = SelectedDevice.Device.SetBreathEffect(RedValue, GreenValue, BlueValue);
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
            StatusMessage = success ? $"Set DPI to {DpiValue}" : "Failed to set DPI";
            if (success) Logger.Info($"DPI set to {DpiValue} successfully");
            else Logger.Warn($"Failed to set DPI to {DpiValue}");
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
            if (SelectedPollRateIndex >= 0 && SelectedPollRateIndex < PollRateOptions.Length)
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
}
