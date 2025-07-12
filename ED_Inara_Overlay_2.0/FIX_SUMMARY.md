# Target Process Detection Timing Fix

## Problem
The original issue was that `WindowsAPI.FindProcessByName(processName)` in the MainWindow constructor couldn't find the target window because there was insufficient time for the target application to start and initialize its main window.

## Root Cause
When testing the overlay behavior with a mock target application, there's a race condition:
1. Overlay starts and immediately tries to find the target process
2. Target process might not have fully started yet
3. Even if the process exists, its main window might not be created yet
4. This caused the overlay to never detect the target and remain invisible

## Solution Implemented

### 1. Non-blocking Retry Mechanism
- **Old approach**: Single synchronous check in constructor
- **New approach**: Timer-based retry system that doesn't block the UI thread

### 2. Two-level Detection
- **Process Detection**: First try to find the process by name
- **Window Detection**: Then try to find the main window handle for that process

### 3. Implementation Details

#### Added Fields:
```csharp
private DispatcherTimer? retryTimer;
private int retryAttempts = 0;
private string retryProcessName = "";
private const int maxRetryAttempts = 20; // 10 seconds with 500ms intervals
```

#### Key Methods:
- `FindTargetProcessWithRetry()`: Initial attempt + setup of retry timer
- `SetupRetryTimer()`: Configures the retry timer
- `RetryTimer_Tick()`: Handles each retry attempt
- Enhanced cleanup in `OnClosed()`: Stops retry timer

### 4. State Management
- When target is found via retry mechanism, automatically calls `EnsureVisibleAfterTargetDetection()`
- Properly transitions state from `Waiting` → `ForceShow` → `Auto`
- Maintains proper focus-dependent visibility behavior

### 5. Benefits
- **Non-blocking**: UI thread remains responsive during target detection
- **Robust**: Handles slow-starting applications and initialization delays
- **Configurable**: Easy to adjust retry intervals and max attempts
- **Clean**: Proper resource cleanup when window closes
- **Logging**: Comprehensive logging for debugging

## Test Results
With the test harness:
1. ✅ Waiting window appears when no target is running
2. ✅ When mock target starts, overlay detects it within seconds via retry mechanism
3. ✅ Overlay appears correctly positioned relative to target
4. ✅ Focus behavior works as expected (hide/show based on focus)
5. ✅ Works with both mock applications and real Elite Dangerous

## Configuration
- **Retry interval**: 500ms (configurable)
- **Max attempts**: 20 (= 10 seconds total, configurable)
- **Detection sequence**: Process → Window handle → Visibility trigger

This fix ensures reliable target detection even with timing-sensitive application startup scenarios while maintaining responsive UI behavior.
