using System;
using System.Runtime.InteropServices;
using NLog;
using PInvoke;

namespace RazerController.Services;

/// <summary>
/// Service for interacting with Windows mouse settings (sensitivity and acceleration)
/// </summary>
public class WindowsMouseSettingsService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Gets the current mouse sensitivity (pointer speed) from Windows settings
    /// </summary>
    /// <returns>Sensitivity value between 1 and 20, or null if failed</returns>
    public int? GetMouseSensitivity()
    {
        try
        {
            int speed = 0;
            unsafe
            {
                bool success = User32.SystemParametersInfo(
                    User32.SystemParametersInfoAction.SPI_GETMOUSESPEED,
                    0,
                    &speed,
                    User32.SystemParametersInfoFlags.None);
                    
                if (success)
                {
                    Logger.Debug($"Current Windows mouse speed: {speed}");
                    return speed;
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Logger.Warn($"Failed to get mouse speed. Error: {error}");
                    return null;
                }
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
            
            unsafe
            {
                // For SPI_SETMOUSESPEED, pass the sensitivity value as a pointer in pvParam
                bool success = User32.SystemParametersInfo(
                    User32.SystemParametersInfoAction.SPI_SETMOUSESPEED,
                    0,
                    sensitivity,
                    0);
                
                int lastError = Marshal.GetLastWin32Error();
                
                if (success)
                {
                    Logger.Info($"Set Windows mouse speed to: {sensitivity}");
                    // Verify the setting was applied
                    var verifySpeed = GetMouseSensitivity();
                    if (verifySpeed.HasValue && verifySpeed.Value != sensitivity)
                    {
                        Logger.Warn($"Verification failed: requested {sensitivity}, got {verifySpeed.Value}");
                    }
                    return true;
                }
                else
                {
                    Logger.Warn($"Failed to set mouse speed. Error code: {lastError} (0x{lastError:X})");
                    return false;
                }
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
            unsafe
            {
                fixed (int* ptr = mouseParams)
                {
                    bool success = User32.SystemParametersInfo(
                        User32.SystemParametersInfoAction.SPI_GETMOUSE,
                        0,
                        ptr,
                        User32.SystemParametersInfoFlags.None);
                    
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
                        int error = Marshal.GetLastWin32Error();
                        Logger.Warn($"Failed to get mouse acceleration. Error: {error}");
                        return null;
                    }
                }
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
            bool success;
            
            unsafe
            {
                fixed (int* ptr = mouseParams)
                {
                    success = User32.SystemParametersInfo(
                        User32.SystemParametersInfoAction.SPI_GETMOUSE,
                        0,
                        ptr,
                        User32.SystemParametersInfoFlags.None);
                }
            }
            
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
            
            unsafe
            {
                fixed (int* ptr = mouseParams)
                {
                    success = User32.SystemParametersInfo(
                        User32.SystemParametersInfoAction.SPI_SETMOUSE,
                        0,
                        ptr,
                        User32.SystemParametersInfoFlags.SPIF_UPDATEINIFILE | User32.SystemParametersInfoFlags.SPIF_SENDCHANGE);
                }
            }
            
            int lastError = Marshal.GetLastWin32Error();
            
            if (success)
            {
                Logger.Info($"Mouse acceleration {(enabled ? "enabled" : "disabled")}");
                return true;
            }
            else
            {
                Logger.Warn($"Failed to set mouse acceleration. Error code: {lastError} (0x{lastError:X})");
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
