using System.Runtime.InteropServices;
using System.Text;

namespace RazerController.Native;

public enum RazerDeviceType
{
    Keyboard,
    Mouse,
    Accessory,
    Headset
}

public class RazerDevice
{
    private readonly IntPtr _devicePtr;
    private readonly Device _device;
    private readonly Dictionary<string, DeviceAttribute> _attributes;

    public RazerDeviceType DeviceType { get; }
    public string? DeviceTypeName { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? FirmwareVersion { get; private set; }

    internal RazerDevice(IntPtr devicePtr, RazerDeviceType deviceType)
    {
        _devicePtr = devicePtr;
        DeviceType = deviceType;
        
        try
        {
            _device = Marshal.PtrToStructure<Device>(devicePtr);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to marshal Device structure from pointer {devicePtr:X}", ex);
        }
        
        _attributes = LoadAttributes();
        LoadDeviceInfo();
    }

    private Dictionary<string, DeviceAttribute> LoadAttributes()
    {
        var attributes = new Dictionary<string, DeviceAttribute>();
        
        // Check if attr_list array is null
        if (_device.attr_list == null)
        {
            return attributes;
        }
        
        // Ensure we don't go beyond array bounds
        int count = (int)Math.Min(_device.attr_count, (uint)_device.attr_list.Length);
        
        for (int i = 0; i < count; i++)
        {
            IntPtr attrPtr = _device.attr_list[i];
            if (attrPtr != IntPtr.Zero)
            {
                try
                {
                    var attr = Marshal.PtrToStructure<DeviceAttribute>(attrPtr);
                    if (attr.name != IntPtr.Zero)
                    {
                        string? name = Marshal.PtrToStringAnsi(attr.name);
                        if (name != null)
                        {
                            attributes[name] = attr;
                        }
                    }
                }
                catch
                {
                    // Skip invalid attribute pointers
                    continue;
                }
            }
        }

        return attributes;
    }

    private void LoadDeviceInfo()
    {
        DeviceTypeName = ReadAttribute("device_type");
        SerialNumber = ReadAttribute("device_serial");
        FirmwareVersion = ReadAttribute("firmware_version");
    }

    public bool HasAttribute(string name) => _attributes.ContainsKey(name);

    public string? ReadAttribute(string name)
    {
        if (!_attributes.TryGetValue(name, out var attr) || attr.show == IntPtr.Zero)
            return null;

        try
        {
            var buffer = new byte[256];
            var showFunc = Marshal.GetDelegateForFunctionPointer<ShowAttributeDelegate>(attr.show);
            int length = showFunc(_devicePtr, IntPtr.Zero, buffer);
            
            if (length > 0 && length <= buffer.Length)
            {
                return Encoding.ASCII.GetString(buffer, 0, length).TrimEnd('\0', '\n', '\r');
            }
        }
        catch
        {
            // Ignore read errors
        }

        return null;
    }

    public bool WriteAttribute(string name, byte[] data)
    {
        if (!_attributes.TryGetValue(name, out var attr) || attr.store == IntPtr.Zero)
            return false;

        try
        {
            var storeFunc = Marshal.GetDelegateForFunctionPointer<StoreAttributeDelegate>(attr.store);
            int result = storeFunc(_devicePtr, IntPtr.Zero, data, data.Length);
            return result >= 0;
        }
        catch
        {
            return false;
        }
    }

    public bool WriteAttribute(string name, string value)
    {
        return WriteAttribute(name, Encoding.ASCII.GetBytes(value));
    }

    // RGB control methods
    public bool SetStaticColor(byte r, byte g, byte b)
    {
        return WriteAttribute("matrix_effect_static", new[] { r, g, b });
    }

    public bool SetLogoStaticColor(byte r, byte g, byte b)
    {
        return WriteAttribute("logo_matrix_effect_static", new[] { r, g, b });
    }

    public bool SetScrollStaticColor(byte r, byte g, byte b)
    {
        return WriteAttribute("scroll_matrix_effect_static", new[] { r, g, b });
    }

    public bool SetSpectrumEffect()
    {
        return WriteAttribute("matrix_effect_spectrum", new byte[] { 0 });
    }

    public bool SetBreathEffect(byte r, byte g, byte b)
    {
        return WriteAttribute("matrix_effect_breath", new[] { r, g, b });
    }

    public bool SetNoneEffect()
    {
        return WriteAttribute("matrix_effect_none", new byte[] { 0 });
    }

    public bool SetBrightness(byte brightness)
    {
        return WriteAttribute("matrix_brightness", brightness.ToString());
    }

    // Mouse-specific methods
    public bool SetDPI(int dpi)
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return false;

        // DPI is sent as two bytes (X and Y DPI, usually the same)
        byte dpiByte = (byte)(dpi / 100);
        return WriteAttribute("dpi", new[] { dpiByte, dpiByte });
    }

    public string? GetDPI()
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return null;

        return ReadAttribute("dpi");
    }

    public bool SetPollRate(int pollRate)
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return false;

        return WriteAttribute("poll_rate", pollRate.ToString());
    }

    public string? GetPollRate()
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return null;

        return ReadAttribute("poll_rate");
    }

    public IReadOnlyDictionary<string, DeviceAttribute> GetAllAttributes() => _attributes;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ShowAttributeDelegate(IntPtr dev, IntPtr attr, [Out] byte[] buf);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int StoreAttributeDelegate(IntPtr dev, IntPtr attr, [In] byte[] buf, int count);
}
