@echo off
REM Simple build script for Razer Controller

echo ===================================
echo Building Razer Controller
echo ===================================
echo.

REM Build .NET Application
echo Building .NET Application...
dotnet build RazerController.sln -c Release

if errorlevel 1 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo ===================================
echo Build completed successfully!
echo ===================================
echo.
echo To run the application:
echo   dotnet run --project src\RazerController -c Release
echo.
echo Note: You need to build the native DLL separately in Visual Studio
echo       Open native\OpenRazer.sln and build in Release x64
echo.
pause
