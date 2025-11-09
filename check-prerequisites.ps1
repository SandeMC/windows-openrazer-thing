# Prerequisites Checker for Razer Controller
# This script checks if all required tools are installed

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Razer Controller - Prerequisites Check" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -match "^9\.") {
        Write-Host "  ✓ .NET SDK $dotnetVersion found" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ .NET SDK $dotnetVersion found, but .NET 9 is recommended" -ForegroundColor Yellow
        Write-Host "    Download from: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ✗ .NET SDK not found" -ForegroundColor Red
    Write-Host "    Download from: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Gray
    $allGood = $false
}
Write-Host ""

# Check Git
Write-Host "Checking Git..." -ForegroundColor Yellow
try {
    $gitVersion = git --version
    Write-Host "  ✓ $gitVersion found" -ForegroundColor Green
} catch {
    Write-Host "  ⚠ Git not found (optional for users, required for developers)" -ForegroundColor Yellow
    Write-Host "    Download from: https://git-scm.com/download/win" -ForegroundColor Gray
}
Write-Host ""

# Check MSBuild (for native DLL compilation)
Write-Host "Checking MSBuild..." -ForegroundColor Yellow
try {
    $msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
        -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
        -prerelease | Select-Object -First 1
    
    if ($msbuildPath) {
        $msbuildVersion = & $msbuildPath /version /nologo | Select-Object -Last 1
        Write-Host "  ✓ MSBuild $msbuildVersion found" -ForegroundColor Green
        Write-Host "    Path: $msbuildPath" -ForegroundColor Gray
    } else {
        Write-Host "  ⚠ MSBuild not found" -ForegroundColor Yellow
        Write-Host "    This is required to build the native DLL" -ForegroundColor Gray
        Write-Host "    Install Visual Studio 2019 or later with C++ support" -ForegroundColor Gray
        Write-Host "    Download from: https://visualstudio.microsoft.com/" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ⚠ Visual Studio not found" -ForegroundColor Yellow
    Write-Host "    This is required to build the native DLL" -ForegroundColor Gray
    Write-Host "    Install Visual Studio 2019 or later with C++ support" -ForegroundColor Gray
    Write-Host "    Download from: https://visualstudio.microsoft.com/" -ForegroundColor Gray
}
Write-Host ""

# Check if native DLL exists
Write-Host "Checking Native DLL..." -ForegroundColor Yellow
$dllPath64 = "native\x64\Release\OpenRazer64.dll"
$dllPath32 = "native\Release\OpenRazer.dll"

if (Test-Path $dllPath64) {
    Write-Host "  ✓ Native DLL found (x64): $dllPath64" -ForegroundColor Green
} elseif (Test-Path $dllPath32) {
    Write-Host "  ✓ Native DLL found (x86): $dllPath32" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Native DLL not built yet" -ForegroundColor Yellow
    Write-Host "    Build native\OpenRazer.sln in Visual Studio" -ForegroundColor Gray
    Write-Host "    Or run: .\build.ps1" -ForegroundColor Gray
}
Write-Host ""

# Summary
Write-Host "==================================" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "All required prerequisites are met!" -ForegroundColor Green
    Write-Host "" 
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Build: .\build.ps1" -ForegroundColor White
    Write-Host "  2. Run: dotnet run --project src\RazerController" -ForegroundColor White
} else {
    Write-Host "Some prerequisites are missing!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "For end users:" -ForegroundColor Cyan
    Write-Host "  - Download pre-built releases (no prerequisites needed)" -ForegroundColor White
    Write-Host ""
    Write-Host "For developers:" -ForegroundColor Cyan
    Write-Host "  - Install .NET 9 SDK" -ForegroundColor White
    Write-Host "  - Install Visual Studio (for native DLL)" -ForegroundColor White
}
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Additional system info
Write-Host "System Information:" -ForegroundColor Gray
Write-Host "  OS: $([System.Environment]::OSVersion.VersionString)" -ForegroundColor Gray
Write-Host "  Architecture: $([System.Environment]::Is64BitOperatingSystem ? 'x64' : 'x86')" -ForegroundColor Gray
Write-Host ""
