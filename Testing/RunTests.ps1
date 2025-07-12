# ED Inara Overlay 2.0 - Test Harness
# ====================================

Write-Host "ED Inara Overlay 2.0 - Test Harness" -ForegroundColor Green
Write-Host "===================================" -ForegroundColor Green
Write-Host ""

# Set paths
$AppPath = "..\ED_Inara_Overlay_2.0\bin\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe"
$MockTargetPath = "..\MockTargetApp\bin\Debug\net8.0-windows\MockTargetApp.exe"

# Check if files exist
if (-not (Test-Path $AppPath)) {
    Write-Host "ERROR: Application not found at $AppPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $MockTargetPath)) {
    Write-Host "ERROR: Mock target not found at $MockTargetPath" -ForegroundColor Red
    exit 1
}

# Function to stop test processes
function Stop-TestProcesses {
    @("MockTargetApp", "notepad", "ED_Inara_Overlay_2.0") | ForEach-Object {
        Get-Process -Name $_ -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "TEST 1: Starting overlay with no target (should show waiting window)" -ForegroundColor Yellow
Stop-TestProcesses
Start-Process -FilePath $AppPath -ArgumentList "MockTargetApp"
Read-Host "Press Enter after verifying waiting window appears"

Write-Host "TEST 2: Starting mock target (waiting should disappear, overlay should appear)" -ForegroundColor Yellow
Start-Process -FilePath $MockTargetPath
Read-Host "Press Enter after verifying overlay appears"

Write-Host "TEST 3: Testing focus behavior with Notepad" -ForegroundColor Yellow
Start-Process -FilePath "notepad.exe"
Write-Host "1. Click Notepad - overlay should hide" -ForegroundColor Cyan
Write-Host "2. Click Mock Target - overlay should reappear" -ForegroundColor Cyan
Read-Host "Press Enter after testing focus behavior"

Write-Host "TEST 4: Testing with Elite Dangerous" -ForegroundColor Yellow
Stop-TestProcesses
Start-Process -FilePath $AppPath -ArgumentList "EliteDangerous64"
Write-Host "If Elite Dangerous is running, overlay should appear immediately" -ForegroundColor Cyan
Write-Host "If not, waiting window should appear until you start Elite Dangerous" -ForegroundColor Cyan
Read-Host "Press Enter when done testing"

Write-Host "Cleaning up..." -ForegroundColor White
Stop-TestProcesses
Write-Host "Test completed!" -ForegroundColor Green
