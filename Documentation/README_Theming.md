# ED Inara Overlay - Theming System

## Overview
The ED Inara Overlay now includes a comprehensive theming system that allows users to customize the appearance of the application with different color schemes and styles.

## Features

### 🎨 Theme Management
- **Multiple theme support**: Support for multiple XML-based themes with comprehensive theming options
- **Runtime theme switching**: Change themes without restarting the application with instant preview
- **Automatic theme persistence**: User's theme preference is automatically saved and restored on application restart
- **Theme preview**: Live preview of themes in the settings window with real-time updates
- **Settings management**: Robust JSON-based settings system with automatic backup and recovery
- **Theme state tracking**: Current theme is properly tracked and displayed in settings interface

### 🎨 Available Themes
1. **Default**: Dark theme with green accents (Elite Dangerous style)
2. **BlueAccent**: Default theme with blue accent colors
3. **DarkBlue**: Dark blue theme with teal accents

### 🎨 UI Settings
- **Opacity control**: Adjust overlay transparency
- **Always on top**: Control whether overlays stay on top of other windows
- **Live preview**: See theme changes immediately
- **Reset to defaults**: Easy way to restore default settings

## Architecture

### Components
1. **ThemeManager** (`Services/ThemeManager.cs`):
   - Manages theme loading and application
   - Handles theme discovery and validation
   - Provides theme switching functionality
   - Integrates with SettingsService for persistence
   - Automatic theme restoration on startup

2. **SettingsService** (`Services/SettingsService.cs`):
   - JSON-based settings persistence and management
   - Handles theme preference storage
   - Manages application configuration
   - Singleton pattern for global access
   - Automatic file creation and error handling

3. **SettingsWindow** (`Windows/SettingsWindow.xaml`):
   - User interface for theme selection
   - Controls for overlay settings
   - Live theme preview with real-time updates
   - Current theme selection display
   - Theme import/export functionality

4. **Theme Model** (`Models/Theme.cs`):
   - Defines theme data structure
   - XML serialization support
   - Color, font, and dimension definitions
   - Comprehensive theme metadata

5. **Theme Files** (`Themes/*.xml`):
   - XML-based theme definitions
   - Color definitions for all UI elements
   - Font and dimension configurations
   - Modular and extensible structure

### File Structure
```
ED_Inara_Overlay_2.0/
├── Utils/
│   ├── Themes/
│   │   └── ThemeManager.cs
│   └── Config/
│       └── ConfigManager.cs
├── Windows/
│   ├── SettingsWindow.xaml
│   └── SettingsWindow.xaml.cs
├── Themes/
│   ├── DefaultTheme.xml
│   ├── BlueAccentTheme.xml
│   └── DarkBlueTheme.xml
└── Resources/
    └── UIStyles.xaml
```

## How It Works

### 1. Theme Loading
- Themes are loaded from the `Themes/` directory at application startup
- Each theme is an XML file with a `<Theme Name="...">` root element
- Theme files define color values for various UI elements

### 2. Theme Application
- When a theme is selected, `ThemeManager.ApplyTheme()` is called
- The theme XML is parsed and color values are applied to `Application.Current.Resources`
- UI elements automatically update through WPF's dynamic resource binding

### 3. Theme Persistence
- User's theme selection is saved in `%APPDATA%/ED_Inara_Overlay/config.json`
- On application startup, the saved theme is automatically applied
- Other settings like opacity and always-on-top are also persisted

## Usage

### Accessing Settings
1. Right-click on the main overlay window
2. Select "Settings" from the context menu
3. The settings window will open with theme selection options

### Changing Themes
1. In the settings window, select a theme from the dropdown
2. The theme is applied immediately with live preview
3. The selection is automatically saved

### Adjusting Overlay Settings
1. Use the opacity slider to adjust transparency
2. Toggle "Always on top" to control window layering
3. Changes are applied immediately to all overlay windows

## Creating Custom Themes

