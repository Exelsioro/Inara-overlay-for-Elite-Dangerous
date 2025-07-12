# Corrected Visibility State Machine

## Overview
This document defines a deterministic state machine for managing overlay visibility with three distinct states that ensure predictable behavior for user interactions.

## States

### 1. WaitingVisible
- **Description**: Initial state where no target is detected and overlay is hidden
- **Behavior**: Overlay remains hidden, system waits for target detection

### 2. OverlayForceShow
- **Description**: Target detected, overlay is forced to show regardless of focus state until first manual focus event
- **Behavior**: Overlay is visible and remains visible until user manually focuses

### 3. OverlayAuto
- **Description**: Standard focus-based visibility rules apply
- **Behavior**: Overlay visibility follows normal focus/blur events

## State Diagram

```
┌─────────────────┐
│  WaitingVisible │ (Initial State)
│   (Hidden)      │
└─────────────────┘
          │
          │ target_detected
          ▼
┌─────────────────┐
│ OverlayForceShow│
│   (Visible)     │
└─────────────────┘
          │
          │ manual_focus_event
          ▼
┌─────────────────┐
│  OverlayAuto    │
│ (Focus-based)   │
└─────────────────┘
          │
          │ target_lost
          ▼
┌─────────────────┐
│  WaitingVisible │
│   (Hidden)      │
└─────────────────┘
```

## Guard Variables

### `_hasShownOnce: boolean`
- **Purpose**: Tracks whether the overlay has been shown at least once in the current session
- **Initial Value**: `false`
- **Usage**: Prevents returning to OverlayForceShow state once manual focus has occurred

### `_targetDetected: boolean`
- **Purpose**: Indicates whether a valid target is currently detected
- **Initial Value**: `false`
- **Usage**: Controls transitions between states based on target presence

### `_manualFocusOccurred: boolean`
- **Purpose**: Tracks if user has manually focused on the overlay
- **Initial Value**: `false`
- **Usage**: Triggers transition from OverlayForceShow to OverlayAuto

## State Transitions

### From WaitingVisible

| Event | Guard Condition | Next State | Actions |
|-------|----------------|------------|---------|
| `target_detected` | `!_hasShownOnce` | `OverlayForceShow` | Set `_targetDetected = true`, Show overlay |
| `target_detected` | `_hasShownOnce` | `OverlayAuto` | Set `_targetDetected = true`, Apply focus rules |

### From OverlayForceShow

| Event | Guard Condition | Next State | Actions |
|-------|----------------|------------|---------|
| `manual_focus_event` | `_targetDetected` | `OverlayAuto` | Set `_hasShownOnce = true`, `_manualFocusOccurred = true` |
| `target_lost` | Always | `WaitingVisible` | Set `_targetDetected = false`, Hide overlay |

### From OverlayAuto

| Event | Guard Condition | Next State | Actions |
|-------|----------------|------------|---------|
| `target_lost` | Always | `WaitingVisible` | Set `_targetDetected = false`, Hide overlay |
| `focus_gained` | `_targetDetected` | `OverlayAuto` | Show overlay |
| `focus_lost` | `_targetDetected` | `OverlayAuto` | Hide overlay |

## Entry/Exit Criteria

### WaitingVisible State
- **Entry Criteria**:
  - Initial state on system start
  - `target_lost` event from any other state
- **Exit Criteria**:
  - `target_detected` event occurs
- **Entry Actions**:
  - Hide overlay
  - Set `_targetDetected = false`
- **Exit Actions**:
  - None

### OverlayForceShow State
- **Entry Criteria**:
  - `target_detected` event AND `!_hasShownOnce`
- **Exit Criteria**:
  - `manual_focus_event` occurs
  - `target_lost` event occurs
- **Entry Actions**:
  - Show overlay
  - Set `_targetDetected = true`
- **Exit Actions**:
  - If exiting via `manual_focus_event`: Set `_hasShownOnce = true`

### OverlayAuto State
- **Entry Criteria**:
  - `manual_focus_event` from OverlayForceShow
  - `target_detected` event AND `_hasShownOnce = true`
- **Exit Criteria**:
  - `target_lost` event occurs
- **Entry Actions**:
  - Apply standard focus-based visibility rules
  - Set `_manualFocusOccurred = true`
- **Exit Actions**:
  - None

## Event Definitions

### `target_detected`
- **Trigger**: Valid target element is detected in the DOM
- **Data**: Target element reference

### `target_lost`
- **Trigger**: Previously detected target is no longer valid/present
- **Data**: None

### `manual_focus_event`
- **Trigger**: User explicitly focuses on the overlay (click, tab, etc.)
- **Data**: Focus event details

### `focus_gained`
- **Trigger**: Overlay receives focus (only relevant in OverlayAuto state)
- **Data**: Focus event details

### `focus_lost`
- **Trigger**: Overlay loses focus (only relevant in OverlayAuto state)
- **Data**: Blur event details

## State Invariants

1. **Deterministic Transitions**: Each state transition is deterministic based on current state, event, and guard conditions
2. **Single Active State**: Only one state can be active at any time
3. **Guard Consistency**: Guard variables maintain consistent state throughout transitions
4. **Event Ordering**: Events are processed in the order they occur
5. **State Isolation**: Each state has clearly defined responsibilities and behaviors

## Implementation Notes

- The state machine should be implemented as a finite state automaton
- Guard variables should be reset appropriately when the system reinitializes
- State transitions should be atomic to prevent race conditions
- Event handling should be queued to ensure proper processing order
- Logging should be implemented for debugging state transitions

## Reset Conditions

The state machine should reset to `WaitingVisible` with all guard variables cleared when:
- Page/application reload occurs
- User explicitly resets the overlay system
- Critical error in state machine execution
