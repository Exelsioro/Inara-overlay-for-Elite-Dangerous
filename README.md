# Elite Dangerous Inara Overlay

A .NET 8 WPF overlay application for Elite Dangerous that provides trade route information from INARA.

## Features

- **Smart Overlay System**: Automatically attaches to Elite Dangerous window
- **Focus-Aware**: Shows/hides based on target window focus and state
- **Trade Route Search**: Search for profitable trade routes using INARA data
- **Results Overlay**: Display trade route results in a convenient overlay
- **Non-Intrusive**: Overlay doesn't steal focus from the game
- **Manual Overlay Control**: User-controlled overlay startup with "Start Overlay" button
- **Global Hotkey Support**: Ctrl+5 hotkey to toggle trade route window from anywhere
- **Comprehensive Theme System**: Advanced theming with persistence across sessions
- **Settings Management**: JSON-based configuration system for user preferences
- **Ko-fi Integration**: Built-in support link for project development

## Repository Structure

**🎯 Unified Repository**: This repository has been unified to contain all components of the Elite Dangerous Inara Overlay system in a single location for easier development and maintenance.

### Project Layout
```
ED_Inara_Overlay/
├── ED_Inara_Overlay/              # Main WPF application
│   ├── MainWindow.xaml.cs         # Primary overlay window
│   ├── Windows/                   # UI windows and dialogs
│   ├── UserControls/              # Reusable UI components
│   ├── ViewModels/                # MVVM view models
│   ├── Utils/                     # Utility classes
│   └── Resources/                 # Application resources
├── InaraTools/                    # INARA API communication library
│   ├── InaraParserUtils.cs        # HTML parsing utilities
│   ├── InaraCommunicator.cs       # API communication
│   └── TradeRouteModels.cs        # Data models
├── Logger/                        # Logging infrastructure
│   └── Logger.cs                  # Centralized logging
├── MockTargetApp/                 # Test application for development
│   └── Program.cs                 # Mock Elite Dangerous window
├── Inara_Parser/                  # Legacy parser (being phased out)
└── OverlayTestHarness.csproj     # Test harness for overlay testing
```

### Architecture

#### Technology Stack
- **.NET 8.0** - Target framework
- **WPF** - Windows Presentation Foundation for UI
- **Windows API (P/Invoke)** - Window management and overlay functionality
- **HtmlAgilityPack** - HTML parsing for web requests

#### Component Responsibilities
- **ED_Inara_Overlay** - Main application with overlay UI
- **InaraTools** - Shared library for INARA API communication
- **Logger** - Centralized logging across all components
- **MockTargetApp** - Development testing utility
- **OverlayTestHarness** - Integration testing framework

### Architecture Diagram
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│                 │    │                 │    │                 │
│ .NET 8 WPF      │◄──►│ InaraTools      │◄──►│ INARA API       │
│ Front-End       │    │ Helper Library  │    │ (External)      │
│ UI Components   │    └─────────────────┘    └─────────────────┘
│                 │                                      │
└─────────────────┘                                      │
    │   ▲                                                ▼
    │   │          ┌─────────────────┐    ┌─────────────────┐
    │   │          │                 │    │                 │
    │   └─────────►│ Logger          │    │ Windows API     │
    │              │ Logging         │    │ P/Invoke        │
    └─────────────►│                 │    │                 │
                   └─────────────────┘    └─────────────────┘
