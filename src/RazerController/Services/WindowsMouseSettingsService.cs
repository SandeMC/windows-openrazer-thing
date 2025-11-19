using System;
using System.Runtime.InteropServices;
using NLog;

namespace RazerController.Services;

/// <summary>
/// Service for interacting with Windows mouse settings (sensitivity and acceleration)
/// </summary>
public class WindowsMouseSettingsService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    // Windows API constants
    private const int SPI_GETMOUSE = 0x0003;
    private const int SPI_SETMOUSE = 0x0004;
    private const int SPI_GETMOUSESPEED = 0x0070;
    private const int SPI_SETMOUSESPEED = 0x0071;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(
        int uiAction,
        int uiParam,
        IntPtr pvParam,
        int fWinIni);
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(
        int uiAction,
        int uiParam,
        int[] pvParam,
        int fWinIni);
    
    /// <summary>
    /// Gets the current mouse sensitivity (pointer speed) from Windows settings
    /// </summary>
    /// <returns>Sensitivity value between 1 and 20, or null if failed</returns>
    public int? GetMouseSensitivity()
    {
        try
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                bool success = SystemParametersInfo(SPI_GETMOUSESPEED, 0, ptr, 0);
                if (success)
                {
                    int speed = Marshal.ReadInt32(ptr);
                    Logger.Debug($"Current Windows mouse speed: {speed}");
                    return speed;
                }
                else
                {
                    Logger.Warn($"Failed to get mouse speed. Error: {Marshal.GetLastWin32Error()}");
                    return null;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error getting mouse sensitivity");
            return null;
        }
    }
    
    /// <summary>
    /// Sets the mouse sensitivity (pointer speed) in Windows settings
    /// </summary>
    /// <param name="sensitivity">Value between 1 and 20</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SetMouseSensitivity(int sensitivity)
    {
        try
        {
            if (sensitivity < 1 || sensitivity > 20)
            {
                Logger.Warn($"Invalid sensitivity value: {sensitivity}. Must be between 1 and 20.");
                return false;
            }
            
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                Marshal.WriteInt32(ptr, sensitivity);
                bool success = SystemParametersInfo(
                    SPI_SETMOUSESPEED, 
                    0, 
                    ptr, 
                    SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                
                if (success)
                {
                    Logger.Info($"Set Windows mouse speed to: {sensitivity}");
                    return true;
                }
                else
                {
                    Logger.Warn($"Failed to set mouse speed. Error: {Marshal.GetLastWin32Error()}");
                    return false;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error setting mouse sensitivity to {sensitivity}");
            return false;
        }
    }
    
    /// <summary>
    /// Gets the current mouse acceleration setting from Windows
    /// </summary>
    /// <returns>True if acceleration is enabled, false if disabled, null if failed</returns>
    public bool? GetMouseAcceleration()
    {
        try
        {
            int[] mouseParams = new int[3];
            bool success = SystemParametersInfo(SPI_GETMOUSE, 0, mouseParams, 0);
            
            if (success)
            {
                // mouseParams[2] determines if acceleration is enabled
                // 0 = disabled, 1 = enabled
                bool accelerationEnabled = mouseParams[2] != 0;
                Logger.Debug($"Mouse acceleration: {(accelerationEnabled ? "enabled" : "disabled")} (params: {mouseParams[0]}, {mouseParams[1]}, {mouseParams[2]})");
                return accelerationEnabled;
            }
            else
            {
                Logger.Warn($"Failed to get mouse acceleration. Error: {Marshal.GetLastWin32Error()}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error getting mouse acceleration");
            return null;
        }
    }
    
    /// <summary>
    /// Sets the mouse acceleration in Windows settings
    /// </summary>
    /// <param name="enabled">True to enable acceleration, false to disable</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SetMouseAcceleration(bool enabled)
    {
        try
        {
            // First get the current settings
            int[] mouseParams = new int[3];
            bool success = SystemParametersInfo(SPI_GETMOUSE, 0, mouseParams, 0);
            
            if (!success)
            {
                Logger.Warn("Failed to get current mouse parameters");
                return false;
            }
            
            // Update the acceleration setting
            // mouseParams[2] determines if acceleration is enabled
            // When enabled: typically [6, 10, 1] or [threshold1, threshold2, 1]
            // When disabled: [0, 0, 0]
            if (enabled)
            {
                // Enable acceleration with default thresholds
                mouseParams[0] = 6;  // First threshold
                mouseParams[1] = 10; // Second threshold
                mouseParams[2] = 1;  // Enable acceleration
            }
            else
            {
                // Disable acceleration
                mouseParams[0] = 0;
                mouseParams[1] = 0;
                mouseParams[2] = 0;
            }
            
            success = SystemParametersInfo(
                SPI_SETMOUSE, 
                0, 
                mouseParams, 
                SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            
            if (success)
            {
                Logger.Info($"Mouse acceleration {(enabled ? "enabled" : "disabled")}");
                return true;
            }
            else
            {
                Logger.Warn($"Failed to set mouse acceleration. Error: {Marshal.GetLastWin32Error()}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error setting mouse acceleration to {enabled}");
            return false;
        }
    }
}
