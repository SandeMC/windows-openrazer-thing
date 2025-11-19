# Windows OpenRazer Thing

## DISCLAIMER

**This is a 100% AI-generated proof of concept that may not work correctly.** All development and maintenance is done exclusively with AI assistance. This project demonstrates implementing the [openrazer-win32](https://github.com/tpoechtrager/openrazer-win32) API in C#.

**Important Notes:**
- Device driver code and database cutoff: May 23, 2025
- Only tested with: Razer DeathAdder V3 and Razer DeathAdder Essential 2021
- Goal: Replace Razer Synapse with an open-source implementation
- For RGB functionality, use Windows 11 built-in RGB settings or [OpenRGB](https://openrgb.org)

## About

A Windows application for controlling Razer devices without Synapse, using the open-source OpenRazer Win32 driver port. Built with C# .NET 9 and Avalonia UI.

## Features

- Device Support: Keyboards, Mice, Accessories, and Headsets
- Mouse Settings: DPI and polling rate configuration (supports up to 8000Hz on compatible devices)
- Basic Lighting Control: Static colors, spectrum effects, breathing effects
- Brightness Control: Adjust lighting brightness
- Battery Status: Display battery level and charging status for wireless devices
- System Tray: Minimize to tray for background operation
- Dynamic Updates: Automatically refreshes device values every few seconds
- Plug and Play: Automatic device detection

## Screenshots

Screenshots will be added after the application is built and running.

## Requirements

- Windows 10/11 (64-bit)
- .NET 9.0 Runtime (included in self-contained builds)
- OpenRazer Win32 DLL (included)

## Installation

### Option 1: Pre-built Release (Recommended)

1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file
3. Run `RazerController.exe`

### Option 2: Build from Source

#### Prerequisites

- Windows 10/11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2019 or later (for building the native DLL)

#### Building the Native DLL

1. Open `native/OpenRazer.sln` in Visual Studio
2. Select your target platform (x64 or x86)
3. Build the solution (Release configuration)
4. The DLL will be output to `native/OpenRazer64.dll` (for x64) or `native/OpenRazer.dll` (for x86)

#### Building the Application

```bash
# Clone the repository
git clone https://github.com/SandeMC/windows-openrazer-thing.git
cd windows-openrazer-thing

# Check if you have all prerequisites
.\check-prerequisites.ps1

# Build the native DLL (requires Visual Studio)
# Open native/OpenRazer.sln and build, or use MSBuild:
msbuild native/OpenRazer.sln /p:Configuration=Release /p:Platform=x64

# Build the .NET application
dotnet build RazerController.sln -c Release

# Run the application
dotnet run --project src/RazerController -c Release
```

#### Creating a Self-Contained Executable

```bash
# Windows x64
dotnet publish src/RazerController/RazerController.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# The executable will be in: src/RazerController/bin/Release/net9.0/win-x64/publish/
```

## Quick Start

1. Launch the Application: Run `RazerController.exe`
2. Initialize Devices: Click the "Initialize Devices" button
3. Select a Device: Click on a device in the left panel
4. Control Your Device:
   - Use the RGB sliders or textboxes to choose a color
   - Click "Set Static Color" to apply it
   - Try other effects like Spectrum or Breath
   - For mice, adjust DPI and polling rate

## Usage

### RGB Lighting

Note: For advanced RGB control, use Windows 11 built-in RGB settings or [OpenRGB](https://openrgb.org). This application provides basic lighting control only.

- Static Color: Choose RGB values (sliders or textboxes) and click "Set Static Color"
- Spectrum Effect: Rainbow cycling effect
- Breath Effect: Pulsing effect with your chosen color
- Turn Off: Disable all lighting
- Brightness: Adjust the overall brightness (0-255)

### Mouse Settings

- DPI: Set your preferred sensitivity (100-20000)
- Polling Rate: Dynamically detected from device (supports 125Hz to 8000Hz)
- Values are automatically loaded from device on selection
- Device values refresh every 3 seconds while selected

### Battery Status (Wireless Devices)

- Displays battery percentage
- Shows charging status
- Updates automatically

### System Tray

- The application minimizes to the system tray when closed
- Right-click the tray icon for options:
  - Show: Restore the main window
  - Exit: Quit the application

## Supported Devices

This application supports all devices compatible with OpenRazer, including:

- Razer Keyboards (BlackWidow, Huntsman, etc.)
- Razer Mice (DeathAdder, Viper, Mamba, etc.)
- Razer Accessories (Mouse mats, etc.)
- Razer Headsets (Kraken series)

For a complete list, see the [OpenRazer device list](https://openrazer.github.io/#devices).

## Troubleshooting

### "Failed to initialize" or "DLL not found" Error

**Check Required DLL:**
The application requires the OpenRazer DLL in the same folder as the executable:
- `OpenRazer64.dll` (or `OpenRazer.dll` for x86)

Note: The hidapi library is now statically linked, so no separate hidapi.dll is required.

**If DLL is missing:**
1. Run the build script: `.\build.ps1` (automatically copies the DLL)
2. Or manually copy from:
   - `native/OpenRazer64.dll` → application folder

**Other steps:**
- Ensure your Razer device is properly connected
- Try running as Administrator
- Check the `logs/` folder for detailed error messages

### Device Not Detected

- Unplug and reconnect your device
- Click the "Refresh" button
- Restart the application

### Changes Not Applied

- Some devices may take a moment to respond
- Try the command again
- Check if your device supports the feature you're trying to use

## Project Structure

```
windows-openrazer-thing/
├── native/                          # OpenRazer Win32 native DLL
│   ├── OpenRazer.sln               # Visual Studio solution
│   └── ...                         # Native C++ code
├── src/
│   ├── RazerController/            # Main Avalonia UI application
│   │   ├── Views/                  # XAML views
│   │   ├── ViewModels/             # MVVM view models
│   │   ├── Models/                 # Data models
│   │   └── Services/               # Application services
│   └── RazerController.Native/     # P/Invoke wrapper library
│       ├── OpenRazerNative.cs      # Native interop
│       ├── RazerDevice.cs          # Device abstraction
│       └── RazerDeviceManager.cs   # Device management
└── RazerController.sln             # Main solution file
```

## Technology Stack

- **UI Framework**: Avalonia UI 11.3.8 (cross-platform XAML-based UI)
- **Language**: C# 12 with .NET 9
- **Architecture**: MVVM (Model-View-ViewModel)
- **Native Interop**: P/Invoke for OpenRazer Win32 DLL
- **Dependencies**: CommunityToolkit.Mvvm

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project uses the OpenRazer Win32 driver, which is based on the OpenRazer Linux driver. Please refer to the original projects for licensing information:

- [OpenRazer Win32](https://github.com/tpoechtrager/openrazer-win32)
- [OpenRazer](https://github.com/openrazer/openrazer)

## Credits

- **OpenRazer Win32**: tpoechtrager (based on CalcProgrammer1's original work)
- **OpenRazer**: The OpenRazer Team
- **UI Framework**: Avalonia Team
- **Application**: SandeMC and contributors

## Acknowledgments

This project would not be possible without:
- The OpenRazer project for reverse-engineering Razer protocols
- CalcProgrammer1's original Windows port of OpenRazer
- tpoechtrager's maintained fork of openrazer-win32
- The Avalonia UI framework for cross-platform UI capabilities
