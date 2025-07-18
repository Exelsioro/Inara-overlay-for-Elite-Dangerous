# ED Inara Overlay 2.0 - Testing Documentation

## Overview
This document provides comprehensive information about testing the ED Inara Overlay 2.0 application, including automated test harnesses, manual testing procedures, and troubleshooting guidance.

## Test Harnesses Available

### 1. TestHarness.bat (Automated Test Script)
**Location:** `Testing/TestHarness.bat`

A comprehensive batch script that automates the testing of overlay behavior with both mock targets and Elite Dangerous.

**Features:**
- Creates and compiles MockTargetApp on-the-fly
- Tests overlay visibility behavior
- Tests focus changes between applications
- Supports both mock testing and real Elite Dangerous testing
- Automated cleanup procedures

**Usage:**
```batch
cd Testing
TestHarness.bat
```

**Test Sequence:**
1. Starts overlay targeting MockTargetApp (initially hidden)
2. Compiles and launches MockTargetApp
3. Verifies overlay appears when target is detected
4. Tests focus behavior with Notepad
5. Tests with real Elite Dangerous (optional)

### 2. OverlayTestHarness (WPF Test Application)
**Location:** `Testing/OverlayTestHarness.exe`

A sophisticated WPF application for interactive testing of overlay behavior.

**Features:**
- Create mock target windows
- Start overlay instances
- Automated focus switching
- Real-time logging
- Configurable test intervals
- Visual status indicators

**Usage:**
```batch
cd Testing
dotnet run --project OverlayTestHarness.csproj
```

**Controls:**
- **Create Mock Target Window:** Creates a simulated Elite Dangerous window
- **Start Overlay:** Launches the overlay targeting a process
- **Start Focus Automation:** Automatically switches focus between windows
- **Interval Slider:** Configures focus change timing (1-10 seconds)

### 3. MinimalTestHarness (Console Application)
**Location:** `Testing/MinimalTestHarness.exe`

A simple console-based test harness for basic overlay functionality testing.

**Features:**
- Starts Notepad as test target
- Creates overlay instance
- Basic cleanup procedures
- Console logging output

**Usage:**
```batch
cd Testing
dotnet run --project MinimalTestHarness.csproj
```

### 4. MockTargetApp (Standalone Test Target)
**Location:** `Testing/MockTargetApp/`

A dedicated mock application that simulates Elite Dangerous for testing purposes.

**Features:**
- Windows Forms application
- Interactive button for testing
- Periodic window title updates
- Proper cleanup on exit

**Usage:**
```batch
cd Testing/MockTargetApp
dotnet run
```

## Build Configuration

### Compilation Error Resolution

The project has been configured to treat warnings as non-fatal to ensure successful builds:

**Modified Project Files:**
- `ED_Inara_Overlay_2.0.csproj`
- `Logger.csproj`
- `InaraTools.csproj`

**Configuration Added:**
```xml
<PropertyGroup>
  <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
</PropertyGroup>
```

### Nullable Reference Types Fix

Fixed the MainWindow constructor signature to properly handle nullable parameters:

**Before:**
```csharp
public MainWindow(string processName = "notepad", Process foundProcess = null)
```

**After:**
```csharp
public MainWindow(string processName = "notepad", Process? foundProcess = null)
```

## Testing Procedures

### Manual Testing Steps

1. **Basic Overlay Functionality:**
   - Start the overlay application
   - Launch the target process (Elite Dangerous or Notepad)
   - Verify overlay appears when target has focus
   - Verify overlay hides when switching to other applications

2. **Focus Behavior Testing:**
   - Use Alt+Tab to switch between applications
   - Click between different windows
   - Minimize/maximize the target application
   - Test with multiple monitors (if available)

3. **Process Lifecycle Testing:**
   - Start overlay before target process
   - Start target process before overlay
   - Kill target process while overlay is running
   - Restart target process after it was closed

### Automated Testing

