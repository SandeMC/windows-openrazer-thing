# Build script for Razer Controller
# This script builds both the native DLL and the .NET application

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("x64", "x86")]
    [string]$Platform = "x64",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipNative,
    
    [Parameter(Mandatory=$false)]
    [switch]$Publish
)

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Razer Controller Build Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptPath

try {
    # Build Native DLL
    if (-not $SkipNative) {
        Write-Host "Building Native OpenRazer DLL..." -ForegroundColor Yellow
        Write-Host "Configuration: $Configuration, Platform: $Platform" -ForegroundColor Gray
        
        $msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
            -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
            -prerelease | Select-Object -First 1
        
        if (-not $msbuildPath) {
            Write-Host "WARNING: MSBuild not found. Skipping native DLL build." -ForegroundColor Yellow
            Write-Host "Please build native/OpenRazer.sln manually in Visual Studio." -ForegroundColor Yellow
        } else {
            & $msbuildPath "native\OpenRazer.sln" /p:Configuration=$Configuration /p:Platform=$Platform /m
            if ($LASTEXITCODE -ne 0) {
                throw "Native DLL build failed"
            }
            Write-Host "Native DLL built successfully!" -ForegroundColor Green
        }
        Write-Host ""
    }

    # Build .NET Application
    Write-Host "Building .NET Application..." -ForegroundColor Yellow
    dotnet build RazerController.sln -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw ".NET application build failed"
    }
    Write-Host ".NET Application built successfully!" -ForegroundColor Green
    Write-Host ""

    # Copy Native DLLs to output
    $dllName = if ($Platform -eq "x64") { "OpenRazer64.dll" } else { "OpenRazer.dll" }
    $dllSource = "native\$dllName"
    $outputDir = "src\RazerController\bin\$Configuration\net9.0"
    $dllDest = "$outputDir\$dllName"
    
    if (Test-Path $dllSource) {
        Write-Host "Copying $dllName to application output..." -ForegroundColor Yellow
        Copy-Item $dllSource $dllDest -Force
        Write-Host "OpenRazer DLL copied successfully!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Native DLL not found at $dllSource" -ForegroundColor Yellow
        Write-Host "Checking for DLL in alternate locations..."
        Get-ChildItem native -Recurse -Filter "*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "  Found: $($_.FullName)"
        }
        Write-Host "The application may not work without the native DLL." -ForegroundColor Yellow
    }
    
    # Copy hidapi.dll dependency
    $hidapiSource = "native\dependencies\hidapi-win\$Platform\hidapi.dll"
    $hidapiDest = "$outputDir\hidapi.dll"
    
    if (Test-Path $hidapiSource) {
        Write-Host "Copying hidapi.dll dependency..." -ForegroundColor Yellow
        Copy-Item $hidapiSource $hidapiDest -Force
        Write-Host "hidapi.dll copied successfully!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: hidapi.dll not found at $hidapiSource" -ForegroundColor Yellow
        Write-Host "The application may not work without this dependency." -ForegroundColor Yellow
    }
    Write-Host ""

    # Publish if requested
    if ($Publish) {
        Write-Host "Publishing self-contained application..." -ForegroundColor Yellow
        $runtime = "win-$($Platform.ToLower())"
        dotnet publish src\RazerController\RazerController.csproj `
            -c $Configuration `
            -r $runtime `
            --self-contained `
            -p:PublishSingleFile=false `
            -p:IncludeNativeLibrariesForSelfExtract=true
        
        if ($LASTEXITCODE -ne 0) {
            throw "Publish failed"
        }
        
        # Copy native DLLs to publish folder
        $publishDir = "src\RazerController\bin\$Configuration\net9.0\$runtime\publish"
        if (Test-Path $dllSource) {
            Copy-Item $dllSource "$publishDir\$dllName" -Force
        }
        if (Test-Path $hidapiSource) {
            Copy-Item $hidapiSource "$publishDir\hidapi.dll" -Force
        }
        
        Write-Host "Application published successfully!" -ForegroundColor Green
        Write-Host "Output: $publishDir" -ForegroundColor Cyan
    }
    
    Write-Host ""
    Write-Host "==================================" -ForegroundColor Green
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "==================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "To run the application:" -ForegroundColor Cyan
    Write-Host "  dotnet run --project src\RazerController -c $Configuration" -ForegroundColor White
    
} catch {
    Write-Host ""
    Write-Host "==================================" -ForegroundColor Red
    Write-Host "Build failed: $_" -ForegroundColor Red
    Write-Host "==================================" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}
