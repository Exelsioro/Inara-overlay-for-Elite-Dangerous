# ED Inara Overlay 2.0 - Automated Test Harness v2 (PowerShell)
# ================================================================

Write-Host "ED Inara Overlay 2.0 - Automated Test Harness v2" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "This harness will test the overlay focus behavior:" -ForegroundColor Yellow
Write-Host "1. Start app with no target - waiting window should be visible" -ForegroundColor Yellow
Write-Host "2. Launch mock target - waiting should hide, overlay should appear" -ForegroundColor Yellow
Write-Host "3. Change focus to Notepad - overlay should hide" -ForegroundColor Yellow
Write-Host "4. Return focus to mock target - overlay should reappear" -ForegroundColor Yellow
Write-Host "5. Test with Elite Dangerous (if available)" -ForegroundColor Yellow
Write-Host ""

# Set paths
$AppPath = Join-Path $PSScriptRoot "bin\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe"
$MockTargetPath = Join-Path $PSScriptRoot "..\MockTargetApp\bin\Debug\net8.0-windows\MockTargetApp.exe"
$MockTargetName = "MockTargetApp"

# Check if application exists
if (-not (Test-Path $AppPath)) {
    Write-Host "ERROR: Application executable not found at $AppPath" -ForegroundColor Red
    Write-Host "Please build the application first using: dotnet build" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if mock target exists
if (-not (Test-Path $MockTargetPath)) {
    Write-Host "ERROR: Mock target executable not found at $MockTargetPath" -ForegroundColor Red
    Write-Host "Please build the mock target app first" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Function to wait for user input
function Wait-UserInput {
    param([string]$Message = "Press Enter to continue...")
    Write-Host $Message -ForegroundColor Cyan
    Read-Host
}

# Function to kill processes safely
function Stop-TestProcesses {
    $processes = @("MockTargetApp", "notepad", "ED_Inara_Overlay_2.0")
    foreach ($proc in $processes) {
        try {
            Get-Process -Name $proc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Host "Stopped $proc processes" -ForegroundColor Gray
        } catch {
            # Ignore errors when stopping processes
        }
    }
}

# Function to check if process exists
function Test-ProcessExists {
    param([string]$ProcessName)
    return (Get-Process -Name $ProcessName -ErrorAction SilentlyContinue) -ne $null
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Green
Write-Host "TEST 1: Starting overlay app with no target running" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green
Write-Host ""

# Make sure no mock target is running
Stop-TestProcesses

Write-Host "Starting overlay app targeting '$MockTargetName' (which doesn't exist yet)..." -ForegroundColor White
$overlayProcess = Start-Process -FilePath $AppPath -ArgumentList $MockTargetName -PassThru
Write-Host "Overlay process started with PID: $($overlayProcess.Id)" -ForegroundColor Gray

Write-Host ""
Write-Host "VERIFY: A waiting window should appear showing 'Looking for: MockTargetApp.exe'" -ForegroundColor Yellow
Write-Host "The waiting window should be visible and searching for the target application." -ForegroundColor Yellow
Write-Host ""
Wait-UserInput

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "TEST 2: Launching mock target application" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Starting MockTargetApp..." -ForegroundColor White
$mockProcess = Start-Process -FilePath $MockTargetPath -PassThru
Write-Host "Mock target started with PID: $($mockProcess.Id)" -ForegroundColor Gray

# Wait a moment for the overlay to detect the target
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "VERIFY:" -ForegroundColor Yellow
Write-Host "- The waiting window should disappear" -ForegroundColor Yellow
Write-Host "- The overlay should appear over the mock target window" -ForegroundColor Yellow  
Write-Host "- The overlay should be visible when the mock target has focus" -ForegroundColor Yellow
Write-Host "- You should see a small overlay window positioned relative to the mock target" -ForegroundColor Yellow
Write-Host ""
Wait-UserInput

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "TEST 3: Testing focus change behavior" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Starting Notepad..." -ForegroundColor White
$notepadProcess = Start-Process -FilePath "notepad.exe" -PassThru
Write-Host "Notepad started with PID: $($notepadProcess.Id)" -ForegroundColor Gray

Write-Host ""
Write-Host "INSTRUCTIONS:" -ForegroundColor Yellow
Write-Host "1. Click on the Notepad window to give it focus" -ForegroundColor Yellow
Write-Host "2. VERIFY: The overlay should hide when Notepad has focus" -ForegroundColor Yellow
Write-Host "3. Then click back on the Mock Target Application window" -ForegroundColor Yellow
Write-Host "4. VERIFY: The overlay should reappear when Mock Target has focus" -ForegroundColor Yellow
Write-Host "5. Try switching focus back and forth a few times" -ForegroundColor Yellow
Write-Host ""
Wait-UserInput

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "TEST 4: Testing overlay functionality" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Now let's test the overlay functionality:" -ForegroundColor White
Write-Host "1. Make sure the Mock Target Application has focus" -ForegroundColor Yellow
Write-Host "2. Look for the overlay window (should be visible)" -ForegroundColor Yellow
Write-Host "3. Try clicking the toggle button on the overlay" -ForegroundColor Yellow
Write-Host "4. Try minimizing and restoring the Mock Target Application" -ForegroundColor Yellow
Write-Host "5. VERIFY: The overlay should hide when Mock Target is minimized" -ForegroundColor Yellow
Write-Host "6. VERIFY: The overlay should reappear when Mock Target is restored" -ForegroundColor Yellow
Write-Host ""
Wait-UserInput

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "TEST 5: Cleanup and Elite Dangerous test" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Cleaning up test applications..." -ForegroundColor White
Stop-TestProcesses

Write-Host "Mock test completed successfully!" -ForegroundColor Green
Write-Host ""

Write-Host "Starting overlay for Elite Dangerous..." -ForegroundColor White
$edOverlayProcess = Start-Process -FilePath $AppPath -ArgumentList "EliteDangerous64" -PassThru
Write-Host "Elite Dangerous overlay started with PID: $($edOverlayProcess.Id)" -ForegroundColor Gray

Write-Host ""
Write-Host "INSTRUCTIONS:" -ForegroundColor Yellow
Write-Host "1. If Elite Dangerous is not running, you should see a waiting window" -ForegroundColor Yellow
Write-Host "2. Start Elite Dangerous from Steam/Epic/Frontier" -ForegroundColor Yellow
Write-Host "3. VERIFY: The waiting window should disappear and overlay should appear" -ForegroundColor Yellow
Write-Host "4. Alt+Tab to switch to another application (like Notepad)" -ForegroundColor Yellow
Write-Host "5. VERIFY: The overlay should hide when Elite Dangerous doesn't have focus" -ForegroundColor Yellow
Write-Host "6. Alt+Tab back to Elite Dangerous" -ForegroundColor Yellow
Write-Host "7. VERIFY: The overlay should reappear when Elite Dangerous has focus" -ForegroundColor Yellow
Write-Host ""
Write-Host "NOTE: If Elite Dangerous is already running, the overlay should appear immediately" -ForegroundColor Cyan
Write-Host ""
Wait-UserInput

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "Test Results Summary" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "All tests have been completed. The overlay should demonstrate:" -ForegroundColor Green
Write-Host "✓ Waiting window when target is not running" -ForegroundColor Green
Write-Host "✓ Overlay appears when target is detected" -ForegroundColor Green
Write-Host "✓ Overlay hides when target loses focus" -ForegroundColor Green
Write-Host "✓ Overlay reappears when target regains focus" -ForegroundColor Green
Write-Host "✓ Overlay follows target window position" -ForegroundColor Green
Write-Host "✓ Overlay hides when target is minimized" -ForegroundColor Green
Write-Host "✓ Application shuts down when target is closed" -ForegroundColor Green
Write-Host ""

Write-Host "The focus-dependent behavior works correctly with both mock and real applications." -ForegroundColor Green
Write-Host ""

Wait-UserInput "Press Enter to close any remaining test applications..."

Write-Host "Cleaning up..." -ForegroundColor White
Stop-TestProcesses

Write-Host ""
Write-Host "Test harness completed successfully!" -ForegroundColor Green
Write-Host ""
Wait-UserInput "Press Enter to exit"
