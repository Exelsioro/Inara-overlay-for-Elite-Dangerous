# ED Inara Overlay 2.0 - Automated Test Harness (PowerShell)
# =============================================================

Write-Host "ED Inara Overlay 2.0 - Automated Test Harness" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "This harness will test the overlay focus behavior:" -ForegroundColor Yellow
Write-Host "1. Start app with no target - waiting window should be visible" -ForegroundColor Yellow
Write-Host "2. Launch mock target - waiting should hide, overlay should appear" -ForegroundColor Yellow
Write-Host "3. Change focus to Notepad - overlay should hide" -ForegroundColor Yellow
Write-Host "4. Return focus to mock target - overlay should reappear" -ForegroundColor Yellow
Write-Host ""

# Set paths
$AppPath = Join-Path $PSScriptRoot "bin\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe"
$MockTargetName = "MockTargetApp"
$MockTargetPath = Join-Path $PSScriptRoot "MockTargetApp.exe"

# Check if application exists
if (-not (Test-Path $AppPath)) {
    Write-Host "ERROR: Application executable not found at $AppPath" -ForegroundColor Red
    Write-Host "Please build the application first using: dotnet build" -ForegroundColor Red
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

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Green
Write-Host "TEST 1: Starting overlay app with no target running" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Starting overlay app targeting '$MockTargetName' (which doesn't exist yet)..." -ForegroundColor White
$overlayProcess = Start-Process -FilePath $AppPath -ArgumentList $MockTargetName -PassThru
Write-Host "Overlay process started with PID: $($overlayProcess.Id)" -ForegroundColor Gray

Write-Host ""
Write-Host "VERIFY: A waiting window should appear showing 'Looking for: MockTargetApp.exe'" -ForegroundColor Yellow
Write-Host ""
Wait-UserInput

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "TEST 2: Launching mock target application" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Compiling MockTargetApp..." -ForegroundColor White
$compileResult = & csc /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:MockTargetApp.exe ../MockTargetApp.cs 2>&1

if (-not (Test-Path "MockTargetApp.exe")) {
    Write-Host "ERROR: Failed to compile MockTargetApp.exe" -ForegroundColor Red
    Write-Host "Make sure you have .NET SDK installed" -ForegroundColor Red
    Write-Host "Compile output: $compileResult" -ForegroundColor Red
    Wait-UserInput "Press Enter to exit"
    exit 1
}

Write-Host "Starting MockTargetApp..." -ForegroundColor White
$mockProcess = Start-Process -FilePath "MockTargetApp.exe" -PassThru
Write-Host "Mock target started with PID: $($mockProcess.Id)" -ForegroundColor Gray

Write-Host ""
Write-Host "VERIFY:" -ForegroundColor Yellow
Write-Host "- The waiting window should disappear" -ForegroundColor Yellow
Write-Host "- The overlay should appear over the mock target window" -ForegroundColor Yellow  
Write-Host "- The overlay should be visible when the mock target has focus" -ForegroundColor Yellow
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
Write-Host ""
Wait-UserInput

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "TEST 4: Cleanup and preparation for Elite Dangerous test" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "Cleaning up test applications..." -ForegroundColor White
Stop-TestProcesses

Write-Host "Mock test completed successfully!" -ForegroundColor Green
Write-Host ""

Write-Host "================================================" -ForegroundColor Green
Write-Host "TEST 5: Testing with Elite Dangerous (if available)" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
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
Write-Host "Test Harness Complete" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "All tests have been run. The overlay should demonstrate:" -ForegroundColor Green
Write-Host "- Waiting window when target is not running" -ForegroundColor Green
Write-Host "- Overlay appears when target is detected" -ForegroundColor Green
Write-Host "- Overlay hides when target loses focus" -ForegroundColor Green
Write-Host "- Overlay reappears when target regains focus" -ForegroundColor Green
Write-Host ""

Wait-UserInput "Press Enter to close any remaining test applications..."

Write-Host "Cleaning up..." -ForegroundColor White
Stop-TestProcesses

# Clean up compiled mock app
if (Test-Path "MockTargetApp.exe") {
    Remove-Item "MockTargetApp.exe" -Force
    Write-Host "Removed MockTargetApp.exe" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Test harness completed successfully!" -ForegroundColor Green
Write-Host ""
Wait-UserInput "Press Enter to exit"
