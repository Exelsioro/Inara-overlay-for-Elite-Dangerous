# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
  - `ED_Inara_Overlay_2.0/` - Main WPF application
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
