# ED Inara Overlay 2.0 - Comprehensive Regression Test

Write-Host "ED Inara Overlay 2.0 - Comprehensive Regression Test" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green
Write-Host ""

# Configuration
$AppPath = Join-Path $PSScriptRoot "bin\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe"
$MockTargetPath = Join-Path $PSScriptRoot "..\MockTargetApp\bin\Debug\net8.0-windows\MockTargetApp.exe"
$MockTargetName = "MockTargetApp"
$TestResults = @()

# Function to add test result
function Add-TestResult {
    param(
        [string]$TestName,
        [string]$Status,
        [string]$Details = ""
    )
    $TestResults += [PSCustomObject]@{
        Test = $TestName
        Status = $Status
        Details = $Details
        Timestamp = Get-Date
    }
}

# Function to wait for user input
function Wait-UserInput {
    param([string]$Message = "Press Enter to continue...")
    Write-Host $Message -ForegroundColor Cyan
    Read-Host
}

# Function to kill test processes
function Stop-TestProcesses {
    $processes = @("MockTargetApp", "notepad", "ED_Inara_Overlay_2.0")
    foreach ($proc in $processes) {
        try {
            Get-Process -Name $proc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Host "Stopped $proc processes" -ForegroundColor Gray
        } catch {
            # Ignore errors
        }
    }
}

# Function to measure CPU usage
function Measure-CPUUsage {
    param(
        [string]$ProcessName,
        [int]$DurationSeconds = 10
    )
    
    $process = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        return $null
    }
    
    $cpu1 = (Get-Counter "\Process($ProcessName)\% Processor Time" -ErrorAction SilentlyContinue).CounterSamples.CookedValue
    Start-Sleep -Seconds $DurationSeconds
    $cpu2 = (Get-Counter "\Process($ProcessName)\% Processor Time" -ErrorAction SilentlyContinue).CounterSamples.CookedValue
    
    return [math]::Round(($cpu2 - $cpu1) / $DurationSeconds, 2)
}

