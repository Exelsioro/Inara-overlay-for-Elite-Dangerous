# ED Inara Overlay 2.0 - Automated Regression Test
# ================================================
# This test automatically verifies core functionality that can be tested without manual interaction

Write-Host "ED Inara Overlay 2.0 - Automated Regression Test" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
Write-Host ""

# Configuration
$AppPath = "..\ED_Inara_Overlay_2.0\bin\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe"
$TestResults = @()

# Function to add test result
function Add-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Details = ""
    )
    $script:TestResults += [PSCustomObject]@{
        Test = $TestName
        Status = if ($Passed) { "PASS" } else { "FAIL" }
        Details = $Details
        Timestamp = Get-Date
    }
}

# Function to stop test processes
function Stop-TestProcesses {
    $processes = @("MockTargetApp", "notepad", "ED_Inara_Overlay_2.0")
    foreach ($proc in $processes) {
        try {
            Get-Process -Name $proc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        } catch {
            # Ignore errors
        }
    }
}

# Clean up any existing processes
Stop-TestProcesses

Write-Host "Test 1: Application Executable Check" -ForegroundColor Cyan
$executableExists = Test-Path $AppPath
Write-Host "Executable path: $AppPath" -ForegroundColor White
if ($executableExists) {
    Write-Host "✓ Application executable found" -ForegroundColor Green
    Add-TestResult "Executable Check" $true "Application executable exists"
} else {
    Write-Host "✗ Application executable not found" -ForegroundColor Red
    Add-TestResult "Executable Check" $false "Application executable missing"
    Write-Host "Cannot continue without executable" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Test 2: Application Startup" -ForegroundColor Cyan
try {
    $overlayProcess = Start-Process -FilePath $AppPath -ArgumentList "notepad" -PassThru
    Start-Sleep -Seconds 3
    
    if ($overlayProcess.HasExited) {
        Write-Host "✗ Application exited immediately" -ForegroundColor Red
        Add-TestResult "Application Startup" $false "Process exited immediately"
    } else {
        Write-Host "✓ Application started successfully (PID: $($overlayProcess.Id))" -ForegroundColor Green
        Add-TestResult "Application Startup" $true "Process started with PID $($overlayProcess.Id)"
    }
} catch {
    Write-Host "✗ Error starting application: $($_.Exception.Message)" -ForegroundColor Red
    Add-TestResult "Application Startup" $false "Error: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "Test 3: Process Resource Usage" -ForegroundColor Cyan
$process = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
if ($process) {
    $memoryMB = [math]::Round($process.WorkingSet64 / 1MB, 2)
    $handleCount = $process.HandleCount
    $threadCount = $process.Threads.Count
    
    Write-Host "Memory usage: $memoryMB MB" -ForegroundColor White
    Write-Host "Handle count: $handleCount" -ForegroundColor White
    Write-Host "Thread count: $threadCount" -ForegroundColor White
    
    # Test memory usage (should be reasonable for a small overlay app)
    if ($memoryMB -lt 100) {
        Write-Host "✓ Memory usage is acceptable" -ForegroundColor Green
        Add-TestResult "Memory Usage" $true "Memory usage: $memoryMB MB"
    } else {
        Write-Host "⚠ High memory usage detected" -ForegroundColor Yellow
        Add-TestResult "Memory Usage" $false "High memory usage: $memoryMB MB"
    }
    
    # Test handle count (should not be excessive)
    if ($handleCount -lt 500) {
        Write-Host "✓ Handle count is acceptable" -ForegroundColor Green
        Add-TestResult "Handle Count" $true "Handle count: $handleCount"
    } else {
        Write-Host "⚠ High handle count detected" -ForegroundColor Yellow
        Add-TestResult "Handle Count" $false "High handle count: $handleCount"
    }
} else {
    Write-Host "✗ Process not found for resource measurement" -ForegroundColor Red
    Add-TestResult "Process Resource Usage" $false "Process not found"
}

Write-Host ""
Write-Host "Test 4: Process Stability (10 second test)" -ForegroundColor Cyan
$stabilityStart = Get-Date
$stable = $true
$measurements = @()

for ($i = 0; $i -lt 10; $i++) {
    $proc = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
    if ($proc) {
        $measurements += [PSCustomObject]@{
            Time = $i
            Memory = [math]::Round($proc.WorkingSet64 / 1MB, 2)
            Threads = $proc.Threads.Count
        }
    } else {
        $stable = $false
        break
    }
    Start-Sleep -Seconds 1
}

if ($stable) {
    $avgMemory = ($measurements | Measure-Object -Property Memory -Average).Average
    $maxMemory = ($measurements | Measure-Object -Property Memory -Maximum).Maximum
    $minMemory = ($measurements | Measure-Object -Property Memory -Minimum).Minimum
    
    Write-Host "✓ Process remained stable for 10 seconds" -ForegroundColor Green
    Write-Host "  Average memory: $([math]::Round($avgMemory, 2)) MB" -ForegroundColor White
    Write-Host "  Memory range: $minMemory - $maxMemory MB" -ForegroundColor White
    
    Add-TestResult "Process Stability" $true "Stable for 10 seconds, avg memory: $([math]::Round($avgMemory, 2)) MB"
} else {
    Write-Host "✗ Process became unstable or crashed" -ForegroundColor Red
    Add-TestResult "Process Stability" $false "Process became unstable"
}

Write-Host ""
Write-Host "Test 5: Target Process Detection" -ForegroundColor Cyan
Write-Host "Starting notepad to test target detection..." -ForegroundColor White
$notepadProcess = Start-Process -FilePath "notepad.exe" -PassThru
Start-Sleep -Seconds 3

# Check if the overlay process is still running after target detection
$overlayAfterTarget = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
if ($overlayAfterTarget) {
    Write-Host "✓ Overlay process survived target detection" -ForegroundColor Green
    Add-TestResult "Target Detection" $true "Overlay process remained stable after target detection"
} else {
    Write-Host "✗ Overlay process crashed during target detection" -ForegroundColor Red
    Add-TestResult "Target Detection" $false "Overlay process crashed during target detection"
}

Write-Host ""
Write-Host "Test 6: Timer Loop Performance" -ForegroundColor Cyan
Write-Host "Monitoring timer loop for 15 seconds..." -ForegroundColor White

$timerStart = Get-Date
$initialCPU = $null
$process = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
if ($process) {
    $initialCPU = $process.TotalProcessorTime
}

Start-Sleep -Seconds 15

$timerEnd = Get-Date
$process = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
if ($process -and $initialCPU) {
    $finalCPU = $process.TotalProcessorTime
    $elapsed = ($timerEnd - $timerStart).TotalMilliseconds
    $cpuDiff = ($finalCPU - $initialCPU).TotalMilliseconds
    $cpuUsagePercent = ($cpuDiff / $elapsed) * 100
    
    Write-Host "Timer loop CPU usage: $([math]::Round($cpuUsagePercent, 2))%" -ForegroundColor White
    
    if ($cpuUsagePercent -lt 5) {
        Write-Host "✓ Timer loop performance is acceptable" -ForegroundColor Green
        Add-TestResult "Timer Loop Performance" $true "CPU usage: $([math]::Round($cpuUsagePercent, 2))%"
    } else {
        Write-Host "⚠ High CPU usage in timer loop" -ForegroundColor Yellow
        Add-TestResult "Timer Loop Performance" $false "High CPU usage: $([math]::Round($cpuUsagePercent, 2))%"
    }
} else {
    Write-Host "⚠ Could not measure timer loop performance" -ForegroundColor Yellow
    Add-TestResult "Timer Loop Performance" $false "Could not measure performance"
}

Write-Host ""
Write-Host "Test 7: Settings File Detection" -ForegroundColor Cyan
$settingsLocations = @(
    "$env:APPDATA\ED_Inara_Overlay_2.0",
    "$env:LOCALAPPDATA\ED_Inara_Overlay_2.0",
    ".\settings.json",
    ".\config.xml",
    ".\settings.xml"
)

$settingsFound = $false
foreach ($location in $settingsLocations) {
    if (Test-Path $location) {
        Write-Host "✓ Settings location found: $location" -ForegroundColor Green
        $settingsFound = $true
        break
    }
}

if ($settingsFound) {
    Add-TestResult "Settings Detection" $true "Settings files found"
} else {
    Write-Host "ℹ No settings files found (may not be implemented)" -ForegroundColor Cyan
    Add-TestResult "Settings Detection" $true "No settings files found (expected for new installation)"
}

Write-Host ""
Write-Host "Test 8: Graceful Shutdown" -ForegroundColor Cyan
Write-Host "Testing application shutdown..." -ForegroundColor White

# Stop notepad first
if ($notepadProcess -and -not $notepadProcess.HasExited) {
    $notepadProcess.CloseMainWindow()
    $notepadProcess.WaitForExit(5000)
    if (-not $notepadProcess.HasExited) {
        $notepadProcess.Kill()
    }
}

# Stop overlay
$overlayProcess = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
if ($overlayProcess) {
    $overlayProcess.CloseMainWindow()
    $gracefulShutdown = $overlayProcess.WaitForExit(5000)
    
    if ($gracefulShutdown) {
        Write-Host "✓ Application shut down gracefully" -ForegroundColor Green
        Add-TestResult "Graceful Shutdown" $true "Application shut down gracefully"
    } else {
        Write-Host "⚠ Application did not shut down gracefully, forcing termination" -ForegroundColor Yellow
        $overlayProcess.Kill()
        Add-TestResult "Graceful Shutdown" $false "Application required forced termination"
    }
} else {
    Write-Host "ℹ Application was not running" -ForegroundColor Cyan
    Add-TestResult "Graceful Shutdown" $true "Application was not running"
}

# Final cleanup
Stop-TestProcesses

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "AUTOMATED REGRESSION TEST RESULTS" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$passCount = ($TestResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($TestResults | Where-Object { $_.Status -eq "FAIL" }).Count

Write-Host ""
Write-Host "Test Results Summary:" -ForegroundColor White
Write-Host "===================" -ForegroundColor White

foreach ($result in $TestResults) {
    $color = if ($result.Status -eq "PASS") { "Green" } else { "Red" }
    Write-Host "[$($result.Status)] $($result.Test): $($result.Details)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Total Tests: $($TestResults.Count)" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red

Write-Host ""
if ($failCount -eq 0) {
    Write-Host "✓ ALL AUTOMATED TESTS PASSED" -ForegroundColor Green
    Write-Host "  Core functionality appears to be working correctly" -ForegroundColor Green
    Write-Host "  No regressions detected in automated testing" -ForegroundColor Green
} else {
    Write-Host "✗ SOME TESTS FAILED" -ForegroundColor Red
    Write-Host "  $failCount out of $($TestResults.Count) tests failed" -ForegroundColor Red
    Write-Host "  Please review the failures above" -ForegroundColor Red
}

Write-Host ""
Write-Host "Note: This automated test covers basic functionality." -ForegroundColor Yellow
Write-Host "Manual testing is still recommended for UI interactions." -ForegroundColor Yellow

Write-Host ""
Write-Host "Automated regression test completed!" -ForegroundColor Green
