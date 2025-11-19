using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
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
        if (!_attributes.TryGetValue(name, out var attr))
        {
            Logger.Debug($"Attribute '{name}' not found in device attributes");
            return false;
        }
        
        if (attr.store == IntPtr.Zero)
        {
            Logger.Debug($"Attribute '{name}' has no store function (read-only)");
            return false;
        }

        try
        {
            var storeFunc = Marshal.GetDelegateForFunctionPointer<StoreAttributeDelegate>(attr.store);
            int result = storeFunc(_devicePtr, IntPtr.Zero, data, data.Length);
            Logger.Debug($"WriteAttribute '{name}' returned {result}");
            return result >= 0;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error writing attribute '{name}'");
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
        bool success = false;
        Logger.Debug($"SetStaticColor({r}, {g}, {b})");
        
        // Try all possible static color attributes
        if (HasAttribute("matrix_effect_static"))
            success |= WriteAttribute("matrix_effect_static", new[] { r, g, b });
        if (HasAttribute("logo_matrix_effect_static"))
            success |= WriteAttribute("logo_matrix_effect_static", new[] { r, g, b });
        if (HasAttribute("scroll_matrix_effect_static"))
            success |= WriteAttribute("scroll_matrix_effect_static", new[] { r, g, b });
        if (HasAttribute("backlight_led_effect"))
            success |= WriteAttribute("backlight_led_effect", new[] { r, g, b });
        
        return success;
    }

    public bool SetLogoStaticColor(byte r, byte g, byte b)
    {
        Logger.Debug($"SetLogoStaticColor({r}, {g}, {b})");
        return WriteAttribute("logo_matrix_effect_static", new[] { r, g, b });
    }

    public bool SetScrollStaticColor(byte r, byte g, byte b)
    {
        Logger.Debug($"SetScrollStaticColor({r}, {g}, {b})");
        return WriteAttribute("scroll_matrix_effect_static", new[] { r, g, b });
    }

    public bool SetSpectrumEffect()
    {
        bool success = false;
        Logger.Debug("SetSpectrumEffect()");
        
        if (HasAttribute("matrix_effect_spectrum"))
            success |= WriteAttribute("matrix_effect_spectrum", new byte[] { 0 });
        if (HasAttribute("logo_matrix_effect_spectrum"))
            success |= WriteAttribute("logo_matrix_effect_spectrum", new byte[] { 0 });
        if (HasAttribute("scroll_matrix_effect_spectrum"))
            success |= WriteAttribute("scroll_matrix_effect_spectrum", new byte[] { 0 });
        
        return success;
    }

    public bool SetBreathEffect(byte r, byte g, byte b)
    {
        bool success = false;
        Logger.Debug($"SetBreathEffect({r}, {g}, {b})");
        
        if (HasAttribute("matrix_effect_breath"))
            success |= WriteAttribute("matrix_effect_breath", new[] { r, g, b });
        if (HasAttribute("logo_matrix_effect_breath"))
            success |= WriteAttribute("logo_matrix_effect_breath", new[] { r, g, b });
        if (HasAttribute("scroll_matrix_effect_breath"))
            success |= WriteAttribute("scroll_matrix_effect_breath", new[] { r, g, b });
        
        return success;
    }

    public bool SetNoneEffect()
    {
        bool success = false;
        Logger.Debug("SetNoneEffect()");
        
        if (HasAttribute("matrix_effect_none"))
            success |= WriteAttribute("matrix_effect_none", new byte[] { 0 });
        if (HasAttribute("logo_matrix_effect_none"))
            success |= WriteAttribute("logo_matrix_effect_none", new byte[] { 0 });
        if (HasAttribute("scroll_matrix_effect_none"))
            success |= WriteAttribute("scroll_matrix_effect_none", new byte[] { 0 });
        
        return success;
    }

    public byte? GetBrightness()
    {
        // Try reading from different brightness attributes
        string[] brightnessAttrs = new[] 
        { 
            "matrix_brightness", 
            "logo_led_brightness", 
            "scroll_led_brightness", 
            "backlight_led_brightness",
            "left_led_brightness",
            "right_led_brightness",
            "set_brightness"
        };

        foreach (var attr in brightnessAttrs)
        {
            try
            {
                string? brightnessStr = ReadAttribute(attr);
                if (!string.IsNullOrEmpty(brightnessStr) && byte.TryParse(brightnessStr, out byte brightness))
                {
                    Logger.Debug($"Read brightness from {attr}: {brightness}");
                    return brightness;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Could not read brightness from {attr}");
            }
        }

        return null;
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

        // DPI is sent as 4 bytes: 2 unsigned shorts in big-endian format (X DPI, Y DPI)
        // For example, 800 DPI = 0x0320 = bytes [0x03, 0x20, 0x03, 0x20]
        byte dpiHighByte = (byte)((dpi >> 8) & 0xFF);
        byte dpiLowByte = (byte)(dpi & 0xFF);
        bool success = WriteAttribute("dpi", new[] { dpiHighByte, dpiLowByte, dpiHighByte, dpiLowByte });
        
        // Also update DPI stages if the attribute exists
        if (success && HasAttribute("dpi_stages"))
        {
            SetDPIStages(dpi);
        }
        
        return success;
    }
    
    public bool SetDPIStages(int dpi)
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return false;
        
        if (!HasAttribute("dpi_stages"))
            return false;
        
        // Set stages at 400, 800, 1600, 3200, 6400
        int[] stages = { 400, 800, 1600, 3200, 6400 };
        
        // Find the active stage index based on current DPI
        int activeStageIndex = 0;
        int closestDiff = int.MaxValue;
        for (int i = 0; i < stages.Length; i++)
        {
            int diff = Math.Abs(stages[i] - dpi);
            if (diff < closestDiff)
            {
                closestDiff = diff;
                activeStageIndex = i;
            }
        }
        
        // Format as bytes: count byte + active stage byte + pairs of DPI values (high, low)
        List<byte> stageBytes = new List<byte>();
        stageBytes.Add((byte)stages.Length); // Number of stages
        stageBytes.Add((byte)(activeStageIndex + 1)); // Active stage (1-based index)
        
        foreach (int stageDpi in stages)
        {
            byte highByte = (byte)((stageDpi >> 8) & 0xFF);
            byte lowByte = (byte)(stageDpi & 0xFF);
            stageBytes.Add(highByte);
            stageBytes.Add(lowByte);
        }
        
        Logger.Debug($"Setting DPI stages with active stage {activeStageIndex + 1} (closest to {dpi})");
        return WriteAttribute("dpi_stages", stageBytes.ToArray());
    }

    public int? GetDPI()
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return null;

        try
        {
            if (!_attributes.TryGetValue("dpi", out var attr) || attr.show == IntPtr.Zero)
            {
                Logger.Debug("DPI attribute not found or not readable");
                return null;
            }

            var buffer = new byte[256];
            var showFunc = Marshal.GetDelegateForFunctionPointer<ShowAttributeDelegate>(attr.show);
            int length = showFunc(_devicePtr, IntPtr.Zero, buffer);
            
            Logger.Debug($"GetDPI returned {length} bytes: {BitConverter.ToString(buffer, 0, Math.Min(length, 20))}");
            
            if (length > 0)
            {
                // DPI is returned as a string in format "X:Y\n" (e.g., "800:800\n")
                string dpiStr = Encoding.ASCII.GetString(buffer, 0, length).TrimEnd('\0', '\n', '\r');
                Logger.Debug($"DPI string: '{dpiStr}'");
                
                // Parse the "X:Y" format
                string[] parts = dpiStr.Split(':');
                if (parts.Length >= 1 && int.TryParse(parts[0], out int dpiX))
                {
                    Logger.Debug($"Parsed DPI: X={dpiX}");
                    return dpiX; // Return X DPI (usually they're the same as Y)
                }
                
                Logger.Warn($"Failed to parse DPI string: '{dpiStr}'");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error reading DPI");
        }

        return null;
    }

    public bool SetPollRate(int pollRate)
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return false;

        return WriteAttribute("poll_rate", pollRate.ToString());
    }

    public int? GetPollRate()
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return null;

        try
        {
            if (!_attributes.TryGetValue("poll_rate", out var attr) || attr.show == IntPtr.Zero)
            {
                Logger.Debug("poll_rate attribute not found or not readable");
                return null;
            }

            var buffer = new byte[256];
            var showFunc = Marshal.GetDelegateForFunctionPointer<ShowAttributeDelegate>(attr.show);
            int length = showFunc(_devicePtr, IntPtr.Zero, buffer);
            
            Logger.Debug($"GetPollRate returned {length} bytes: {BitConverter.ToString(buffer, 0, Math.Min(length, 20))}");
            
            if (length > 0)
            {
                // Try to parse as string first (some drivers return it as text)
                string pollRateStr = Encoding.ASCII.GetString(buffer, 0, length).TrimEnd('\0', '\n', '\r');
                if (int.TryParse(pollRateStr, out int pollRate))
                {
                    Logger.Debug($"Parsed poll rate from string: {pollRate}");
                    return pollRate;
                }
                
                // If string parsing fails, try reading as binary (byte value)
                if (length >= 1)
                {
                    int pollRateByte = buffer[0];
                    Logger.Debug($"Parsed poll rate from byte: {pollRateByte}");
                    return pollRateByte;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error reading poll rate");
        }

        return null;
    }

    public List<int>? GetSupportedPollRates()
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return null;

        // Try to read supported_poll_rates attribute if available
        try
        {
            string? ratesStr = ReadAttribute("supported_poll_rates");
            if (!string.IsNullOrEmpty(ratesStr))
            {
                // Parse comma-separated list of poll rates
                var rates = new List<int>();
                foreach (var rateStr in ratesStr.Split(',', ' ', '\n', '\r'))
                {
                    if (int.TryParse(rateStr.Trim(), out int rate) && rate > 0)
                    {
                        rates.Add(rate);
                    }
                }
                if (rates.Count > 0)
                {
                    // Ensure rates don't exceed 8000Hz
                    rates = rates.Where(r => r <= 8000).OrderBy(r => r).ToList();
                    Logger.Debug($"Detected supported poll rates: {string.Join(", ", rates)}");
                    return rates;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Could not read supported_poll_rates attribute");
        }

        // If supported_poll_rates is not available, return full range up to 8000Hz
        Logger.Debug("supported_poll_rates attribute not available, returning full range up to 8000Hz");
        return new List<int> { 125, 250, 500, 1000, 2000, 4000, 8000 };
    }

    public (int activeStage, List<(int x, int y)> stages)? GetDPIStages()
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return null;

        try
        {
            if (!_attributes.TryGetValue("dpi_stages", out var attr) || attr.show == IntPtr.Zero)
            {
                Logger.Debug("dpi_stages attribute not found or not readable");
                return null;
            }

            var buffer = new byte[256];
            var showFunc = Marshal.GetDelegateForFunctionPointer<ShowAttributeDelegate>(attr.show);
            int length = showFunc(_devicePtr, IntPtr.Zero, buffer);
            
            Logger.Debug($"GetDPIStages returned {length} bytes: {BitConverter.ToString(buffer, 0, Math.Min(length, 20))}");
            
            if (length >= 5) // At least 1 byte for active stage + 4 bytes for one stage (2 shorts)
            {
                int activeStage = buffer[0];
                var stages = new List<(int x, int y)>();
                
                // Each stage is 4 bytes: 2 bytes for X DPI (big-endian), 2 bytes for Y DPI (big-endian)
                int offset = 1;
                while (offset + 3 < length)
                {
                    int dpiX = (buffer[offset] << 8) | buffer[offset + 1];
                    int dpiY = (buffer[offset + 2] << 8) | buffer[offset + 3];
                    
                    if (dpiX > 0 || dpiY > 0) // Skip empty stages
                    {
                        stages.Add((dpiX, dpiY));
                    }
                    offset += 4;
                }
                
                Logger.Debug($"Parsed DPI stages: active={activeStage}, stages={string.Join(", ", stages)}");
                return (activeStage, stages);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error reading DPI stages");
        }

        return null;
    }

    public bool SetDPIStages(int activeStage, List<(int x, int y)> stages)
    {
        if (DeviceType != RazerDeviceType.Mouse)
            return false;

        if (stages.Count == 0 || stages.Count > 5 || activeStage < 1 || activeStage > stages.Count)
        {
            Logger.Warn($"Invalid DPI stages: activeStage={activeStage}, stageCount={stages.Count}");
            return false;
        }

        try
        {
            // Format: 1 byte for active stage + 4 bytes per stage (X_high, X_low, Y_high, Y_low)
            var data = new List<byte> { (byte)activeStage };
            
            foreach (var (x, y) in stages)
            {
                data.Add((byte)((x >> 8) & 0xFF));
                data.Add((byte)(x & 0xFF));
                data.Add((byte)((y >> 8) & 0xFF));
                data.Add((byte)(y & 0xFF));
            }
            
            Logger.Debug($"Setting DPI stages: active={activeStage}, stages={string.Join(", ", stages)}");
            return WriteAttribute("dpi_stages", data.ToArray());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error setting DPI stages");
            return false;
        }
    }

    public int? GetBatteryLevel()
    {
        try
        {
            string? levelStr = ReadAttribute("charge_level");
            if (!string.IsNullOrEmpty(levelStr) && int.TryParse(levelStr, out int level))
            {
                Logger.Debug($"Battery level: {level}%");
                return level;
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error reading battery level");
        }
        return null;
    }

    public string? GetBatteryStatus()
    {
        try
        {
            string? status = ReadAttribute("charge_status");
            if (!string.IsNullOrEmpty(status))
            {
                Logger.Debug($"Battery status: {status}");
                return status.Trim();
            }
        }
        catch (Exception ex)
        {
            Logger.Debug(ex, "Error reading battery status");
        }
        return null;
    }

    public bool GetIsCharging()
    {
        string? status = GetBatteryStatus();
        return status != null && (status.Contains("charging", StringComparison.OrdinalIgnoreCase) || 
                                   status.Contains("full", StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyDictionary<string, DeviceAttribute> GetAllAttributes() => _attributes;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ShowAttributeDelegate(IntPtr dev, IntPtr attr, [Out] byte[] buf);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int StoreAttributeDelegate(IntPtr dev, IntPtr attr, [In] byte[] buf, int count);
}
