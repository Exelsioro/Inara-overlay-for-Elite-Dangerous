# Elite Dangerous INARA Overlay 2.0 - Documentation

Welcome to the comprehensive documentation for the Elite Dangerous INARA Overlay 2.0 project.

## Documentation Index

### 📋 Project Overview
- [Project Synopsis](Project_Synopsis.md) - Comprehensive overview of the project's purpose, features, and architecture
- [Technical Audit](Audit.txt) - Detailed technical analysis, code quality assessment, and recommendations
- [Codebase Reference](codebase_reference/ED_Inara_Overlay_2.0_summary.md) - Source code structure and file organization

### 🏗️ Architecture

#### Core Components
1. **Main Application (ED_Inara_Overlay_2.0)**
   - .NET 8.0 WPF Windows Application
   - Overlay management and window control
   - User interface components

2. **InaraTools Library**
   - INARA API communication
   - Web scraping and data parsing
   - Trade route models and utilities

3. **Logger Library**
   - Centralized logging system
   - File-based logging with timestamps
   - User action tracking

#### Technology Stack
- **Framework**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **Web Scraping**: HtmlAgilityPack
- **API Integration**: Windows API (P/Invoke)
- **Logging**: Custom file-based logging

### 🎯 Features

#### Overlay Management
- **Smart Window Attachment**: Automatically detects and attaches to Elite Dangerous
- **Focus-Aware Behavior**: Shows/hides based on target window state
- **Non-Intrusive Design**: Doesn't steal focus from the game
- **Real-time Position Tracking**: Maintains relative position to target window

#### Trade Route Functionality
- **INARA Integration**: Retrieves trade route data from INARA community database
- **Advanced Filtering**: Supports multiple search criteria (cargo capacity, distance, station types)
- **Real-time Results**: Displays current market data and profit calculations
- **Round-trip Support**: Handles both single-leg and round-trip routes

#### User Interface
- **Main Overlay Window**: Primary control interface
- **Trade Route Search Window**: Advanced search interface
- **Results Overlay Window**: Non-intrusive results display
- **Responsive Design**: Adapts to different screen sizes and resolutions

### 🔧 Technical Details

#### Window Management
- Uses Windows API for advanced overlay behavior
- Implements `WS_EX_NOACTIVATE` for non-activating windows
- Real-time focus detection and visibility management
- Proper window positioning with bounds checking

#### Data Processing
- Web scraping of INARA trade route pages
- HTML parsing using HtmlAgilityPack
- Structured data models for trade routes and stations
- Error handling with fallback data

#### Performance Optimization
- 60 FPS update rate for smooth overlay behavior
- Async/await patterns for non-blocking operations
- Efficient memory usage and disposal patterns
- Minimal CPU overhead

### 📊 Project Status

#### Current State
- **Version**: 2.0
- **Framework**: .NET 8.0
- **Status**: Active Development
- **Platform**: Windows 10/11

#### Recent Updates
- Modern .NET 8.0 migration
- Enhanced overlay behavior
- Improved error handling
- Comprehensive logging system
- Advanced window management

### 🛠️ Development Information

#### Build Requirements
- Visual Studio 2022 (recommended)
- .NET 8.0 SDK
- Windows 10/11 development environment

#### Project Structure
```
ED_Inara_Overlay_2.0/
├── ED_Inara_Overlay_2.0/          # Main WPF application
│   ├── MainWindow.xaml/cs         # Primary overlay controller
│   ├── Windows/                   # Additional window components
│   ├── UserControls/              # Reusable UI components
│   ├── Utils/                     # Utility classes
│   ├── ViewModels/                # MVVM view models
│   ├── Resources/                 # UI resources and styles
│   └── Documentation/             # Project documentation
├── InaraTools/                    # INARA integration library
│   ├── InaraCommunicator.cs       # API communication
│   ├── TradeRouteModels.cs        # Data models
│   └── InaraParserUtils.cs        # HTML parsing
└── Logger/                        # Logging framework
    └── Logger.cs                  # Centralized logging
```

### 📝 Usage Guidelines

#### For End Users
1. Launch Elite Dangerous
2. Run the overlay application
3. Use the toggle button to access trade route search
4. Enter search criteria and view results
5. Overlay automatically manages visibility based on game focus

#### For Developers
1. Clone the repository
2. Open the solution in Visual Studio
3. Build the solution
4. Run for development/testing
5. Refer to audit document for improvement areas

### 🔍 Quality Assurance

#### Code Quality
- **Architecture**: Clean separation of concerns
- **Patterns**: Proper async/await usage
- **Error Handling**: Comprehensive exception management
- **Logging**: Detailed debugging and monitoring

#### Testing Status
- **Unit Tests**: ⚠️ Currently missing (recommended for Phase 3)
- **Integration Tests**: ⚠️ Currently missing
- **Manual Testing**: ✅ Extensive manual testing performed
- **Performance Testing**: ✅ Validated overlay performance

### 🔮 Future Roadmap

#### Phase 1: Foundation (1-2 weeks)
- Enhanced documentation
- Improved error handling
- Basic configuration management

#### Phase 2: User Experience (2-3 weeks)
- User settings persistence
- Configuration UI
- Performance optimizations

#### Phase 3: Quality Assurance (3-4 weeks)
- Comprehensive unit test suite
- Integration testing
- Automated testing pipeline

#### Phase 4: Advanced Features (2-3 weeks)
- Additional overlay features
- Enhanced INARA integration
- Community feedback integration

### 📞 Support Information

#### Documentation Updates
This documentation is automatically maintained and updated with each release. For the most current information, refer to the project repository.

#### Issue Reporting
For bugs, feature requests, or technical issues, please use the project's issue tracking system.

#### Contributing
Contributions are welcome! Please refer to the project's contribution guidelines and code style standards.

---

**Last Updated**: July 11, 2025  
**Documentation Version**: 2.0  
**Target Framework**: .NET 8.0