# Function to test window positioning
function Test-WindowPosition {
    param(
        [string]$WindowTitle,
        [int]$ExpectedX,
        [int]$ExpectedY,
        [int]$Tolerance = 50
    )
    
    Add-Type -TypeDefinition @"
        using System;
        using System.Runtime.InteropServices;
        using System.Text;
        
        public class WindowAPI {
            [DllImport("user32.dll")]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            
            [DllImport("user32.dll")]
            public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
            
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
        }
"@
    
    $hwnd = [WindowAPI]::FindWindow($null, $WindowTitle)
    if ($hwnd -eq [IntPtr]::Zero) {
        return $false
    }
    
    $rect = New-Object WindowAPI+RECT
    $success = [WindowAPI]::GetWindowRect($hwnd, [ref]$rect)
    
    if ($success) {
        $actualX = $rect.Left
        $actualY = $rect.Top
        $deltaX = [math]::Abs($actualX - $ExpectedX)
        $deltaY = [math]::Abs($actualY - $ExpectedY)
        
        return ($deltaX -le $Tolerance -and $deltaY -le $Tolerance)
    }
    
    return $false
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
if (-not (Test-Path $AppPath)) {
    Write-Host "ERROR: Application not found at $AppPath" -ForegroundColor Red
    Add-TestResult "Prerequisites" "FAIL" "Application executable not found"
    exit 1
}

if (-not (Test-Path $MockTargetPath)) {
    Write-Host "WARNING: Mock target not found at $MockTargetPath" -ForegroundColor Yellow
    Write-Host "Some tests will be skipped" -ForegroundColor Yellow
    Add-TestResult "Prerequisites" "PARTIAL" "Mock target not available"
} else {
    Add-TestResult "Prerequisites" "PASS" "All required files found"
}

# Clean up any existing processes
Stop-TestProcesses

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 1: Application Startup Behavior" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "Starting overlay application..." -ForegroundColor White
$overlayProcess = Start-Process -FilePath $AppPath -ArgumentList $MockTargetName -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 2

if ($overlayProcess.HasExited) {
    Write-Host "ERROR: Application exited immediately" -ForegroundColor Red
    Add-TestResult "Application Startup" "FAIL" "Process exited immediately"
} else {
    Write-Host "✓ Application started successfully" -ForegroundColor Green
    Add-TestResult "Application Startup" "PASS" "Process started and running"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 2: CPU Usage Measurement" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "Measuring CPU usage for 10 seconds..." -ForegroundColor White
$cpuUsage = Measure-CPUUsage -ProcessName "ED_Inara_Overlay_2.0" -DurationSeconds 10

if ($null -eq $cpuUsage) {
    Write-Host "WARNING: Could not measure CPU usage" -ForegroundColor Yellow
    Add-TestResult "CPU Usage" "PARTIAL" "Could not measure CPU usage"
} elseif ($cpuUsage -lt 5.0) {
    Write-Host "✓ CPU usage is acceptable: $cpuUsage%" -ForegroundColor Green
    Add-TestResult "CPU Usage" "PASS" "CPU usage: $cpuUsage%"
} else {
    Write-Host "WARNING: High CPU usage detected: $cpuUsage%" -ForegroundColor Yellow
    Add-TestResult "CPU Usage" "WARN" "CPU usage: $cpuUsage% (high)"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 3: Window Focus Behavior" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

if (Test-Path $MockTargetPath) {
    Write-Host "Starting mock target application..." -ForegroundColor White
    $mockProcess = Start-Process -FilePath $MockTargetPath -PassThru
    Start-Sleep -Seconds 3
    
    Write-Host ""
    Write-Host "MANUAL TEST REQUIRED:" -ForegroundColor Yellow
    Write-Host "1. Verify the waiting window disappears" -ForegroundColor Yellow
    Write-Host "2. Verify the overlay appears over the mock target" -ForegroundColor Yellow
    Write-Host "3. Click on a different window (like this PowerShell window)" -ForegroundColor Yellow
    Write-Host "4. Verify the overlay hides when target loses focus" -ForegroundColor Yellow
    Write-Host "5. Click back on the mock target" -ForegroundColor Yellow
    Write-Host "6. Verify the overlay reappears when target regains focus" -ForegroundColor Yellow
    Write-Host ""
    
    $response = Read-Host "Did the focus behavior work correctly? (y/n)"
    if ($response -eq "y" -or $response -eq "Y") {
        Write-Host "✓ Focus behavior verified manually" -ForegroundColor Green
        Add-TestResult "Focus Behavior" "PASS" "Manual verification successful"
    } else {
        Write-Host "✗ Focus behavior failed verification" -ForegroundColor Red
        Add-TestResult "Focus Behavior" "FAIL" "Manual verification failed"
    }
} else {
    Write-Host "Skipping focus behavior test (mock target not available)" -ForegroundColor Yellow
    Add-TestResult "Focus Behavior" "SKIP" "Mock target not available"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 4: Window Drag and Resize" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "MANUAL TEST REQUIRED:" -ForegroundColor Yellow
Write-Host "1. Try to drag the overlay window" -ForegroundColor Yellow
Write-Host "2. Try to resize the overlay window" -ForegroundColor Yellow
Write-Host "3. Verify the overlay maintains its position relative to target" -ForegroundColor Yellow
Write-Host ""

$response = Read-Host "Did the window drag/resize behavior work as expected? (y/n)"
if ($response -eq "y" -or $response -eq "Y") {
    Write-Host "✓ Window drag/resize behavior verified" -ForegroundColor Green
    Add-TestResult "Window Drag/Resize" "PASS" "Manual verification successful"
} else {
    Write-Host "✗ Window drag/resize behavior failed" -ForegroundColor Red
    Add-TestResult "Window Drag/Resize" "FAIL" "Manual verification failed"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 5: Settings Persistence" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "Checking for settings files..." -ForegroundColor White
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
        Write-Host "✓ Settings found at: $location" -ForegroundColor Green
        $settingsFound = $true
        break
    }
}

if (-not $settingsFound) {
    Write-Host "WARNING: No settings files found" -ForegroundColor Yellow
    Write-Host "This may indicate settings are not persisted" -ForegroundColor Yellow
    Add-TestResult "Settings Persistence" "PARTIAL" "No settings files found"
} else {
    Write-Host "✓ Settings persistence appears to be implemented" -ForegroundColor Green
    Add-TestResult "Settings Persistence" "PASS" "Settings files found"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 6: Tray Icon Controls" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "Checking for tray icon..." -ForegroundColor White
Write-Host "MANUAL TEST REQUIRED:" -ForegroundColor Yellow
Write-Host "1. Look for a tray icon in the system tray" -ForegroundColor Yellow
Write-Host "2. If present, right-click on it to test context menu" -ForegroundColor Yellow
Write-Host "3. Test any show/hide functionality" -ForegroundColor Yellow
Write-Host ""

$response = Read-Host "Is there a tray icon and does it work correctly? (y/n/not-present)"
if ($response -eq "y" -or $response -eq "Y") {
    Write-Host "✓ Tray icon functionality verified" -ForegroundColor Green
    Add-TestResult "Tray Icon" "PASS" "Manual verification successful"
} elseif ($response -eq "not-present" -or $response -eq "n") {
    Write-Host "NOTE: Tray icon not present or not working" -ForegroundColor Yellow
    Add-TestResult "Tray Icon" "NOT_IMPLEMENTED" "Tray icon not present"
} else {
    Write-Host "✗ Tray icon functionality failed" -ForegroundColor Red
    Add-TestResult "Tray Icon" "FAIL" "Manual verification failed"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 7: Memory Usage Check" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$process = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
if ($process) {
    $memoryMB = [math]::Round($process.WorkingSet64 / 1MB, 2)
    Write-Host "Memory usage: $memoryMB MB" -ForegroundColor White
    
    if ($memoryMB -lt 100) {
        Write-Host "✓ Memory usage is acceptable" -ForegroundColor Green
        Add-TestResult "Memory Usage" "PASS" "Memory usage: $memoryMB MB"
    } else {
        Write-Host "WARNING: High memory usage detected" -ForegroundColor Yellow
        Add-TestResult "Memory Usage" "WARN" "Memory usage: $memoryMB MB (high)"
    }
} else {
    Write-Host "WARNING: Could not measure memory usage" -ForegroundColor Yellow
    Add-TestResult "Memory Usage" "PARTIAL" "Process not found for measurement"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST 8: Timer Loop Performance" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "Measuring timer loop performance..." -ForegroundColor White
Write-Host "Running for 30 seconds to check for performance issues..." -ForegroundColor White

# Monitor for 30 seconds
$startTime = Get-Date
$performanceIssues = 0
$iterations = 0

while ((Get-Date) -lt $startTime.AddSeconds(30)) {
    $process = Get-Process -Name "ED_Inara_Overlay_2.0" -ErrorAction SilentlyContinue
    if ($process) {
        $currentCPU = $process.CPU
        Start-Sleep -Seconds 1
        $newCPU = $process.CPU
        
        if ($newCPU -and $currentCPU) {
            $cpuDiff = $newCPU - $currentCPU
            if ($cpuDiff -gt 0.5) {  # High CPU usage per second
                $performanceIssues++
            }
        }
    }
    $iterations++
}

if ($performanceIssues -lt ($iterations * 0.1)) {  # Less than 10% high CPU samples
    Write-Host "✓ Timer loop performance is acceptable" -ForegroundColor Green
    Add-TestResult "Timer Loop Performance" "PASS" "Performance stable over 30 seconds"
} else {
    Write-Host "WARNING: Timer loop performance issues detected" -ForegroundColor Yellow
    Add-TestResult "Timer Loop Performance" "WARN" "Performance issues detected"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Cleanup" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "Stopping test processes..." -ForegroundColor White
Stop-TestProcesses

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "TEST RESULTS SUMMARY" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

$passCount = ($TestResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($TestResults | Where-Object { $_.Status -eq "FAIL" }).Count
$warnCount = ($TestResults | Where-Object { $_.Status -eq "WARN" }).Count
$skipCount = ($TestResults | Where-Object { $_.Status -eq "SKIP" }).Count
$partialCount = ($TestResults | Where-Object { $_.Status -eq "PARTIAL" }).Count
$notImplementedCount = ($TestResults | Where-Object { $_.Status -eq "NOT_IMPLEMENTED" }).Count

Write-Host ""
Write-Host "Test Results:" -ForegroundColor White
Write-Host "=============" -ForegroundColor White

foreach ($result in $TestResults) {
    $color = switch ($result.Status) {
        "PASS" { "Green" }
        "FAIL" { "Red" }
        "WARN" { "Yellow" }
        "SKIP" { "Gray" }
        "PARTIAL" { "Yellow" }
        "NOT_IMPLEMENTED" { "Cyan" }
        default { "White" }
    }
    
    Write-Host "[$($result.Status)] $($result.Test): $($result.Details)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor White
Write-Host "  PASS: $passCount" -ForegroundColor Green
Write-Host "  FAIL: $failCount" -ForegroundColor Red
Write-Host "  WARN: $warnCount" -ForegroundColor Yellow
Write-Host "  SKIP: $skipCount" -ForegroundColor Gray
Write-Host "  PARTIAL: $partialCount" -ForegroundColor Yellow
Write-Host "  NOT_IMPLEMENTED: $notImplementedCount" -ForegroundColor Cyan

Write-Host ""
if ($failCount -eq 0) {
    Write-Host "✓ REGRESSION TEST PASSED - No critical failures detected" -ForegroundColor Green
    Write-Host "  All existing functionality appears to be working correctly" -ForegroundColor Green
} else {
    Write-Host "✗ REGRESSION TEST FAILED - $failCount critical failures detected" -ForegroundColor Red
    Write-Host "  Some existing functionality may have been broken" -ForegroundColor Red
}

Write-Host ""
Write-Host "Regression test completed!" -ForegroundColor Green
Wait-UserInput "Press Enter to exit..."
