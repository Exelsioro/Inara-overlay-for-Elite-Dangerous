# Waiting Window Feature - ED Inara Overlay 2.0

## Overview

The ED Inara Overlay 2.0 now includes a **Waiting Window** feature that provides a user-friendly interface while the target application (Elite Dangerous) is not yet running. This enhances the user experience by providing clear feedback and instructions instead of silently waiting in the background.

## Features

### 1. **Smart Startup Flow**
- **Target Detection**: Checks if target application is already running at startup
- **Conditional Display**: Shows waiting window only when target application is not found
- **Direct Launch**: If target is already running, goes straight to overlay mode
- **Seamless Transition**: Automatically switches to overlay when target is detected

### 2. **User-Friendly Interface**
- **Elite Dangerous Theme**: Dark UI with blue accents matching the game aesthetic
- **Animated Loading Icon**: Rotating spinner to indicate active monitoring
- **Dynamic Status**: Text updates with dots animation to show activity
- **Clear Instructions**: Step-by-step guidance for the user
- **Target Information**: Shows which application the overlay is waiting for

### 3. **Flexible Process Support**
- **Multiple Targets**: Supports various target applications
- **Smart Display Names**: Shows user-friendly names (e.g., "Elite Dangerous" instead of "EliteDangerous64")
- **Process Name Detection**: Automatically detects and displays process information
- **Command Line Configuration**: Target can be specified via command line arguments

## Implementation Details

### **New Files Added**

#### `WaitingWindow.xaml`
- Modern WPF interface with Elite Dangerous styling
- Animated loading indicator with smooth rotation
- Responsive layout with proper spacing
- User-friendly instructions and exit button

#### `WaitingWindow.xaml.cs`
- Process monitoring logic (checks every 1 second)
- Event-driven architecture for target detection
- Proper cleanup and application shutdown handling
- Smart display name mapping for common processes

#### `App.xaml.cs` (Modified)
- Enhanced startup flow with conditional logic
- Target process detection at application start
- Event handling for waiting window to main overlay transition
- Improved error handling and logging

### **Startup Flow Logic**

```csharp
Application Start
    ↓
Check if target process is running
    ↓
┌─ YES → Start Main Overlay directly
└─ NO  → Show Waiting Window
    ↓
Monitor for target process (every 1 second)
    ↓
Target process detected → Close Waiting Window → Start Main Overlay
```

### **Key Methods**

#### `App.OnStartup()`
```csharp
// Checks for existing target process
var existingProcess = WindowsAPI.FindProcessByName(targetProcessName);

if (existingProcess != null)
    StartMainOverlay();
else
    ShowWaitingWindow();
```

#### `WaitingWindow.CheckTimer_Tick()`
```csharp
// Monitors for target process
var process = WindowsAPI.FindProcessByName(targetProcessName);
if (process != null)
{
    TargetProcessFound?.Invoke(this, targetProcessName);
    this.Close();
}
```

#### `App.OnTargetProcessFound()`
```csharp
// Handles transition from waiting to main overlay
waitingWindow.CloseWaitingWindow();
this.Dispatcher.BeginInvoke(() => StartMainOverlay());
```

## User Experience Flow

### **Scenario 1: Target Already Running**
1. User launches overlay
2. System detects Elite Dangerous is already running
3. Main overlay window appears immediately
4. No waiting window is shown

### **Scenario 2: Target Not Running**
1. User launches overlay
2. System shows waiting window with instructions
3. User launches Elite Dangerous
4. Waiting window detects the process within 1 second
5. Waiting window closes automatically
6. Main overlay window appears and attaches to Elite Dangerous

### **Scenario 3: User Cancellation**
1. Waiting window is displayed
2. User clicks "Exit Overlay" button OR closes window via X
3. Entire application shuts down cleanly
4. No orphaned processes remain

## Configuration

### **Target Process Support**

| Process Name | Display Name | Usage |
|--------------|--------------|--------|
| `notepad` | Notepad | Development/Testing |
| `elitedangerous64` | Elite Dangerous | Production (64-bit) |
| `elitedangerous32` | Elite Dangerous | Production (32-bit) |
| `steam` | Steam | Alternative launch method |

### **Command Line Usage**

```bash
# Default (notepad for testing)
ED_Inara_Overlay_2.0.exe

# Elite Dangerous 64-bit
ED_Inara_Overlay_2.0.exe EliteDangerous64

# Elite Dangerous 32-bit  
ED_Inara_Overlay_2.0.exe EliteDangerous32

# Custom process
ED_Inara_Overlay_2.0.exe MyCustomApp
```

## Visual Design