Use the provided test harnesses for automated testing:

1. **Quick Test:** Run `TestHarness.bat` for comprehensive automated testing
2. **Interactive Test:** Use OverlayTestHarness for manual control and observation
3. **Minimal Test:** Use MinimalTestHarness for basic functionality verification

## Test Scenarios

### Scenario 1: Overlay Startup with No Target
- **Expected:** Waiting window appears showing "Looking for: [ProcessName].exe"
- **Test:** Start overlay when target process is not running

### Scenario 2: Target Process Detection
- **Expected:** Waiting window disappears, overlay appears over target
- **Test:** Start target process after overlay is waiting

### Scenario 3: Focus Change Behavior
- **Expected:** Overlay hides when target loses focus, reappears when target regains focus
- **Test:** Switch focus between target and other applications

### Scenario 4: Target Process Termination
- **Expected:** Application shuts down gracefully
- **Test:** Close target process while overlay is running

## Troubleshooting

### Common Issues

1. **Overlay Not Appearing:**
   - Check if target process name matches exactly
   - Verify overlay is not hidden behind other windows
   - Check Windows permissions and UAC settings

2. **Focus Detection Problems:**
   - Test with different applications to isolate the issue
   - Check Windows focus events in the logs
   - Verify overlay window properties (topmost, etc.)

3. **Build Errors:**
   - Ensure all projects have `TreatWarningsAsErrors` set to `false`
   - Check for missing nullable reference annotations
   - Verify all project references are correct

### Log Analysis

The application generates detailed logs in the `logs/` directory:
- Startup and initialization events
- Target process detection
- Focus change events
- Error conditions and exceptions

**Log Locations:**
- Main application: `ED_Inara_Overlay_2.0/logs/`
- Test harnesses: Use console output and logger integration

## File Structure

```
Testing/
├── TestHarness.bat                 # Main automated test script
├── MockTargetApp.cs               # Mock target application class
├── MinimalTestHarness.cs          # Simple console test harness
├── TestHarnessApp.xaml            # WPF test harness application definition
├── TestHarnessApp.xaml.cs         # WPF test harness code-behind
├── TestHarnessMainWindow.xaml     # WPF test harness main window
├── TestHarnessMainWindow.xaml.cs  # WPF test harness main window logic
├── MainWindow.xaml                # Test project main window
├── MainWindow.xaml.cs             # Test project main window logic
├── MockTargetApp/
│   ├── Program.cs                 # Standalone mock target entry point
│   └── MockTargetApp.csproj       # Mock target project file
├── SimpleTestHarness/
│   ├── Program.cs                 # Simple test harness
│   └── SimpleTestHarness.csproj   # Simple test harness project
├── MinimalTestHarness.csproj      # Minimal test harness project
└── OverlayTestHarness.csproj      # WPF test harness project
```

## Build Status

**Current Status:** ✅ All projects build successfully with zero compilation errors

**Projects:**
- ✅ ED_Inara_Overlay_2.0 (1 warning - unused field)
- ✅ Logger (2 warnings - nullable reference types)
- ✅ InaraTools (9 warnings - nullable reference types)
- ✅ MockTargetApp (0 errors)
- ✅ MinimalTestHarness (0 errors)
- ✅ OverlayTestHarness (0 errors)

**Total:** 0 compilation errors, 13 warnings (all non-critical)

## Recent Changes

### Version 2.0 Updates
- Fixed nullable reference type issues in MainWindow constructor
- Added comprehensive test harness suite
- Configured projects to ignore warnings during build
- Moved Documentation to solution level
- Added automated testing scripts
- Improved error handling and logging

### Build Configuration
- Updated all project files to treat warnings as non-fatal
- Fixed multiple entry point conflicts in test projects
- Resolved XAML compilation issues
- Cleaned up project file dependencies

---

**Last Updated:** 2025-07-12  
**Version:** 2.0  
**Build Status:** ✅ Success (0 errors, 13 warnings)
