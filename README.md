# Windows OpenRazer Thing

> [!CAUTION]
> **THIS APPLICATION IS AN AI-MADE PROOF OF CONCEPT**
> 
> This is a proof-of-concept application that has been **entirely vibecoded** (excluding existing libraries - such as [openrazer-win32](https://github.com/tpoechtrager/openrazer-win32) - which are a base for this application. It may not work correctly, and **will likely not be maintained**. It does support some features, but use at your own risk.
>
> **Key Limitations:**
> - Device driver code and database cutoff: **May 23, 2025**
> - Only tested with: **Razer DeathAdder V3** and **Razer DeathAdder Essential 2021**
> - Only designed for mice. Most features untested due to lack of hardware ownership.
> - RGB support very rudimentary. Use [OpenRGB](https://openrgb.org) instead.

## About

A Windows application for controlling Razer devices without Synapse, using the open-source OpenRazer Win32 driver port. Built with C# .NET 9 and Avalonia UI.

## Features

### Device Support
- Keyboards [untested]
- Mice  
- Accessories (mousepads, etc.) [untested]
- Headsets [untested]
- Automatic device detection on startup
- Plug and play - just works without any extra drivers.

### Mouse Settings
- DPI configuration with a slider (100-35000 DPI)
  - Allows selecting any intermediate value
- Polling rate configuration (125Hz - 8000Hz on compatible devices)
  - Slider with device-supported rates as anchor points
  - Dynamically detects supported polling rates
- Quick access to Windows sensitivity and mouse acceleration settings

### RGB Lighting Control
- Static colors with RGB sliders and color preview
- Spectrum effect (rainbow cycling)
- Breathing effect with custom color
- Turn off RGB completely
- Brightness control (0-255)
- Only shows effects supported by current device

### Power Management
- Battery level display for wireless devices [untested]
- Charging status indicator [untested]

### System Integration
- System tray icon for quick access to the app

## Screenshots
<img width="1920" height="1027" alt="image" src="https://github.com/user-attachments/assets/936643fe-0daf-4e1b-9a07-d3237129d5e8" />
<img width="1920" height="1027" alt="image" src="https://github.com/user-attachments/assets/c5bb3ae2-8c67-47ef-9721-b17d6cab594a" />

## Requirements

- Windows 10/11 (64-bit)
- Razer peripherals

## Installation

### Option 1: Pre-built Release (Recommended)

1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file of the desired version
3. Run `WindowsOpenrazerThing.exe`

### Option 2: Build from Source

#### Prerequisites

- Windows 10/11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2019 or later (for building the native DLL)

#### Building the OpenRazer DLL

1. Open `native/OpenRazer.sln` in Visual Studio
2. Select your target platform (x64 or x86)
3. Build the solution (Release configuration)
4. The DLL will be output to `native/OpenRazer64.dll` (for x64) or `native/OpenRazer.dll` (for x86)

#### Building the Application

```bash
# Clone the repository
git clone https://github.com/SandeMC/windows-openrazer-thing.git --recursive
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

1. Launch the Application: Run `WindowsOpenrazerThing.exe`
2. Devices Auto-Initialize: The app automatically detects your Razer devices on startup
3. Select a Device: Click on a device in the left panel (first device is auto-selected)
4. Control Your Device:
   - **Mouse Settings** appear at the top (for mice)
     - Adjust DPI with the slider or number input
     - Set polling rate with the slider
   - **RGB Lighting** controls appear below
     - Use the RGB sliders to choose a color
     - Click effect buttons to apply (only supported effects are shown)
     - Adjust brightness as needed
   - Changes are applied immediately when you click "Set" buttons


## Project Structure

```
windows-openrazer-thing/
├── native/                          # OpenRazer Win32 native code
│   ├── OpenRazer.sln               # Visual Studio solution for building DLL
│   ├── openrazer/                  # OpenRazer driver code (from openrazer-win32)
│   └── ...                         # Supporting files and dependencies
├── src/
│   ├── RazerController/            # Main Avalonia UI application
│   │   ├── Views/                  # XAML UI views
│   │   ├── ViewModels/             # MVVM view models
│   │   ├── Models/                 # Data models
│   │   ├── Services/               # Application services (tray, etc.)
│   │   └── NLog.config             # Logging configuration
│   └── RazerController.Native/     # P/Invoke wrapper library
│       ├── OpenRazerNative.cs      # Native function declarations
│       ├── RazerDevice.cs          # Device abstraction layer
│       └── RazerDeviceManager.cs   # Device discovery and management
├── .github/workflows/              # CI/CD automation
└── RazerController.sln             # Main solution file
```

## Technology Stack

- **UI Framework**: Avalonia UI 11.3.8 (cross-platform XAML-based UI)
- **Language**: C# 12 with .NET 9
- **Architecture**: MVVM (Model-View-ViewModel)
- **Native Interop**: P/Invoke for OpenRazer Win32 DLL
- **Dependencies**: 
  - CommunityToolkit.Mvvm (MVVM helpers)
  - NLog (logging framework)
  - System.Drawing.Common (color handling)
- **Driver**: Based on [openrazer-win32](https://github.com/SandeMC/openrazer-win32)

## License

This project uses the OpenRazer Win32 driver, which is based on the OpenRazer Linux driver. Please refer to the original projects for licensing information:

- [OpenRazer Win32](https://github.com/tpoechtrager/openrazer-win32)
- [OpenRazer](https://github.com/openrazer/openrazer)

## Credits

- **OpenRazer Win32**: [tpoechtrager](https://github.com/tpoechtrager/openrazer-win32) (based on CalcProgrammer1's original work)
- **OpenRazer**: The [OpenRazer Team](https://github.com/openrazer/openrazer)
- **UI Framework**: [Avalonia Team](https://github.com/AvaloniaUI/Avalonia)
- **Polychromatic**: [Polychromatic Team](https://github.com/polychromatic/polychromatic) (some of their implementations have been used for reference)

## Acknowledgments

This project would not be possible without:
- The OpenRazer project for reverse-engineering Razer protocols
- CalcProgrammer1's original Windows port of OpenRazer
- tpoechtrager's more up-to-date fork of openrazer-win32
- The Avalonia UI framework for cross-platform UI capabilities
