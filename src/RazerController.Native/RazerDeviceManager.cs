using System.Runtime.InteropServices;
using NLog;

namespace RazerController.Native;

public class RazerDeviceManager
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly List<RazerDevice> _devices = new();
    private bool _initialized = false;

    public IReadOnlyList<RazerDevice> Devices => _devices.AsReadOnly();

    public bool Initialize()
    {
        if (_initialized)
        {
            Logger.Info("Device manager already initialized");
            return true;
        }

        try
        {
            Logger.Info("Starting device manager initialization");
            
            // Check if DLL exists
            string dllName = Environment.Is64BitProcess ? "OpenRazer64.dll" : "OpenRazer.dll";
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string dllPath = Path.Combine(appDir, dllName);
            
            Logger.Info($"Looking for DLL: {dllPath}");
            Logger.Info($"DLL exists: {File.Exists(dllPath)}");
            
            if (!File.Exists(dllPath))
            {
                Logger.Error($"OpenRazer DLL not found at expected location: {dllPath}");
                Logger.Info($"Application directory contents:");
                foreach (var file in Directory.GetFiles(appDir, "*.dll"))
                {
                    Logger.Info($"  Found DLL: {Path.GetFileName(file)}");
                }
                return false;
            }
            
            // Initialize keyboard driver
            Logger.Debug("Attempting to initialize keyboard driver");
            uint kbdCount = OpenRazerNative.InitRazerKbdDriver(out IntPtr kbdDevices);
            Logger.Info($"Keyboard driver initialized: found {kbdCount} device(s)");
            
            for (uint i = 0; i < kbdCount; i++)
            {
                IntPtr hdevPtr = kbdDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Keyboard));
                Logger.Debug($"Added keyboard device {i + 1}");
            }

            // Initialize mouse driver
            Logger.Debug("Attempting to initialize mouse driver");
            uint mouseCount = OpenRazerNative.InitRazerMouseDriver(out IntPtr mouseDevices);
            Logger.Info($"Mouse driver initialized: found {mouseCount} device(s)");
            
            for (uint i = 0; i < mouseCount; i++)
            {
                IntPtr hdevPtr = mouseDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Mouse));
                Logger.Debug($"Added mouse device {i + 1}");
            }

            // Initialize accessory driver
            Logger.Debug("Attempting to initialize accessory driver");
            uint accessoryCount = OpenRazerNative.InitRazerAccessoryDriver(out IntPtr accessoryDevices);
            Logger.Info($"Accessory driver initialized: found {accessoryCount} device(s)");
            
            for (uint i = 0; i < accessoryCount; i++)
            {
                IntPtr hdevPtr = accessoryDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Accessory));
                Logger.Debug($"Added accessory device {i + 1}");
            }

            // Initialize headset driver
            Logger.Debug("Attempting to initialize headset driver");
            uint headsetCount = OpenRazerNative.InitRazerKrakenDriver(out IntPtr headsetDevices);
            Logger.Info($"Headset driver initialized: found {headsetCount} device(s)");
            
            for (uint i = 0; i < headsetCount; i++)
            {
                IntPtr hdevPtr = headsetDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Headset));
                Logger.Debug($"Added headset device {i + 1}");
            }

            _initialized = true;
            Logger.Info($"Device manager initialization complete. Total devices: {_devices.Count}");
            return _devices.Count > 0 || true; // Return true even if no devices, as DLL loaded successfully
        }
        catch (DllNotFoundException dllEx)
        {
            Logger.Error(dllEx, "OpenRazer DLL not found. Make sure OpenRazer64.dll is in the application directory");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during device manager initialization");
            return false;
        }
    }

    public void Refresh()
    {
        _devices.Clear();
        _initialized = false;
        Initialize();
    }
}
