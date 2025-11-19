# Quick Start Guide

> **⚠️ AI-Generated POC**: This application is 100% vibecoded with AI. May not work correctly.

Get up and running with Windows OpenRazer Thing in minutes!

## For End Users

### Download and Run

1. **Download** the latest release from [Releases](../../releases)
2. **Extract** the ZIP file to a folder
3. **Run** `WindowsOpenrazerThing.exe` (or `RazerController.exe`)
4. **Wait** for automatic device detection (happens on startup)
5. **Select** your device from the list and start controlling it!

### First Time Setup

When you first run the application:

1. **Plug in your Razer device** (keyboard, mouse, headset, etc.)
2. **Launch the application**
3. **Devices Auto-Detect** - the app automatically finds all connected Razer devices on startup
4. **First device is auto-selected** - just start using it!
5. Or **select a different device** from the list on the left

## For Developers

### Quick Build

```bash
# Clone the repository
git clone https://github.com/SandeMC/windows-openrazer-thing.git
cd windows-openrazer-thing

# Build (requires Visual Studio for native DLL)
# Option 1: PowerShell script
.\build.ps1

# Option 2: Manual build
# Step 1: Build native DLL in Visual Studio
# Open native/OpenRazer.sln and build Release x64

# Step 2: Build .NET app
dotnet build RazerController.sln -c Release

# Run
dotnet run --project src/RazerController -c Release
```

### Development Mode

```bash
# Install dependencies
dotnet restore

# Build
dotnet build

# Run in debug mode
dotnet run --project src/RazerController
```

## Common Tasks

### Changing RGB Colors

1. Select your device (or use the auto-selected one)
2. Use the **RGB sliders** to pick a color (see live preview)
3. Click **"Set Static Color"**
4. RGB automatically enables if it was off

### Setting Up Your Mouse

1. Select your mouse device
2. Adjust **DPI** using:
   - The slider for quick changes
   - Or type exact value in the number box
   - Standard anchors: 400, 800, 1600, 3200, 6400
3. Set **Polling Rate** using:
   - The slider (shows only supported rates)
   - Or dropdown menu
4. Click "Set" buttons to apply
5. Values update automatically every second

### Using Effects

- **Static Color**: Solid color of your choice
- **Spectrum Effect**: Rainbow cycling
- **Breath Effect**: Pulsing with your chosen color
- **Turn Off**: Disable all lighting

### System Tray

- **Close Window**: Minimizes to system tray (doesn't quit)
- **Click Tray Icon**: Restore window
- **Right-click Tray Icon**: Open menu
  - Show: Restore window
  - Exit: Quit application completely
- Device values poll once when menu opens (silent)

## Troubleshooting

### Application Won't Start

- Ensure .NET 9 runtime is installed (included in self-contained builds)
- Run as Administrator if needed
- Check Windows Event Viewer for errors

### No Devices Found

- Ensure device is plugged in
- Try a different USB port
- Click "Refresh" after reconnecting
- Restart the application

### Changes Don't Apply

- Some devices take a moment to update
- Try the command again
- Check if the feature is supported by your device

### DLL Not Found Error

If building from source:
- Make sure you built the native DLL first
- Copy `OpenRazer64.dll` to the application folder
- Use the build scripts which handle this automatically

## Next Steps

- Read the full [README](README.md) for detailed documentation
- Check out [CONTRIBUTING](CONTRIBUTING.md) if you want to contribute
- Report issues on [GitHub Issues](../../issues)
- Star the repo if you find it useful!

## FAQ

**Q: Do I need Razer Synapse?**  
A: No! This application works independently of Synapse.

**Q: Will this interfere with Synapse?**  
A: It's recommended to close Synapse when using this application.

**Q: What devices are supported?**  
A: All devices supported by OpenRazer. See the [device list](https://openrazer.github.io/#devices). Note: Driver code cutoff is May 23, 2025.

**Q: Is this safe for my device?**  
A: Yes, it uses the same protocols as the official Razer software. However, this is an AI-generated POC, so use at your own risk.

**Q: Can I use this on Linux?**  
A: This is a Windows-specific port. For Linux, use [OpenRazer](https://openrazer.github.io/) directly.

**Q: Why do I need Visual Studio to build?**  
A: Only if building from source. The native DLL requires C++ compilation. Pre-built releases don't require Visual Studio.

**Q: How do I enable debug logging?**  
A: Create an empty file named `DEBUG` in the application folder and restart the app.

## Support

Need help? 
- Check the [README](README.md)
- Browse [existing issues](../../issues)
- Create a [new issue](../../issues/new)

Enjoy controlling your Razer devices! 🎮✨
