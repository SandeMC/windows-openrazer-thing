using System.Runtime.InteropServices;

namespace RazerController.Native;

public class RazerDeviceManager
{
    private readonly List<RazerDevice> _devices = new();
    private bool _initialized = false;

    public IReadOnlyList<RazerDevice> Devices => _devices.AsReadOnly();

    public bool Initialize()
    {
        if (_initialized)
            return true;

        try
        {
            // Initialize keyboard driver
            uint kbdCount = OpenRazerNative.InitRazerKbdDriver(out IntPtr kbdDevices);
            for (uint i = 0; i < kbdCount; i++)
            {
                IntPtr hdevPtr = kbdDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Keyboard));
            }

            // Initialize mouse driver
            uint mouseCount = OpenRazerNative.InitRazerMouseDriver(out IntPtr mouseDevices);
            for (uint i = 0; i < mouseCount; i++)
            {
                IntPtr hdevPtr = mouseDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Mouse));
            }

            // Initialize accessory driver
            uint accessoryCount = OpenRazerNative.InitRazerAccessoryDriver(out IntPtr accessoryDevices);
            for (uint i = 0; i < accessoryCount; i++)
            {
                IntPtr hdevPtr = accessoryDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Accessory));
            }

            // Initialize headset driver
            uint headsetCount = OpenRazerNative.InitRazerKrakenDriver(out IntPtr headsetDevices);
            for (uint i = 0; i < headsetCount; i++)
            {
                IntPtr hdevPtr = headsetDevices + (int)(i * Marshal.SizeOf<HidDevice>());
                HidDevice hdev = Marshal.PtrToStructure<HidDevice>(hdevPtr);
                IntPtr devPtr = hdevPtr + Marshal.OffsetOf<HidDevice>("dev").ToInt32();
                _devices.Add(new RazerDevice(devPtr, RazerDeviceType.Headset));
            }

            _initialized = true;
            return true;
        }
        catch (Exception)
        {
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
