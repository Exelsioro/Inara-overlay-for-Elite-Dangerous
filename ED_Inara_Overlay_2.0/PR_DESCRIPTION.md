# Pull Request: State-Machine Based Overlay Visibility Management

## Overview
This PR implements a robust state-machine based solution to fix the initial overlay visibility issue when launching Elite Dangerous. The problem occurred when the overlay application started before Elite Dangerous was fully initialized, causing the overlay to remain invisible.

## Problem Statement
- **Issue**: Overlay would remain invisible when Elite Dangerous took time to start
- **Root Cause**: Race condition between overlay startup and target process initialization
- **Impact**: Users would see a "waiting" window but the overlay would never appear even after the game loaded

## Solution Implementation

### State Machine Architecture
Implemented a three-state system for overlay visibility management:

1. **`Waiting`** - Initial state when no target is detected
2. **`ForceShow`** - Transition state that ensures visibility after target detection
3. **`Auto`** - Normal operational state with focus-based visibility

### Key Components

#### 1. Non-Blocking Retry Mechanism
```csharp
private DispatcherTimer? retryTimer;
private int retryAttempts = 0;
private const int maxRetryAttempts = 20; // 10 seconds with 500ms intervals
```

#### 2. Two-Level Detection System
- **Process Detection**: Find the target process by name
- **Window Detection**: Verify the main window handle exists

#### 3. State Transitions
- `Waiting` → `ForceShow`: When target is detected via retry mechanism
- `ForceShow` → `Auto`: After successful visibility and focus change
- Maintains focus-dependent visibility behavior in `Auto` state

### Technical Details

#### Enhanced MainWindow.xaml.cs
- Added retry timer infrastructure
- Implemented `FindTargetProcessWithRetry()` method
- Enhanced `UpdateTimer_Tick()` with state machine logic
- Added `EnsureVisibleAfterTargetDetection()` method
- Proper resource cleanup in `OnClosed()`

#### State Management
- Non-blocking UI thread operation
- Configurable retry intervals (500ms) and max attempts (20)
- Comprehensive logging for debugging
- Automatic cleanup when target application closes

## Test Coverage

### Comprehensive Test Harness
- **TestHarness.ps1**: Manual testing with mock applications
- **RegressionTest.ps1**: Comprehensive automated testing
- **QuickRegressionTest.ps1**: Fast regression testing
- **BasicRegressionTest.ps1**: Basic functionality testing
- **AutomatedRegressionTest.ps1**: Fully automated testing
- **SimpleRegressionTest.ps1**: Simple scenario testing

### Test Results
✅ **All Tests Passing**
1. Waiting window appears when no target is running
2. When mock target starts, overlay detects it within seconds via retry mechanism
3. Overlay appears correctly positioned relative to target
4. Focus behavior works as expected (hide/show based on focus)
5. Works with both mock applications and real Elite Dangerous
6. Proper cleanup when target application closes

## Files Modified

### Core Implementation
- `MainWindow.xaml.cs` - Main state machine implementation
- `App.xaml.cs` - Integration with application lifecycle

### Documentation
- `CHANGELOG.md` - **NEW** - Comprehensive changelog with technical details
- `README.md` - Enhanced with recent updates and test documentation
- `FIX_SUMMARY.md` - Detailed technical analysis

### Test Infrastructure
- `TestHarness.ps1` - Manual testing framework
- `TestHarness_v2.ps1` - Enhanced test harness
- `RegressionTest.ps1` - Comprehensive automated testing
- `QuickRegressionTest.ps1` - Fast regression testing
- `BasicRegressionTest.ps1` - Basic functionality testing
- `AutomatedRegressionTest.ps1` - Fully automated testing
- `SimpleRegressionTest.ps1` - Simple scenario testing
- `RunTests.ps1` - Test runner script

## Benefits

### Reliability
- **Robust Target Detection**: Handles slow-starting applications
- **Non-Blocking**: UI thread remains responsive during detection
- **Configurable**: Easy to adjust retry intervals and max attempts

### User Experience
- **Immediate Feedback**: Waiting window shows while detecting target
- **Seamless Transition**: Automatic visibility when target is found
- **Focus-Aware**: Maintains expected hide/show behavior

### Maintainability
- **Clean Architecture**: Well-defined state transitions
- **Comprehensive Logging**: Detailed debugging information
- **Proper Resource Management**: Automatic cleanup of timers and resources

## Configuration Options
- **Retry Interval**: 500ms (configurable)
- **Max Attempts**: 20 attempts = 10 seconds total (configurable)
- **Detection Sequence**: Process → Window Handle → Visibility Trigger

## Testing Instructions

### Quick Test
```powershell
# Run fast regression test
.\\QuickRegressionTest.ps1
```

### Comprehensive Test
```powershell
# Run all tests
.\\RunTests.ps1
```

### Manual Test
```powershell
# Run manual test harness
.\\TestHarness.ps1
```

## Link to Test Harness
The comprehensive test harness is available in the repository:
- Primary: `TestHarness.ps1`
- Enhanced: `TestHarness_v2.ps1`
- All test files are included in the repository root

## CHANGELOG Entry
Updated `CHANGELOG.md` with:
- **Fixed**: "Fixed initial overlay visibility when launching Elite Dangerous"
- Detailed technical implementation notes
- Complete test coverage documentation

## Backward Compatibility
- ✅ Maintains all existing functionality
- ✅ No breaking changes to public API
- ✅ Preserves existing focus behavior
- ✅ Compatible with all existing overlay windows

## Performance Impact
- **Minimal**: Uses efficient timer-based approach
- **Non-Blocking**: Doesn't impact UI responsiveness
- **Resource-Efficient**: Proper cleanup prevents memory leaks
- **Configurable**: Can be tuned for different scenarios

This implementation resolves the overlay visibility issue while maintaining all existing functionality and providing a robust, testable solution for reliable target detection.
