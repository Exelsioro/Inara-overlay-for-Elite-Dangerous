# Build script for Elite Dangerous Inara Overlay and Installer Creation
# This script builds the application and creates an installer using Inno Setup

param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU",
    [switch]$SkipBuild = $false,
    [switch]$SkipInstaller = $false
)

# Set error handling
$ErrorActionPreference = "Stop"

# Script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$ProjectDir = $ScriptDir

Write-Host "Elite Dangerous Inara Overlay - Build and Installer Script" -ForegroundColor Green
Write-Host "=================================================================" -ForegroundColor Green

# Check if Inno Setup is installed
$InnoSetupPath = ""
$PossiblePaths = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 5\ISCC.exe"
)

foreach ($Path in $PossiblePaths) {
    if (Test-Path $Path) {
        $InnoSetupPath = $Path
        break
    }
}

if (-not $InnoSetupPath -and -not $SkipInstaller) {
    Write-Host "Warning: Inno Setup not found. Please install Inno Setup 6 from https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "Installer creation will be skipped." -ForegroundColor Yellow
    $SkipInstaller = $true
}

# Build the application
if (-not $SkipBuild) {
    Write-Host "`nBuilding application..." -ForegroundColor Cyan
    
    # Change to the project directory
    Set-Location $ProjectDir
    
    # Clean previous builds
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "ED_Inara_Overlay\bin") {
        Remove-Item "ED_Inara_Overlay\bin" -Recurse -Force
    }
    if (Test-Path "ED_Inara_Overlay\obj") {
        Remove-Item "ED_Inara_Overlay\obj" -Recurse -Force
    }
    
    # Build the solution
    Write-Host "Building solution..." -ForegroundColor Yellow
    $BuildResult = dotnet build "ED_Inara_Overlay\ED_Inara_Overlay.sln" --configuration $Configuration --verbosity minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    
    # Verify build output
    $ExePath = "ED_Inara_Overlay\bin\$Configuration\net8.0-windows\ED_Inara_Overlay.exe"
    if (-not (Test-Path $ExePath)) {
        Write-Host "Error: Built executable not found at $ExePath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Executable found at: $ExePath" -ForegroundColor Green
} else {
    Write-Host "Skipping build step..." -ForegroundColor Yellow
}

# Create the installer
if (-not $SkipInstaller) {
    Write-Host "`nCreating installer..." -ForegroundColor Cyan
    
    # Ensure dist directory exists
    if (-not (Test-Path "dist")) {
        New-Item -ItemType Directory -Path "dist" | Out-Null
    }
    
    # Run Inno Setup compiler
    Write-Host "Running Inno Setup compiler..." -ForegroundColor Yellow
    $InstallerScript = Join-Path $ProjectDir "installer.iss"
    
    if (-not (Test-Path $InstallerScript)) {
        Write-Host "Error: Installer script not found at $InstallerScript" -ForegroundColor Red
        exit 1
    }
    
    $InnoArgs = @(
        "/Q",  # Quiet mode
        "`"$InstallerScript`""
    )
    
    $InnoProcess = Start-Process -FilePath $InnoSetupPath -ArgumentList $InnoArgs -Wait -PassThru -NoNewWindow
    
    if ($InnoProcess.ExitCode -ne 0) {
        Write-Host "Installer creation failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Installer created successfully!" -ForegroundColor Green
    
    # List created files
    $DistFiles = Get-ChildItem "dist" -Filter "*.exe" | Sort-Object LastWriteTime -Descending
    if ($DistFiles.Count -gt 0) {
        Write-Host "`nCreated installer files:" -ForegroundColor Cyan
        foreach ($File in $DistFiles) {
            $FileSize = [math]::Round($File.Length / 1MB, 2)
            Write-Host "  $($File.Name) ($FileSize MB)" -ForegroundColor White
        }
    }
} else {
    Write-Host "Skipping installer creation..." -ForegroundColor Yellow
}

# Summary
Write-Host "`n=================================================================" -ForegroundColor Green
Write-Host "Build and installer creation completed!" -ForegroundColor Green

if (-not $SkipBuild) {
    Write-Host "✓ Application built successfully" -ForegroundColor Green
}
if (-not $SkipInstaller) {
    Write-Host "✓ Installer created successfully" -ForegroundColor Green
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Test the installer by running it on a clean system" -ForegroundColor White
Write-Host "2. Verify the application launches correctly after installation" -ForegroundColor White
Write-Host "3. Test the uninstaller to ensure clean removal" -ForegroundColor White

Write-Host "`nPress any key to continue..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
