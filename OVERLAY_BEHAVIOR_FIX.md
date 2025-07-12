# Overlay Behavior Fix - ED Inara Overlay 2.0

## Issue Description

**Problem**: The MainWindow was appearing as a permanent overlay above every window instead of properly attaching to and following the target application.

**Symptoms**:
- MainWindow appeared over all applications, not just the target
- Overlay stayed visible even when switching to other applications
- Didn't follow proper overlay behavior (focus-based visibility)

## Root Cause Analysis

### **Permanent `forceVisible` Flag**
The `forceVisible` flag was set to `true` during target detection but never reset, causing the overlay to remain permanently visible regardless of target window focus state.

### **Logic Flow Issue**
```csharp
// BEFORE (Broken):
bool shouldBeVisible = targetVisible && !targetMinimized && 
                     (forceVisible || targetHasFocus || overlayHasFocus);
// forceVisible stayed true forever → always visible
```

## Solution Implemented

### **1. Temporary `forceVisible` Flag**

Made the `forceVisible` flag temporary - used only for initial display, then reset:

```csharp
// Reset forceVisible after first successful show
if (forceVisible && targetVisible && !targetMinimized)
{
    shouldBeVisible = true;
    Logger.Logger.Info("Using forceVisible flag for initial display");
}

if (shouldBeVisible && !this.IsVisible)
{
    this.Show();
    
    // Reset forceVisible flag after successful show
    if (forceVisible)
    {
        forceVisible = false;
        Logger.Logger.Info("Resetting forceVisible flag after successful show");
    }
}
```

### **2. Proper Focus-Based Visibility**

After the initial show, the overlay follows normal focus rules:

```csharp
// Should be visible if target has focus OR any overlay window has focus
bool shouldBeVisible = targetVisible && !targetMinimized && 
                     (targetHasFocus || overlayHasFocus);
```

### **3. Immediate Hiding on Focus Loss**

Simplified hiding logic to respond immediately to focus changes:

```csharp
else if (!shouldBeVisible && this.IsVisible)
{
    Logger.Logger.Info("MainWindow hiding - target focus lost");
    this.Hide();
    // Hide child windows too
}
```

## Expected Behavior Now

### **Initial Display Sequence**
1. Target detected → `forceVisible = true`
2. MainWindow shows immediately (using `forceVisible`)
3. `forceVisible` reset to `false` after successful show
4. Normal focus-based behavior takes over

### **Normal Operation**
- **Target App Focused**: Overlay visible ✅
- **Overlay Window Focused**: Overlay remains visible ✅
- **Other App Focused**: Overlay hidden ✅
- **Target Minimized**: Overlay hidden ✅

### **Use Cases**

#### **Scenario 1: Working with Target Application**
1. Focus target app → Overlay appears
2. Click on overlay button → Overlay stays visible (overlay has focus)
3. Click back on target app → Overlay stays visible (target has focus)
4. Switch to browser → Overlay hides ✅

#### **Scenario 2: Multitasking**
1. Working in target app with overlay visible
2. Alt-Tab to another application → Overlay hides
3. Alt-Tab back to target app → Overlay reappears
4. Perfect non-intrusive behavior ✅

#### **Scenario 3: Initial Startup**
1. Target not running → Waiting window appears
2. Start target app → Waiting window closes
3. MainWindow appears immediately (using `forceVisible`)
4. Switch focus away → MainWindow hides (normal behavior starts)

## Technical Changes

### **Files Modified**

**`MainWindow.xaml.cs`**:
- Enhanced visibility logic in `UpdateTimer_Tick()`
- Added temporary `forceVisible` flag handling
- Improved logging for debugging visibility behavior

### **Key Code Changes**

#### **Before (Broken)**
```csharp
bool shouldBeVisible = targetVisible && !targetMinimized && 
                     (forceVisible || targetHasFocus || overlayHasFocus);
// forceVisible never reset → always visible
```

#### **After (Fixed)**
```csharp
bool shouldBeVisible = targetVisible && !targetMinimized && 
                     (targetHasFocus || overlayHasFocus);

// Use forceVisible only for initial transition
if (forceVisible && targetVisible && !targetMinimized)
{
    shouldBeVisible = true;
}

// Reset forceVisible after successful show
if (shouldBeVisible && !this.IsVisible)
{
    this.Show();
    if (forceVisible)
    {
        forceVisible = false; // Reset flag
    }
}
```

## Logging Added

Enhanced logging for debugging overlay behavior:

```
[INFO] Using forceVisible flag for initial display
[INFO] MainWindow showing - target visible: True, targetFocus: False, overlayFocus: False, forced: True
[INFO] Resetting forceVisible flag after successful show
[INFO] MainWindow hiding - target focus lost: targetFocus: False, overlayFocus: False
```

## Benefits

### **For Users**
- **Non-Intrusive**: Overlay only appears when working with target application
- **Predictable**: Clear focus-based visibility rules
- **Professional**: No more overlay stuck above all windows
- **Seamless**: Smooth transitions between applications

### **For Developers**
- **Clear State Management**: `forceVisible` flag has defined lifecycle
- **Debuggable**: Comprehensive logging of visibility state changes
- **Maintainable**: Simple, clear visibility logic
- **Reliable**: Consistent behavior across different scenarios

## Testing Scenarios

### **Test 1: Initial Display** ✅
1. Launch overlay (target not running)
2. Start target app
3. MainWindow appears immediately
4. Switch to another app → MainWindow hides

### **Test 2: Focus-Based Behavior** ✅
1. Target app focused → Overlay visible
2. Click overlay button → Overlay stays visible
3. Switch to other app → Overlay hides
4. Return to target app → Overlay reappears

### **Test 3: Window Management** ✅
1. Target app minimized → Overlay hides
2. Target app restored → Overlay appears (if has focus)
3. Target app closed → Application shuts down

## Implementation Status

✅ **COMPLETED** - Overlay behavior fully fixed  
✅ **FOCUS-BASED** - Proper visibility rules implemented  
✅ **NON-INTRUSIVE** - No more permanent overlay above all windows  
✅ **TESTED** - All visibility scenarios working correctly

**The overlay now behaves like a proper game overlay - visible only when the target application has focus.**
