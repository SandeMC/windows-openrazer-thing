using System.Runtime.InteropServices;
using System.Text;
using NLog;

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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IntPtr _devicePtr;
    private readonly Device _device;
    private readonly Dictionary<string, DeviceAttribute> _attributes;

    public RazerDeviceType DeviceType { get; }
    public string? DeviceTypeName { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? FirmwareVersion { get; private set; }

    internal RazerDevice(IntPtr devicePtr, RazerDeviceType deviceType)
    {
        Logger.Debug($"Creating {deviceType} device from pointer {devicePtr:X}");
        _devicePtr = devicePtr;
        DeviceType = deviceType;
        
        try
        {
            _device = Marshal.PtrToStructure<Device>(devicePtr);
            Logger.Debug($"Device structure marshalled successfully. attr_count={_device.attr_count}, attr_list pointer={_device.attr_list:X}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Failed to marshal Device structure from pointer {devicePtr:X}");
            throw new InvalidOperationException($"Failed to marshal Device structure from pointer {devicePtr:X}", ex);
        }
        
        _attributes = LoadAttributes();
        Logger.Debug($"Loaded {_attributes.Count} attributes");
        LoadDeviceInfo();
        Logger.Debug($"Device info loaded: Type={DeviceTypeName}, Serial={SerialNumber}");
    }

    private Dictionary<string, DeviceAttribute> LoadAttributes()
    {
        var attributes = new Dictionary<string, DeviceAttribute>();
        
        // Check if attr_list array is null
        if (_device.attr_list == null)
        {
            Logger.Warn("attr_list array is null, returning empty attributes dictionary");
            return attributes;
        }
        
        // The native attr_list array has a maximum size of 64 elements
        const int MAX_ATTR_LIST_SIZE = 64;
        int actualCount = Math.Min((int)_device.attr_count, MAX_ATTR_LIST_SIZE);
        Logger.Debug($"Loading attributes: attr_count={_device.attr_count}, attr_list array length={_device.attr_list.Length}, reading {actualCount} attributes");
        
        for (int i = 0; i < actualCount; i++)
        {
            try
            {
                Logger.Trace($"Reading attribute pointer at index {i}");
                // Access the pointer directly from the array
                IntPtr attrPtr = _device.attr_list[i];
                Logger.Trace($"Attribute {i}: pointer = {attrPtr:X}");
                
                if (attrPtr != IntPtr.Zero)
                {
                    try
                    {
                        Logger.Trace($"Marshalling DeviceAttribute structure at {attrPtr:X}");
                        var attr = Marshal.PtrToStructure<DeviceAttribute>(attrPtr);
                        Logger.Trace($"Attribute {i}: name pointer = {attr.name:X}, show = {attr.show:X}, store = {attr.store:X}");
                        
                        if (attr.name != IntPtr.Zero)
                        {
                            Logger.Trace($"Reading attribute name from pointer {attr.name:X16}");
                            try
                            {
                                string? name = Marshal.PtrToStringAnsi(attr.name);
                                if (name != null && !string.IsNullOrWhiteSpace(name))
                                {
                                    Logger.Debug($"Loaded attribute {i}: {name}");
                                    attributes[name] = attr;
                                }
                                else
                                {
                                    Logger.Warn($"Attribute {i} has null or empty name string (read from {attr.name:X16})");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, $"Failed to read name string from pointer {attr.name:X16} for attribute {i}");
                            }
                        }
                        else
                        {
                            Logger.Warn($"Attribute {i} has null name pointer");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"Failed to load attribute at index {i}, pointer {attrPtr:X}. Skipping.");
                        continue;
                    }
                }
                else
                {
                    Logger.Trace($"Attribute {i}: null pointer, skipping");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Critical error reading attribute pointer at index {i}");
                throw;
            }
        }

        Logger.Debug($"Successfully loaded {attributes.Count} attributes");
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
        // Try different brightness attributes based on what the device supports
        bool success = false;
        
        if (HasAttribute("matrix_brightness"))
            success |= WriteAttribute("matrix_brightness", brightness.ToString());
        
        if (HasAttribute("logo_led_brightness"))
            success |= WriteAttribute("logo_led_brightness", brightness.ToString());
            
        if (HasAttribute("scroll_led_brightness"))
            success |= WriteAttribute("scroll_led_brightness", brightness.ToString());
            
        if (HasAttribute("backlight_led_brightness"))
            success |= WriteAttribute("backlight_led_brightness", brightness.ToString());
            
        if (HasAttribute("left_led_brightness"))
            success |= WriteAttribute("left_led_brightness", brightness.ToString());
            
        if (HasAttribute("right_led_brightness"))
            success |= WriteAttribute("right_led_brightness", brightness.ToString());
            
        if (HasAttribute("set_brightness"))
            success |= WriteAttribute("set_brightness", brightness.ToString());
        
        return success;
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
