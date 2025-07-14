# Theme System Documentation

## Overview

The theme system uses a simple, flat XML structure to define color values that can be loaded at runtime. Themes allow for easy customization of the application's appearance without modifying the source code.

## XML Structure

```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme Name="ThemeName">
  <Color Key="ColorKey" Value="#AARRGGBB"/>
  <!-- More color definitions -->
</Theme>
```

### Elements

- **Theme**: Root element with a `Name` attribute
- **Color**: Individual color definition with `Key` and `Value` attributes
  - `Key`: The color resource key (must match the keys used in UIStyles.xaml)
  - `Value`: Hex color value in ARGB format (#AARRGGBB)

## Required Color Keys

The following color keys are required for a complete theme. If any key is missing, the application will fall back to hard-coded default values.

### Background Colors
- `PrimaryBackgroundColor`: Main background color for windows and controls
- `SecondaryBackgroundColor`: Secondary background color for panels and sections
- `CardBackgroundColor`: Background color for card-style containers
- `HighlightBackgroundColor`: Background color for highlighted/selected items

### Border Colors
- `BorderColor`: Standard border color for controls
- `AccentBorderColor`: Accent border color for emphasis
- `SuccessBorderColor`: Border color for success states

### Text Colors
- `PrimaryTextColor`: Main text color for headers and primary content
- `SecondaryTextColor`: Secondary text color for body text
- `MutedTextColor`: Muted text color for captions and disabled text
- `SuccessTextColor`: Text color for success messages and values
- `ErrorTextColor`: Text color for error messages
- `WarningTextColor`: Text color for warning messages

### Overlay and Transparency Colors
- `OverlayBackgroundColor`: Background color for modal overlays
- `ContentOverlayBackgroundColor`: Background color for content overlays
- `CloseButtonBackgroundColor`: Background color for close buttons
- `PrimaryActionButtonBackgroundColor`: Background color for primary action buttons
- `SecondaryActionButtonBackgroundColor`: Background color for secondary action buttons

### ScrollBar Colors
- `ScrollBarBackgroundColor`: Background color for scrollbar track
- `ScrollBarBorderColor`: Border color for scrollbar elements
- `ScrollBarForegroundColor`: Foreground color for scrollbar elements
- `ScrollBarThumbBackgroundColor`: Background color for scrollbar thumb
- `ScrollBarThumbBorderColor`: Border color for scrollbar thumb

### ComboBox Colors
- `ComboBoxAccentColor`: Accent color for ComboBox controls
- `ComboBoxHoverColor`: Hover color for ComboBox controls
- `ComboBoxActiveColor`: Active/selected color for ComboBox controls

## Partial Override Support

The theme system supports partial overrides:
- **Unknown keys**: Any color keys not recognized by the system are ignored
- **Missing keys**: If a required key is missing from the theme file, the application will use hard-coded default values
- **Invalid values**: If a color value is invalid, the default value will be used

## Usage Examples

### Complete Theme
```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme Name="Dark">
  <Color Key="PrimaryBackgroundColor" Value="#FF1E1E1E"/>
  <Color Key="SecondaryBackgroundColor" Value="#FF2A2A2A"/>
  <!-- ... all other required keys ... -->
</Theme>
```

### Partial Theme (Only Override Specific Colors)
```xml
<?xml version="1.0" encoding="utf-8"?>
<Theme Name="BlueAccent">
  <Color Key="ComboBoxAccentColor" Value="#FF0078D4"/>
  <Color Key="ComboBoxHoverColor" Value="#FF106EBE"/>
  <Color Key="ComboBoxActiveColor" Value="#FF005A9E"/>
  <!-- Missing keys will use default values -->
</Theme>
```

## Color Format

All color values must be in hex format with the alpha channel included:
- Format: `#AARRGGBB`
- Example: `#FF1E1E1E` (fully opaque dark gray)
- Example: `#80FF0000` (50% transparent red)

## Available Themes

The application includes several built-in themes:

1. **Default**: Dark theme with green accents (Elite Dangerous style)
2. **BlueAccent**: Default theme with blue accent colors (partial override)
3. **DarkBlue**: Dark blue theme with teal accents
4. **Elite Dangerous HUD**: Authentic Elite Dangerous HUD colors with:
   - Orange (#FF8000) main HUD text and lines
   - Golden Yellow (#FFCC00) accents and highlights
   - Blue (#0066FF) for shields/defense elements
   - Cyan (#00FFFF) for hull integrity
   - Red (#FF0000) for hostile targets and errors
   - Green (#00FF00) for friendly targets and success
   - Black (#000000) backgrounds for authentic HUD appearance

## File Location

Theme files should be placed in the `Themes` directory relative to the application executable.
