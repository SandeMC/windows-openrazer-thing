using RazerController.Native;

namespace RazerController.Models;

public class DeviceModel
{
    public RazerDevice Device { get; }
    
    public string Name => Device.DeviceTypeName ?? "Unknown Device";
    public string DeviceType => Device.DeviceType.ToString();
    public string SerialNumber => Device.SerialNumber ?? "N/A";
    public string FirmwareVersion => Device.FirmwareVersion ?? "N/A";
    
    public bool SupportsDPI => Device.DeviceType == RazerDeviceType.Mouse && Device.HasAttribute("dpi");
    public bool SupportsDPIStages => Device.DeviceType == RazerDeviceType.Mouse && Device.HasAttribute("dpi_stages");
    public bool SupportsPollRate => Device.DeviceType == RazerDeviceType.Mouse && Device.HasAttribute("poll_rate");
    public bool SupportsBattery => Device.HasAttribute("charge_level") || Device.HasAttribute("charge_status");
    
    // Check for any RGB effect attributes across all device types
    public bool SupportsRGB => Device.HasAttribute("matrix_effect_static") || 
                               Device.HasAttribute("matrix_effect_breath") ||
                               Device.HasAttribute("matrix_effect_spectrum") ||
                               Device.HasAttribute("matrix_effect_none") ||
                               Device.HasAttribute("logo_matrix_effect_static") || 
                               Device.HasAttribute("logo_matrix_effect_breath") ||
                               Device.HasAttribute("logo_matrix_effect_spectrum") ||
                               Device.HasAttribute("logo_matrix_effect_none") ||
                               Device.HasAttribute("scroll_matrix_effect_static") ||
                               Device.HasAttribute("scroll_matrix_effect_breath") ||
                               Device.HasAttribute("scroll_matrix_effect_spectrum") ||
                               Device.HasAttribute("scroll_matrix_effect_none") ||
                               Device.HasAttribute("backlight_led_state");
    
    // Check for any brightness control attributes across all device types
    public bool SupportsBrightness => Device.HasAttribute("matrix_brightness") || 
                                      Device.HasAttribute("logo_led_brightness") ||
                                      Device.HasAttribute("scroll_led_brightness") ||
                                      Device.HasAttribute("backlight_led_brightness") ||
                                      Device.HasAttribute("left_led_brightness") ||
                                      Device.HasAttribute("right_led_brightness") ||
                                      Device.HasAttribute("set_brightness");

    public DeviceModel(RazerDevice device)
    {
        Device = device;
    }
}
