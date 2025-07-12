# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- Fixed initial overlay visibility when launching Elite Dangerous. Implemented a robust state-machine based retry mechanism to handle timing issues during target application startup, ensuring the overlay appears correctly even when the target process takes time to initialize its main window.

## [1.0.0] - 2024-07-12

### Added
- Initial release of Elite Dangerous Inara Overlay 2.0
- WPF-based overlay system with automatic target detection
- State-machine based visibility management (Waiting → ForceShow → Auto)
- Timer-based retry mechanism for target process detection
- Comprehensive test harness for regression testing
- Trade route overlay functionality
- Results overlay window
- Pinned route overlay
- Proper window positioning relative to target application
- Focus-based visibility management
- Automatic cleanup when target application closes

### Technical Details
- **State Machine**: Implemented three-state system for overlay visibility management
  - `Waiting`: Initial state when no target is detected
  - `ForceShow`: Transition state that ensures visibility after target detection
  - `Auto`: Normal operational state with focus-based visibility
- **Retry Mechanism**: Non-blocking timer-based system with configurable retry count (20 attempts over 10 seconds)
- **Process Detection**: Two-level detection system (process existence + main window handle)
- **Resource Management**: Proper cleanup of timers and windows on application exit

### Test Coverage
- Automated regression test suite
- Basic, Quick, and Simple regression tests
- Test harness for manual verification
- Coverage for both mock applications and real Elite Dangerous
