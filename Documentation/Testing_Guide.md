# ED Inara Overlay 2.0 - Testing Guide

## Overview

This guide provides comprehensive instructions for testing the ED Inara Overlay 2.0 application. The application is a WPF-based overlay system designed to display trade route information for Elite Dangerous players using data from Inara.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Test Environment Setup](#test-environment-setup)
3. [Automated Testing](#automated-testing)
4. [Manual Testing](#manual-testing)
5. [Regression Testing](#regression-testing)
6. [Test Scenarios](#test-scenarios)
7. [Troubleshooting](#troubleshooting)
8. [Test Results Documentation](#test-results-documentation)

## Prerequisites

### System Requirements
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime for Windows
- Visual Studio 2022 or .NET 8.0 SDK (for building from source)
- PowerShell 5.1 or higher
- Elite Dangerous (for production testing)

### Development Tools
- Visual Studio 2022 (recommended) or Visual Studio Code
- .NET 8.0 SDK
- Git for version control
- PowerShell ISE or VS Code with PowerShell extension

## Test Environment Setup

### 1. Build the Application

Before testing, ensure the application is built:

`ash
# From the ED_Inara_Overlay_2.0 directory
dotnet build --configuration Debug
`

or

`ash
dotnet build --configuration Release
`

### 2. Verify Build Output

The built executable should be located at:
- Debug: in\Debug\net8.0-windows\ED_Inara_Overlay_2.0.exe
- Release: in\Release\net8.0-windows\ED_Inara_Overlay_2.0.exe

### 3. Testing Directory Structure

`
ED_Inara_Overlay_2.0/
├── ED_Inara_Overlay_2.0/          # Main application
│   ├── bin/Debug/net8.0-windows/  # Built executable
│   └── Documentation/             # Documentation (this file)
└── Testing/                       # Test harnesses and utilities
    ├── TestHarness.bat            # Batch test script
    ├── TestHarness.ps1            # PowerShell test script
    ├── MockTargetApp.cs           # Mock application for testing
    └── Various regression tests   # Automated test scripts
`

## Automated Testing

### Test Harness Overview

The application includes several automated test harnesses:

1. **TestHarness.bat** - Basic batch script for simple testing
2. **TestHarness.ps1** - Advanced PowerShell script with better error handling
3. **Various regression test scripts** - Automated regression testing

### Running the Main Test Harness

#### Option 1: PowerShell Test Harness (Recommended)

`powershell
# Navigate to the Testing directory
cd "D:\Projects\ED_Inara_Overlay_2.0\Testing"

# Run the PowerShell test harness
.\TestHarness.ps1
`

#### Option 2: Batch Test Harness

`cmd
cd "D:\Projects\ED_Inara_Overlay_2.0\Testing"
TestHarness.bat
`

### Test Harness Flow

The automated test harness performs the following sequence:

1. **Initialization Test**
   - Verifies application executable exists
   - Checks for required dependencies

2. **Target Not Found Test**
   - Starts overlay targeting "MockTargetApp"
   - Verifies waiting window appears
   - Tests "Looking for: MockTargetApp.exe" message

3. **Mock Target Launch Test**
   - Compiles and launches MockTargetApp
   - Verifies waiting window disappears
   - Confirms overlay appears over mock target

4. **Focus Change Test**
   - Launches Notepad
   - Tests overlay hiding when focus changes
   - Verifies overlay reappears when target regains focus

5. **Elite Dangerous Test**
   - Tests with actual Elite Dangerous process
   - Verifies production behavior

6. **Cleanup**
   - Terminates all test processes
   - Removes temporary files

## Manual Testing

### Basic Manual Test Procedure

1. **Start Without Target**
   `cmd
   ED_Inara_Overlay_2.0.exe NonExistentApp
   `
   - **Expected**: Waiting window appears
   - **Verify**: "Looking for: NonExistentApp.exe" message

2. **Start With Elite Dangerous**
   `cmd
   ED_Inara_Overlay_2.0.exe EliteDangerous64
   `
   - **Expected**: 
     - If ED not running: Waiting window appears
     - If ED running: Overlay appears immediately

3. **Focus Testing**
   - Start Elite Dangerous
   - Launch overlay
   - Alt+Tab to different applications
   - **Verify**: Overlay hides/shows based on ED focus

### User Interface Testing

#### Waiting Window
- **Location**: Should appear centered on screen
- **Content**: Clear message indicating target process
- **Behavior**: Should disappear when target found
- **Styling**: Consistent with application theme

#### Overlay Window
- **Positioning**: Should overlay target application
- **Transparency**: Should not obstruct gameplay
- **Content**: Trade route information should be readable
- **Responsiveness**: Should update when data changes

### Command Line Arguments Testing

Test various command line scenarios:

`cmd
# No arguments (should show help or default behavior)
ED_Inara_Overlay_2.0.exe

# Elite Dangerous (production use)
ED_Inara_Overlay_2.0.exe EliteDangerous64

# Custom target
ED_Inara_Overlay_2.0.exe notepad

# Case sensitivity
ED_Inara_Overlay_2.0.exe ELITEDANGEROUS64
`

## Regression Testing

### Automated Regression Tests

The Testing directory contains several regression test scripts:

- AutomatedRegressionTest.ps1 - Comprehensive automated testing
- QuickRegressionTest.ps1 - Fast smoke tests
- BasicRegressionTest.ps1 - Core functionality tests
- SimpleRegressionTest.ps1 - Minimal test set

### Running Regression Tests

`powershell
# Quick smoke test
.\QuickRegressionTest.ps1

# Full regression suite
.\AutomatedRegressionTest.ps1

# Run all tests
.\RunTests.ps1
`

### Critical Regression Test Areas

1. **Window Management**
   - Overlay positioning accuracy
   - Window focus detection
   - Multi-monitor support

2. **Process Detection**
   - Target process finding
   - Process lifecycle handling
   - Edge cases (process name changes, multiple instances)

3. **Data Handling**
   - API data retrieval
   - Data parsing and display
   - Error handling for network issues

4. **Performance**
   - Memory usage over time
   - CPU utilization
   - Startup/shutdown times

## Test Scenarios

### Scenario 1: First-Time User Experience

**Steps:**
1. Launch application without arguments
2. Launch with Elite Dangerous not running
3. Start Elite Dangerous
4. Verify overlay appears correctly

**Expected Results:**
- Clear instructions or help information
- Waiting window appears with clear messaging
- Smooth transition to overlay when target detected

### Scenario 2: Multi-Monitor Setup

**Steps:**
1. Configure multi-monitor setup
2. Run Elite Dangerous on secondary monitor
3. Launch overlay
4. Move Elite Dangerous between monitors

**Expected Results:**
- Overlay follows target window
- Positioning remains accurate across monitors
- No display artifacts or positioning errors

### Scenario 3: Performance Testing

**Steps:**
1. Run application for extended periods (2+ hours)
2. Monitor memory and CPU usage
3. Frequently switch focus between applications
4. Test with high system load

**Expected Results:**
- Memory usage remains stable
- CPU usage stays minimal when overlay hidden
- No performance degradation over time

### Scenario 4: Error Handling

**Steps:**
1. Disconnect network during API calls
2. Provide invalid command line arguments
3. Terminate target process unexpectedly
4. Simulate system resource exhaustion

**Expected Results:**
- Graceful error handling
- Clear error messages to user
- Application recovery when possible
- No crashes or data corruption

### Scenario 5: Edge Cases

**Steps:**
1. Launch multiple instances of target application
2. Rename target executable
3. Run as different user privileges
4. Test with antivirus software active

**Expected Results:**
- Handles multiple target instances appropriately
- Fails gracefully when target not found
- Respects system security boundaries
- No false positives from security software

## Troubleshooting

### Common Test Issues

#### Application Won't Start
- **Cause**: Missing .NET 8.0 runtime
- **Solution**: Install .NET 8.0 runtime for Windows
- **Verification**: dotnet --version should show 8.0.x

#### Overlay Doesn't Appear
- **Cause**: Target process not detected
- **Solution**: Verify exact process name with Task Manager
- **Verification**: Check Windows event logs for errors

#### Mock Target App Won't Compile
- **Cause**: Missing C# compiler
- **Solution**: Install .NET SDK or Visual Studio
- **Verification**: csc command should be available

#### Focus Detection Issues
- **Cause**: Windows focus APIs not responding
- **Solution**: Restart application and/or target process
- **Verification**: Test with simple applications like Notepad

### Debug Information

Enable debug logging by:
1. Setting environment variable: ED_OVERLAY_DEBUG=1
2. Check application logs in %TEMP%\ED_Inara_Overlay_2.0\
3. Use Windows Event Viewer for system-level issues

### Performance Profiling

For performance testing:
1. Use Windows Performance Monitor
2. Monitor these counters:
   - Process CPU usage
   - Process memory usage
   - Window handle count
   - GDI object count

## Test Results Documentation

### Test Report Template

`
Test Run: [Date/Time]
Version: [Application Version]
Environment: [OS Version, .NET Version]
Tester: [Name]

Test Results:
- Automated Tests: [Pass/Fail Count]
- Manual Tests: [Pass/Fail Count]
- Regression Tests: [Pass/Fail Count]

Critical Issues:
- [List any blocking issues]

Performance Notes:
- Memory Usage: [Peak/Average]
- CPU Usage: [Peak/Average]
- Startup Time: [Time in seconds]

Recommendations:
- [Any recommended fixes or improvements]
`

### Continuous Integration

For automated testing in CI/CD pipelines:

`yaml
# Example GitHub Actions workflow
name: Test ED Inara Overlay

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Build
      run: dotnet build
    - name: Run Tests
      run: |
        cd Testing
        .\QuickRegressionTest.ps1
`

## Conclusion

This testing guide provides comprehensive coverage for validating the ED Inara Overlay 2.0 application. Regular execution of these tests ensures:

- Functional correctness
- Performance stability
- User experience quality
- Regression prevention

For questions or issues with testing, please refer to the project documentation or contact the development team.

---

*Last Updated: [Current Date]*
*Version: 2.0*
*Author: Testing Team*
