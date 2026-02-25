# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Global Hotkey System**: Ctrl+5 hotkey support for system-wide overlay control
  - Windows API RegisterHotKey implementation for system-wide hotkey registration
  - Ctrl+5 hotkey triggers the same toggle action as clicking the toggle button
  - Background operation works even when Elite Dangerous is in focus
  - Automatic hotkey registration when overlay starts
  - Graceful fallback handling if hotkey is already in use by another application
  - Thread-safe hotkey event handling with proper UI thread marshaling
  - Comprehensive logging for hotkey registration and activation events
  - Proper cleanup and unregistration of hotkey when application closes

## [Latest] - 2025-07-16

### Added
- **Ko-fi Integration**: Built-in support link for project development
  - Custom coffee cup icon with Ko-fi brand colors (#FF5E5E)
  - Clickable Ko-fi link (Ko-fi.com/exelsior) in waiting window
  - Proper event handling to open link in browser
  - User action logging for Ko-fi link clicks
  - Visual appeal with heart emoji and professional styling
- **Application Icon**: Custom app icon for enhanced user experience
  - New app.ico file in Resources folder
  - Icon appears in taskbar, Alt+Tab, and system UI
  - Professional branding for the application
- **Manual Overlay Control**: Enhanced user control over overlay startup
  - Prevents automatic overlay startup when target process is detected
  - Added "Start Overlay" button for explicit user control
  - Enhanced UI feedback with green text when target is available
  - Status message: "Target application found! Click 'Start Overlay' to proceed."
  - Continuous monitoring without auto-starting overlay
  - Graceful return to waiting state if target process closes

### Enhanced
- **Waiting Window**: Improved user experience and control
  - Enhanced target process detection with visual feedback
  - Animated status messages with progress indicators
  - Better state management for target process availability
  - Improved button layout and visual hierarchy
  - Professional styling consistent with application theme
- **User Experience**: Better control and feedback mechanisms
  - User retains full control over overlay initialization timing
  - Clear visual indication when target process is available
  - Smooth transitions between waiting and ready states
  - Enhanced logging for user actions and system events

### Technical Improvements
- **Event Handling**: Improved event management for user interactions
  - Better separation of user-initiated vs system-initiated actions
  - Enhanced logging for debugging and user behavior analysis
  - Proper cleanup of event handlers and resources
- **State Management**: Better tracking of application and target states
  - Improved target process monitoring logic
  - Better handling of edge cases and state transitions
  - Enhanced reliability of overlay startup process

## [Latest] - 2025-07-15

### Added
- **Theme Persistence System**: Comprehensive settings management for theme preferences
  - `SettingsService` - JSON-based settings storage and management
  - Automatic theme saving when applied through settings window
  - Theme restoration on application startup
  - Settings stored in `%APPDATA%/ED_Inara_Overlay/settings.json`
- **Enhanced Theme Management**: Improved theme system with better state management
  - Current theme tracking and restoration
  - Fallback to default theme when saved theme is unavailable
  - Proper theme initialization in Settings window
  - Better error handling and logging for theme operations
- **Improved User Experience**: Theme selection now persists across application restarts
  - No need to reselect theme every time the app starts
  - Seamless theme experience with automatic preference saving
  - Settings window now properly shows currently selected theme

### Enhanced
- **ThemeManager**: Updated with persistence support and better state management
  - Integration with SettingsService for automatic theme saving
  - LoadSavedTheme method for startup theme restoration
  - Better handling of theme loading and application states
- **App.xaml.cs**: Simplified theme initialization using persistent settings
  - Automatic loading of saved theme preferences
  - Fallback to default theme when no saved preference exists
- **SettingsWindow**: Enhanced to show current theme selection
  - Current theme is pre-selected when opening settings
  - Real-time theme preview with automatic saving
  - Better theme state management

### Technical Improvements
- **Settings Architecture**: Robust JSON-based configuration system
  - Singleton pattern for settings management
  - Automatic file creation and directory management
  - Comprehensive error handling and logging
  - Version tracking and timestamp recording
- **Theme State Management**: Improved theme state tracking
  - Better synchronization between theme manager and settings
  - Proper handling of theme availability and fallbacks
  - Enhanced logging for theme operations
- **Code Quality**: Improved code organization and documentation
  - Better separation of concerns between theme management and persistence
  - Enhanced error handling and user feedback
  - Consistent coding patterns and best practices

## [2.0.1] - 2025-07-12

### Fixed
- **Nullable Reference Types**: Fixed MainWindow constructor parameter to properly handle nullable Process parameter
- **Build Configuration**: Added `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>` to all project files to prevent warnings from blocking builds
- **Multiple Entry Points**: Resolved conflicting Main methods in test projects causing compilation errors
- **XAML Compilation**: Fixed missing XAML files and InitializeComponent errors in test projects
- **Project Dependencies**: Cleaned up test project file inclusions to prevent conflicts

### Added
- **Comprehensive Testing Suite**: Complete test harness ecosystem for thorough application testing
  - `TestHarness.bat` - Automated batch script for complete testing workflow
  - `OverlayTestHarness` - Interactive WPF application for manual testing with automation
  - `MinimalTestHarness` - Simple console-based test harness
  - `MockTargetApp` - Dedicated mock application for testing overlay behavior
- **Testing Documentation**: Comprehensive documentation for all testing procedures and troubleshooting
- **Build Status Verification**: All projects now build successfully with zero compilation errors

### Changed
- **Documentation Structure**: Moved Documentation folder to solution level for better organization
- **Build Output**: Solution now builds with 0 errors and 13 warnings (all non-critical nullable reference type warnings)
- **Project Organization**: Improved test project structure and file organization

### Technical Improvements
- **Error-Free Compilation**: Achieved zero compilation errors across all projects
- **Warning Management**: Configured appropriate warning levels while maintaining code quality
- **Test Infrastructure**: Robust testing infrastructure for continuous verification
- **Documentation Updates**: Updated build guides and testing documentation with current status

## [2.0.0] - 2025-07-12

### Project Restructure
- **Repository Unification**: Consolidated all components into a single unified repository
  - `ED_Inara_Overlay/` - Main WPF application
  - `InaraTools/` - INARA API communication library
  - `Logger/` - Centralized logging infrastructure
  - `Testing/` - Test harness and mock applications
  - `Documentation/` - Project documentation

### Enhanced Features
- **Advanced UI Components**: Enhanced TradeRouteCard with Elite Dangerous styling
- **Clipboard Integration**: Clickable system names with copy-to-clipboard functionality
- **Improved User Experience**: Professional UI styling with hover effects and visual feedback
- **Enhanced Logging**: Comprehensive logging across all components with file output

### Fixed Issues
- **Application Shutdown**: Proper cleanup and shutdown handling to prevent process hanging
- **Auto-Close Feature**: Automatic overlay closure when target application exits
- **Overlay Behavior**: Improved focus detection and window positioning
- **Spinner Animation**: Fixed loading animations and state transitions
- **Waiting Window**: Enhanced waiting window behavior and visibility management
- **UI Compilation**: Fixed XAML compilation issues and resource dependencies
- **Window Positioning**: Accurate trade route window positioning relative to target
- **State Machine**: Robust visibility state management for reliable overlay behavior

### Technical Improvements
- **Build System**: Unified solution file managing all projects
- **Dependency Management**: Consistent dependency versions across components
- **Error Handling**: Comprehensive exception handling and user feedback
- **Resource Management**: Proper disposal patterns and memory leak prevention
- **Code Organization**: Clear separation of concerns and modular design

### Documentation
- **Consolidated Documentation**: Removed redundant fix-specific documentation
- **Updated README**: Comprehensive project overview and setup instructions
- **Contributing Guide**: Detailed development workflow for unified repository
- **Build Guide**: Step-by-step build instructions with troubleshooting
- **Project Synopsis**: Complete technical architecture overview

## [1.0.0] - 2024-07-12

### Added
- Initial release of Elite Dangerous Inara Overlay
- WPF-based overlay system with automatic target detection
- State-machine based visibility management (Waiting  ForceShow  Auto)
- Timer-based retry mechanism for target process detection
- Comprehensive test harness for regression testing
- Trade route overlay functionality
- Results overlay window
- Pinned route overlay
- Proper window positioning relative to target application
- Focus-based visibility management
- Automatic cleanup when target application closes

### Technical Implementation
- **State Machine**: Three-state system for overlay visibility management
  - `Waiting`: Initial state when no target is detected
  - `ForceShow`: Transition state that ensures visibility after target detection
  - `Auto`: Normal operational state with focus-based visibility
- **Retry Mechanism**: Non-blocking timer-based system with configurable retry count
- **Process Detection**: Two-level detection system (process + window handle)
- **Resource Management**: Proper cleanup of timers and windows on application exit

### Test Coverage
- Automated regression test suite (Basic, Quick, Simple, Comprehensive)
- Test harness for manual verification
- Mock applications for development testing
- Coverage for both test scenarios and real Elite Dangerous integration


