# Inara Overlay for Elite Dangerous v2

A .NET 8 WPF overlay application for Elite Dangerous that provides trade route information from INARA.

## Features

- **Smart Overlay System**: Automatically attaches to Elite Dangerous window
- **Focus-Aware**: Shows/hides based on target window focus and state
- **Trade Route Search**: Search for profitable trade routes using INARA data
- **Results Overlay**: Display trade route results in a convenient overlay
- **Non-Intrusive**: Overlay doesn't steal focus from the game
- **State-Machine Based Visibility**: Robust retry mechanism for reliable target detection

## Recent Updates

### Fixed Initial Overlay Visibility
- **Problem**: Overlay would remain invisible when Elite Dangerous took time to start
- **Solution**: Implemented state-machine based retry mechanism
- **States**: `Waiting` → `ForceShow` → `Auto` for robust visibility management
- **Test Coverage**: Comprehensive test harness for regression testing

## Architecture

### Technology Stack
- **.NET 8.0** - Target framework
- **WPF** - Windows Presentation Foundation for UI
- **Windows API (P/Invoke)** - Window management and overlay functionality
- **HtmlAgilityPack** - HTML parsing for web requests

### Project Structure
- **MainWindow** - Main overlay control window
- **TradeRouteWindow** - Trade route search interface
- **ResultsOverlayWindow** - Results display overlay
- **InaraTools** - Helper library for INARA API communication
- **Logger** - Logging functionality
- **WindowsAPI** - Windows API wrapper utilities

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
- **Retry Mechanism**: Non-blocking timer-based retry for reliable target detection

### Focus Handling
- **Non-Activating Windows**: Overlay windows don't steal focus using `WS_EX_NOACTIVATE`
- **Smart Visibility**: Shows when target window is focused and visible
- **Seamless Integration**: Overlay windows are considered part of the target application

## Installation

1. Clone the repository
2. Open the solution in Visual Studio 2022
3. Build the solution
4. Run the application

## Usage

1. Launch Elite Dangerous
2. Run the overlay application
3. The overlay will automatically attach to the Elite Dangerous window
4. Use the toggle button to show/hide the trade route search window
5. Search for trade routes and view results in the overlay

## Testing

### Test Harness
- **TestHarness.ps1**: Manual testing with mock applications
- **RegressionTest.ps1**: Comprehensive automated testing
- **QuickRegressionTest.ps1**: Fast regression testing
- **BasicRegressionTest.ps1**: Basic functionality testing

### Running Tests
```powershell
# Run all tests
.\RunTests.ps1

# Run specific test
.\QuickRegressionTest.ps1
```

## Development

### Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK
- Windows 10/11

### Building
```bash
dotnet build
```

### Running
```bash
dotnet run
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly using the test harness
5. Submit a pull request

## License

This project is licensed under the MIT License.