```

## Key Features

### Overlay Management
- **Target Window Attachment**: Automatically finds and attaches to Elite Dangerous
- **Focus Detection**: Uses Windows API to monitor window focus states
- **Position Tracking**: Maintains relative position to target window
- **State Preservation**: Remembers overlay state when focus changes

### Focus Handling
- **Non-Activating Windows**: Overlay windows don't steal focus using `WS_EX_NOACTIVATE`
- **Smart Visibility**: Shows when target window is focused and visible
- **Seamless Integration**: Overlay windows are considered part of the target application

### Theme System
- **XML-Based Themes**: Complete theme definitions with colors, fonts, and dimensions
- **Theme Persistence**: Automatically saves and restores selected themes
- **Built-in Themes**: Default Elite Dangerous-inspired theme included
- **Custom Theme Support**: Import/export custom themes
- **Real-time Preview**: Theme changes apply immediately

### Global Hotkey System
- **Ctrl+5 Hotkey**: System-wide hotkey registration using Windows API
- **Background Operation**: Works even when Elite Dangerous is in focus
- **Toggle Functionality**: Same action as clicking the toggle button
- **Automatic Registration**: Hotkey is registered when the overlay starts
- **Conflict Handling**: Graceful fallback if hotkey is already in use
- **Thread Safety**: Hotkey events are properly marshaled to the UI thread

### User Experience Enhancements
- **Waiting Window**: Enhanced UI while target application is not running
- **Manual Control**: User must explicitly click "Start Overlay" button
- **Visual Feedback**: Animated status messages and progress indicators
- **Settings Window**: Centralized configuration with theme selection
- **Ko-fi Integration**: Built-in support link with custom coffee cup icon
- **Application Icon**: Custom app icon for taskbar and system UI

### Settings Management
- **JSON Configuration**: Persistent settings stored in `%APPDATA%/ED_Inara_Overlay/settings.json`
- **Theme Preferences**: Automatically restore selected theme on startup
- **Version Tracking**: Settings include version and timestamp information
- **Fallback Handling**: Graceful degradation when settings are unavailable

## Installation

1. Clone the repository
2. Open the solution in Visual Studio 2022
3. Build the solution
4. Run the application

## Usage

1. Launch Elite Dangerous (or your target application)
2. Run the overlay application
3. The waiting window will appear, monitoring for the target application
4. When the target is detected, status changes to "Target application found!"
5. Click the "Start Overlay" button to activate the overlay system
6. The overlay will attach to the target window and become available
7. Use the toggle button to show/hide the trade route search window
8. **Global Hotkey**: Press **Ctrl+5** from anywhere to toggle the trade route window
9. Search for trade routes and view results in the overlay
10. Access Settings to customize themes and other preferences
11. Use the Ko-fi link to support development (optional)

## Development

### Prerequisites
- Visual Studio 2022 or Visual Studio Code
- .NET 8.0 SDK
- Windows 10/11

### Building the Solution

#### Option 1: Build Everything
```bash
# Build all projects in the unified solution
dotnet build ED_Inara_Overlay/ED_Inara_Overlay.sln
```

#### Option 2: Build Individual Projects
```bash
# Build main application
dotnet build ED_Inara_Overlay/ED_Inara_Overlay.csproj

# Build supporting libraries
dotnet build InaraTools/InaraTools.csproj
dotnet build Logger/Logger.csproj
```

### Running the Application
```bash
# Run the main overlay application
dotnet run --project ED_Inara_Overlay/ED_Inara_Overlay.csproj
```

### Testing
```bash
# Run the test harness
dotnet run --project OverlayTestHarness.csproj

# Run mock target app for development
dotnet run --project MockTargetApp/MockTargetApp.csproj
```

### Repository Migration Notes

This repository was unified from multiple separate repositories to improve:
- **Dependency Management**: All components now share the same dependency versions
- **Build Consistency**: Single solution file manages all projects
- **Development Workflow**: Easier to develop and test cross-component features
- **Version Control**: Unified versioning and release management

**Previous Structure**: Components were in separate repositories with individual build processes
**Current Structure**: All components unified under a single repository with shared build infrastructure

## Contributing

We welcome contributions to the Elite Dangerous Inara Overlay project! This is a unified repository containing all components of the overlay system.

**📋 Please read our [Contributing Guide](CONTRIBUTING.md) for detailed information about:**
- Repository structure and unified development workflow
- Setting up your development environment
- Code style guidelines and best practices
- Testing procedures and tools
- Pull request process

### Quick Start for Contributors

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes following our [coding guidelines](CONTRIBUTING.md#code-style-guidelines)
4. Test thoroughly using the provided test tools
5. Submit a pull request with a clear description

**🔍 New to the unified repository?** Check out the [Repository Migration Notes](CONTRIBUTING.md#migration-from-separate-repositories) to understand how the project structure has evolved.

## License

This project is licensed under the MIT License.

---

**Last Updated**: July 18, 2025
**Framework**: .NET 8.0
