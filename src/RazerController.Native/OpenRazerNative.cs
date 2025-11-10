using System.Runtime.InteropServices;

namespace RazerController.Native;

/// <summary>
/// Native structs and types from OpenRazer
/// </summary>
public static class OpenRazerNative
{
    private const string DllName64 = "OpenRazer64.dll";
    private const string DllName32 = "OpenRazer.dll";

    private static string GetDllName() => Environment.Is64BitProcess ? DllName64 : DllName32;

    [DllImport(DllName64, EntryPoint = "init_razer_kbd_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_kbd_driver_64(out IntPtr hdev);

    [DllImport(DllName32, EntryPoint = "init_razer_kbd_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_kbd_driver_32(out IntPtr hdev);

    [DllImport(DllName64, EntryPoint = "init_razer_mouse_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_mouse_driver_64(out IntPtr hdev);

    [DllImport(DllName32, EntryPoint = "init_razer_mouse_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_mouse_driver_32(out IntPtr hdev);

    [DllImport(DllName64, EntryPoint = "init_razer_accessory_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_accessory_driver_64(out IntPtr hdev);

    [DllImport(DllName32, EntryPoint = "init_razer_accessory_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_accessory_driver_32(out IntPtr hdev);

    [DllImport(DllName64, EntryPoint = "init_razer_kraken_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_kraken_driver_64(out IntPtr hdev);

    [DllImport(DllName32, EntryPoint = "init_razer_kraken_driver", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint init_razer_kraken_driver_32(out IntPtr hdev);

    public static uint InitRazerKbdDriver(out IntPtr hdev)
    {
        return Environment.Is64BitProcess 
            ? init_razer_kbd_driver_64(out hdev) 
            : init_razer_kbd_driver_32(out hdev);
    }

    public static uint InitRazerMouseDriver(out IntPtr hdev)
    {
        return Environment.Is64BitProcess 
            ? init_razer_mouse_driver_64(out hdev) 
            : init_razer_mouse_driver_32(out hdev);
    }

    public static uint InitRazerAccessoryDriver(out IntPtr hdev)
    {
        return Environment.Is64BitProcess 
            ? init_razer_accessory_driver_64(out hdev) 
            : init_razer_accessory_driver_32(out hdev);
    }

    public static uint InitRazerKrakenDriver(out IntPtr hdev)
    {
        return Environment.Is64BitProcess 
            ? init_razer_kraken_driver_64(out hdev) 
            : init_razer_kraken_driver_32(out hdev);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct Device
{
    public int attr_count;
    public IntPtr attr_list;
}

[StructLayout(LayoutKind.Sequential)]
public struct HidDevice
{
    public Device dev;
}

[StructLayout(LayoutKind.Sequential)]
public struct DeviceAttribute
{
    public IntPtr name;  // const char*
    public IntPtr show;  // function pointer: ssize_t(*)(struct device*, struct device_attribute*, char*)
    public IntPtr store; // function pointer: ssize_t(*)(struct device*, struct device_attribute*, const char*, size_t)
}
