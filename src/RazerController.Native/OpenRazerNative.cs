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
    public IntPtr parent;           // struct device* parent
    public IntPtr p;                // void* p
    public IntPtr init_name;        // const char* init_name
    public IntPtr bus;              // struct bus_type* bus
    public IntPtr driver_data;      // void* driver_data
    public uint attr_count;         // unsigned int attr_count
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public IntPtr[] attr_list;      // struct device_attribute* attr_list[64] - inline array of 64 pointers
    public IntPtr parent_usb_interface; // struct usb_interface* parent_usb_interface
}

[StructLayout(LayoutKind.Sequential)]
public struct HidDevice
{
    public ushort product;          // __u16 product
    public int type;                // enum hid_type type (enums are typically int)
    public Device dev;              // struct device dev
    public IntPtr ll_driver;        // struct hid_ll_driver *ll_driver
    public uint status;             // unsigned int status
    public IntPtr driver;           // struct hid_driver *driver
}

[StructLayout(LayoutKind.Sequential)]
public struct DeviceAttribute
{
    public IntPtr name;  // const char*
    public IntPtr show;  // function pointer: ssize_t(*)(struct device*, struct device_attribute*, char*)
    public IntPtr store; // function pointer: ssize_t(*)(struct device*, struct device_attribute*, const char*, size_t)
}
