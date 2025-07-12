# TradeRouteWindow Positioning Logic

## Positioning Strategy: Right Center of Target Window

The TradeRouteWindow is positioned using custom calculated coordinates to place it at the **right center** of the target window.

### Calculation Method:

```
Target Window Dimensions:
- targetWidth = targetRect.Right - targetRect.Left
- targetHeight = targetRect.Bottom - targetRect.Top
- targetCenterY = targetRect.Top + (targetHeight / 2)

TradeRouteWindow Position:
- Left = targetRect.Right (right edge of target)
- Top = targetCenterY - (TradeRouteWindow.Height / 2) (vertically centered)
```

### Visual Representation:

```
┌─────────────────────────┐
│                         │
│     Target Window       │
│                         │ ┌──────────────────┐
│         Center ────────────► TradeRouteWindow │
│                         │ │                  │
│                         │ │                  │
└─────────────────────────┘ │                  │
                            │                  │
                            └──────────────────┘
```

### Fallback Positioning:

1. **Right Side Doesn't Fit**: Position to left of target window
   ```
   ┌──────────────────┐ ┌─────────────────────────┐
   │ TradeRouteWindow │ │                         │
   │                  │ │     Target Window       │
   │                  │ │                         │
   │                  ◄────── Center              │
   │                  │ │                         │
   └──────────────────┘ └─────────────────────────┘
   ```

2. **Left Side Also Doesn't Fit**: Position at screen edge (10px from left)

3. **Vertical Overflow**: Adjust top position to keep within screen bounds

### Screen Bounds Checking:

- **Horizontal**: Ensures window doesn't go off-screen left or right
- **Vertical**: Ensures window doesn't go off-screen top or bottom
- **Margins**: Maintains 10px minimum distance from screen edges

### Implementation Details:

- **Real-time Updates**: Position recalculated every frame (60 FPS) to follow target window movement
- **Error Handling**: Graceful fallback if calculations fail
- **Comprehensive Logging**: Detailed position information for debugging
- **Screen-aware**: Uses `SystemParameters.WorkArea` for accurate screen dimensions

This approach ensures the TradeRouteWindow is always optimally positioned relative to the target window while remaining fully visible on screen.
