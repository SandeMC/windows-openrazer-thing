# Razer Controller Screenshots

## Main Application Window

```
┌─────────────────────────────────────────────────────────────────────┐
│ Razer Controller                                          ─  □  ×    │
├─────────────────────────────────────────────────────────────────────┤
│ [Initialize Devices]  [Refresh]                                     │
├──────────────────┬──────────────────────────────────────────────────┤
│ Devices          │ Device Information                               │
│                  │ ┌────────────────────────────────────────────┐   │
│ ┌──────────────┐ │ │ Name:         Razer DeathAdder Elite       │   │
│ │ ▶ Razer      │ │ │ Type:         Mouse                        │   │
│ │   DeathAdder │ │ │ Serial:       PM1234567890                 │   │
│ │   Elite      │ │ │ Firmware:     v2.5                         │   │
│ │              │ │ └────────────────────────────────────────────┘   │
│ │ ◯ Razer      │ │                                                  │
│ │   BlackWidow │ │ RGB Lighting                                     │
│ │   Chroma     │ │ ┌────────────────────────────────────────────┐   │
│ │              │ │ │ Color (RGB):                               │   │
│ └──────────────┘ │ │   Red:   [■■■■■■■■■■░░░░] 255              │   │
│                  │ │   Green: [■■■■■■■■■■░░░░] 255              │   │
│                  │ │   Blue:  [■■■■■■■■■■░░░░] 255              │   │
│                  │ │                                             │   │
│                  │ │   Preview: [████████] (White)              │   │
│                  │ │                                             │   │
│                  │ │   [Set Static Color] [Spectrum Effect]     │   │
│                  │ │   [Breath Effect]    [Turn Off]            │   │
│                  │ │                                             │   │
│                  │ │   Brightness: [■■■■■■■■■■■■■░] 255 [Apply] │   │
│                  │ └────────────────────────────────────────────┘   │
│                  │                                                  │
│                  │ Mouse Settings                                   │
│                  │ ┌────────────────────────────────────────────┐   │
│                  │ │ DPI:          [800    ▼]  [Set DPI]        │   │
│                  │ │                                             │   │
│                  │ │ Polling Rate: [1000 Hz ▼]  [Set Poll Rate] │   │
│                  │ └────────────────────────────────────────────┘   │
├──────────────────┴──────────────────────────────────────────────────┤
│ Status: Set color to RGB(255, 255, 255)                             │
└─────────────────────────────────────────────────────────────────────┘
```

## System Tray Integration

The application minimizes to the system tray when closed:

```
System Tray:
┌─────────────────┐
│ [🎮]            │  ← Razer Controller Icon
│                 │
│  Right-click:   │
│  ┌────────────┐ │
│  │ Show       │ │  ← Restores main window
│  │ ──────────-│ │
│  │ Exit       │ │  ← Quits application
│  └────────────┘ │
└─────────────────┘
```

## Features Showcase

### RGB Lighting
- **Static Color**: Choose any RGB color with sliders
- **Live Preview**: See your color before applying
- **Quick Effects**: One-click spectrum, breath, or off
- **Brightness Control**: Adjust overall brightness 0-255

### Mouse Configuration
- **DPI Settings**: Set from 100 to 20,000 in 100 DPI increments
- **Polling Rate**: Choose from 125Hz, 250Hz, 500Hz, or 1000Hz
- **Easy Application**: Click to apply settings instantly

### Device Management
- **Auto-Detection**: Automatically finds all Razer devices
- **Device List**: Clean list showing all connected devices
- **Device Info**: View model, serial number, and firmware
- **Multiple Devices**: Switch between devices easily

## Color Examples

The application supports full RGB color control:
- Pure colors (Red, Green, Blue)
- Mixed colors (Purple, Yellow, Cyan)
- White and grayscale
- Any RGB value from 0-255 per channel

## Notes

*Actual screenshots will be added once the application is built and tested with real devices.*

To run and capture actual screenshots:
1. Build the application following the README
2. Connect a Razer device
3. Launch the application
4. Use Windows Snipping Tool or similar to capture screens

The UI is built with Avalonia UI, providing a modern, clean interface that's easy to use.