### Theme File Structure
```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme Name="CustomTheme">
  <!-- Background Colors -->
  <Color Key="PrimaryBackgroundColor" Value="#FF1E1E1E"/>
  <Color Key="SecondaryBackgroundColor" Value="#FF2A2A2A"/>
  
  <!-- Text Colors -->
  <Color Key="PrimaryTextColor" Value="#FFFFFFFF"/>
  <Color Key="SecondaryTextColor" Value="#FFE0E0E0"/>
  
  <!-- More color definitions... -->
</Theme>
```

### Color Keys
The following color keys are used throughout the application:

#### Background Colors
- `PrimaryBackgroundColor`: Main background color
- `SecondaryBackgroundColor`: Secondary background color
- `CardBackgroundColor`: Card/panel background color
- `HighlightBackgroundColor`: Highlight/selection color

#### Text Colors
- `PrimaryTextColor`: Primary text color
- `SecondaryTextColor`: Secondary text color
- `MutedTextColor`: Muted/disabled text color
- `SuccessTextColor`: Success messages and positive values
- `ErrorTextColor`: Error messages and negative values
- `WarningTextColor`: Warning messages

#### Border Colors
- `BorderColor`: Standard border color
- `AccentBorderColor`: Accent border color
- `SuccessBorderColor`: Success border color

#### Interactive Elements
- `PrimaryActionButtonBackgroundColor`: Primary button background
- `SecondaryActionButtonBackgroundColor`: Secondary button background
- `ComboBoxAccentColor`: ComboBox accent color
- `ComboBoxHoverColor`: ComboBox hover color
- `ComboBoxActiveColor`: ComboBox active color

### Adding Custom Themes
1. Create a new XML file in the `Themes/` directory
2. Follow the structure shown above
3. Add the file to the project file with `<CopyToOutputDirectory>Always</CopyToOutputDirectory>`
4. The theme will be automatically discovered on next application start

## Technical Details

### Dynamic Resource Binding
- All UI elements use `{DynamicResource}` bindings for colors
- This allows runtime theme switching without recreating UI elements
- Brush resources are automatically created from color resources

### Color Conversion
- Colors are defined in XML as hex strings (e.g., `#FFRRGGBB`)
- The `ColorConverter` class handles conversion from strings to WPF `Color` objects
- Colors are applied as `SolidColorBrush` resources

### Event System
- `ThemeManager.ThemeApplied` event is fired when themes change
- Components can subscribe to this event for custom theme handling
- Currently used for logging and potential future extensions

## Troubleshooting

### Common Issues
1. **Theme not appearing**: Check that the XML file is valid and in the correct directory
2. **Colors not updating**: Ensure UI elements use `{DynamicResource}` instead of `{StaticResource}`
3. **Settings not saving**: Check file permissions for `%APPDATA%/ED_Inara_Overlay/`

### Debug Information
- Theme loading and application is logged to the application log
- Invalid theme files are logged with error details
- Theme selection changes are logged for debugging

## Future Enhancements

### Planned Features
- Theme editor UI for creating custom themes
- Theme import/export functionality
- Community theme sharing
- Font size and family customization
- More UI customization options

### Extension Points
- `ThemeManager` can be extended to support additional theme formats
- `ConfigManager` can handle additional user preferences
- Additional color keys can be added for new UI elements

## Best Practices

### For Developers
1. Always use `{DynamicResource}` for theme-able elements
2. Define new color keys in all theme files when adding new UI elements
3. Use the established color key naming conventions
4. Test themes with all application windows and states

### For Theme Creators
1. Test themes in different lighting conditions
2. Ensure sufficient contrast for accessibility
3. Consider Elite Dangerous design language
4. Test with all overlay windows visible

## Support
For issues related to theming:
1. Check the application logs for error messages
2. Verify theme XML syntax is valid
3. Ensure all required color keys are defined
4. Test with the default theme to isolate issues
