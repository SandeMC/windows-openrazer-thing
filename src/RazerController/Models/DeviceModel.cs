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
    public bool SupportsPollRate => Device.DeviceType == RazerDeviceType.Mouse && Device.HasAttribute("poll_rate");
    public bool SupportsRGB => Device.HasAttribute("matrix_effect_static");
    public bool SupportsBrightness => Device.HasAttribute("matrix_brightness");

    public DeviceModel(RazerDevice device)
    {
        Device = device;
    }
}
