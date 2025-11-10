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

        bool success = SelectedDevice.Device.SetStaticColor(RedValue, GreenValue, BlueValue);
        if (success)
        {
            SelectedDevice.Device.SetLogoStaticColor(RedValue, GreenValue, BlueValue);
            SelectedDevice.Device.SetScrollStaticColor(RedValue, GreenValue, BlueValue);
            StatusMessage = $"Set color to RGB({RedValue}, {GreenValue}, {BlueValue})";
        }
        else
        {
            StatusMessage = "Failed to set color";
        }
    }

    [RelayCommand]
    private void SetSpectrum()
    {
        if (SelectedDevice?.Device == null) return;

        bool success = SelectedDevice.Device.SetSpectrumEffect();
        StatusMessage = success ? "Set spectrum effect" : "Failed to set spectrum effect";
    }

    [RelayCommand]
    private void SetBreath()
    {
        if (SelectedDevice?.Device == null) return;

        bool success = SelectedDevice.Device.SetBreathEffect(RedValue, GreenValue, BlueValue);
        StatusMessage = success ? "Set breath effect" : "Failed to set breath effect";
    }

    [RelayCommand]
    private void TurnOff()
    {
        if (SelectedDevice?.Device == null) return;

        bool success = SelectedDevice.Device.SetNoneEffect();
        StatusMessage = success ? "Turned off lighting" : "Failed to turn off lighting";
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

        bool success = SelectedDevice.Device.SetDPI(DpiValue);
        StatusMessage = success ? $"Set DPI to {DpiValue}" : "Failed to set DPI";
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

        bool success = SelectedDevice.Device.SetPollRate(PollRate);
        StatusMessage = success ? $"Set poll rate to {PollRate}Hz" : "Failed to set poll rate";
    }
}
