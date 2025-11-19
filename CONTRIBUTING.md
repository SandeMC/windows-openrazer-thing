# Contributing to Razer Controller

Thank you for your interest in contributing to Razer Controller! This document provides guidelines and instructions for contributing.

## Getting Started

### Prerequisites

- Windows 10/11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2019 or later](https://visualstudio.microsoft.com/) (for native DLL development)
- Git

### Setting Up the Development Environment

1. **Clone the repository**
   ```bash
   git clone https://github.com/SandeMC/windows-openrazer-thing.git
   cd windows-openrazer-thing
   ```

2. **Build the native DLL**
   - Open `native/OpenRazer.sln` in Visual Studio
   - Select Release x64 configuration
   - Build the solution

3. **Build the .NET application**
   ```bash
   dotnet build RazerController.sln
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/RazerController
   ```

## Project Structure

```
windows-openrazer-thing/
├── .github/workflows/    # CI/CD workflows
├── native/              # Native C++ OpenRazer DLL
├── src/
│   ├── RazerController/        # Main Avalonia UI application
│   │   ├── Views/             # XAML UI views
│   │   ├── ViewModels/        # MVVM view models
│   │   ├── Models/            # Data models
│   │   └── Services/          # Application services
│   └── RazerController.Native/ # P/Invoke wrapper
└── RazerController.sln  # Solution file
```

## Development Guidelines

### Code Style

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

### MVVM Pattern

This application follows the MVVM (Model-View-ViewModel) pattern:
- **Models**: Data structures and business logic
- **Views**: XAML UI definitions
- **ViewModels**: UI logic and state management

### Adding New Features

1. **Device Support**
   - Add device attributes in `RazerDevice.cs`
   - Update `DeviceModel.cs` if new capabilities are needed
   - Add UI controls in `MainWindow.axaml`
   - Implement commands in `MainWindowViewModel.cs`

2. **UI Changes**
   - Keep UI responsive and intuitive
   - Test on different screen sizes
   - Maintain consistent styling

3. **Native Interop**
   - Add P/Invoke declarations in `OpenRazerNative.cs`
   - Wrap native calls in managed methods in `RazerDevice.cs`
   - Handle errors gracefully

### Testing

Before submitting a pull request:

1. **Build the solution**
   ```bash
   dotnet build RazerController.sln -c Release
   ```

2. **Test manually**
   - Run the application
   - Test with your Razer device(s)
   - Verify all features work as expected

3. **Check for regressions**
   - Ensure existing features still work
   - Test edge cases

## Submitting Changes

### Pull Request Process

1. **Fork the repository**

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   - Write clean, documented code
   - Follow the existing code style
   - Test thoroughly

4. **Commit your changes**
   ```bash
   git add .
   git commit -m "Add: Brief description of your changes"
   ```
   
   Use conventional commit messages:
   - `Add:` for new features
   - `Fix:` for bug fixes
   - `Update:` for improvements
   - `Docs:` for documentation changes

5. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request**
   - Go to the original repository
   - Click "New Pull Request"
   - Select your branch
   - Fill in the PR template
   - Submit

### Pull Request Guidelines

- Provide a clear description of the changes
- Reference any related issues
- Include screenshots for UI changes
- Ensure the build passes
- Be responsive to feedback

## Reporting Issues

### Bug Reports

When reporting bugs, include:
- OS version (Windows 10/11)
- Application version
- Device model
- Steps to reproduce
- Expected vs actual behavior
- Screenshots if applicable

### Feature Requests

When requesting features:
- Describe the feature clearly
- Explain the use case
- Suggest implementation if possible

## Code of Conduct

- Be respectful and constructive
- Welcome newcomers
- Focus on what is best for the community
- Show empathy towards other contributors

## Questions?

If you have questions:
- Open an issue with the "question" label
- Check existing issues for similar questions
- Reach out to maintainers

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

Thank you for contributing to Razer Controller!