### **Color Scheme**
- **Background**: Dark gray (#1A1A1A) - matches Elite Dangerous UI
- **Accent**: Blue (#00B4FF) - primary highlight color
- **Success**: Green (#00FF00) - for target application name
- **Warning**: Orange (#FFB300) - for loading spinner
- **Danger**: Red (#FF4444) - for exit button

### **Layout Elements**
- **Header**: Application title with blue accent
- **Status Area**: Animated loading icon and status text
- **Instructions**: Clear step-by-step guidance in bordered box
- **Progress Info**: Shows monitoring frequency
- **Exit Button**: Prominent red button for application termination

### **Animation**
- **Loading Spinner**: 2-second rotation cycle
- **Status Text**: Animated dots (...) showing monitoring activity
- **Button Hover**: Color transitions for interactive feedback

## Benefits

### **For Users**
- **Clear Feedback**: No confusion about application state
- **Easy Exit**: Simple way to cancel if target won't be launched
- **Professional Appearance**: Polished UI matching Elite Dangerous aesthetic
- **Helpful Instructions**: Clear guidance on what to do next

### **For Developers**
- **Better UX**: Much more user-friendly than silent background waiting
- **Error Prevention**: Reduces user confusion and support requests
- **Flexible Architecture**: Easy to add support for new target applications
- **Clean Shutdown**: Proper application lifecycle management

## Testing

### **Test Scenarios**

#### **Test 1: Normal Waiting Flow**
1. Ensure Elite Dangerous is NOT running
2. Launch overlay application
3. ✅ Waiting window should appear
4. Launch Elite Dangerous
5. ✅ Waiting window should close within 1-2 seconds
6. ✅ Main overlay should appear and attach to Elite Dangerous

#### **Test 2: Target Already Running**
1. Launch Elite Dangerous first
2. Launch overlay application
3. ✅ Main overlay should appear immediately
4. ✅ No waiting window should be shown

#### **Test 3: User Cancellation**
1. Ensure Elite Dangerous is NOT running
2. Launch overlay application
3. ✅ Waiting window should appear
4. Click "Exit Overlay" button
5. ✅ Application should shut down completely
6. ✅ No processes should remain in Task Manager

#### **Test 4: Window Close (X button)**
1. Ensure Elite Dangerous is NOT running
2. Launch overlay application
3. ✅ Waiting window should appear
4. Click X button to close window
5. ✅ Application should shut down completely

### **Test Commands**

```bash
# Test with notepad (easier for development)
cd "D:\Projects\ED_Inara_Overlay_2.0\ED_Inara_Overlay_2.0\bin\Debug\net8.0-windows"

# Test 1: Launch overlay first, then notepad
./ED_Inara_Overlay_2.0.exe notepad
# Then launch: notepad.exe

# Test 2: Launch notepad first, then overlay
notepad.exe
./ED_Inara_Overlay_2.0.exe notepad

# Test 3: Test Elite Dangerous
./ED_Inara_Overlay_2.0.exe EliteDangerous64
```

## Logging

The waiting window feature includes comprehensive logging:

```
[INFO] Application starting with target process: EliteDangerous64
[INFO] Target process EliteDangerous64 not running - showing waiting window
[INFO] Creating and showing WaitingWindow
[INFO] WaitingWindow initialized for target process: EliteDangerous64
[INFO] WaitingWindow UI configured for: Elite Dangerous
[INFO] Started monitoring for target process: EliteDangerous64
[INFO] WaitingWindow displayed
[INFO] Target process found: EliteDangerous64 (PID: 12345)
[INFO] Target process found event received: EliteDangerous64
[INFO] Starting main overlay for target process: EliteDangerous64
[INFO] Main overlay window created and displayed
```

## Troubleshooting

### **Issue: Waiting window doesn't detect target**
1. Verify target process name is correct
2. Check Task Manager for actual process name
3. Try launching target with administrator privileges
4. Check logs for error messages

### **Issue: Application doesn't shut down properly**
1. Check Task Manager for orphaned processes
2. Verify all event handlers are properly unsubscribed
3. Review logs for shutdown sequence
4. Ensure proper cleanup in OnClosed methods

### **Issue: Waiting window shows but target is running**
1. Check process name spelling and capitalization
2. Verify target process has proper window title
3. Check if target is running as different user
4. Review WindowsAPI.FindProcessByName logic

---

## Recent Updates

### **Fix: MainWindow Visibility After Target Detection**

**Issue**: MainWindow wasn't appearing after waiting window closed because it only showed when target had focus.

**Solution**: 
- Added `forceVisible` flag to ensure MainWindow appears after target detection
- Modified visibility logic to be less strict about focus requirements
- Added `EnsureVisibleAfterTargetDetection()` method called during transition
- Improved child window hiding logic to only hide when target is minimized/not visible

**Changes Made**:
1. **MainWindow.xaml.cs**: Added forceVisible flag and enhanced visibility logic
2. **App.xaml.cs**: Call EnsureVisibleAfterTargetDetection() when creating MainWindow
3. **Visibility Logic**: MainWindow now shows if target is visible AND (forceVisible OR targetHasFocus OR overlayHasFocus)

## Implementation Status

✅ **COMPLETED** - Waiting window feature fully implemented  
✅ **USER-FRIENDLY** - Professional UI with clear instructions  
✅ **ROBUST** - Proper error handling and cleanup  
✅ **FLEXIBLE** - Supports multiple target applications  
✅ **TESTED** - Works with both notepad (dev) and Elite Dangerous (prod)  
✅ **FIXED** - MainWindow now reliably appears after target detection

**Ready for production use - provides excellent user experience for overlay startup.**
