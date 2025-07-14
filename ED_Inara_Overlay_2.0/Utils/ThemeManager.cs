using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ED_Inara_Overlay_2._0.Utils
{
    /// <summary>
    /// Manages application theming and dynamic theme switching
    /// </summary>
    public static class ThemeManager
    {
        private static readonly Dictionary<string, ThemeDefinition> _themes = new Dictionary<string, ThemeDefinition>();
        private static string _currentTheme = "Dark";

        static ThemeManager()
        {
            InitializeThemes();
        }

        /// <summary>
        /// Gets the current theme name
        /// </summary>
        public static string CurrentTheme => _currentTheme;

        /// <summary>
        /// Gets all available theme names
        /// </summary>
        public static IEnumerable<string> AvailableThemes => _themes.Keys;

        /// <summary>
        /// Initializes default themes
        /// </summary>
        private static void InitializeThemes()
        {
            // Dark Theme (default)
            _themes["Dark"] = new ThemeDefinition
            {
                Name = "Dark",
                Colors = new Dictionary<string, Color>
                {
                    ["PrimaryBackgroundColor"] = Color.FromRgb(30, 30, 30),
                    ["SecondaryBackgroundColor"] = Color.FromRgb(45, 45, 45),
                    ["TertiaryBackgroundColor"] = Color.FromRgb(60, 60, 60),
                    ["PrimaryTextColor"] = Colors.White,
                    ["SecondaryTextColor"] = Color.FromRgb(204, 204, 204),
                    ["DisabledTextColor"] = Color.FromRgb(128, 128, 128),
                    ["AccentColor"] = Color.FromRgb(0, 122, 204),
                    ["AccentHoverColor"] = Color.FromRgb(31, 147, 255),
                    ["AccentPressedColor"] = Color.FromRgb(0, 96, 160),
                    ["BorderColor"] = Color.FromRgb(90, 90, 90),
                    ["FocusBorderColor"] = Color.FromRgb(0, 122, 204),
                    ["SuccessColor"] = Color.FromRgb(40, 167, 69),
                    ["WarningColor"] = Color.FromRgb(255, 193, 7),
                    ["ErrorColor"] = Color.FromRgb(220, 53, 69),
                    ["InfoColor"] = Color.FromRgb(23, 162, 184)
                }
            };

            // Light Theme
            _themes["Light"] = new ThemeDefinition
            {
                Name = "Light",
                Colors = new Dictionary<string, Color>
                {
                    ["PrimaryBackgroundColor"] = Color.FromRgb(255, 255, 255),
                    ["SecondaryBackgroundColor"] = Color.FromRgb(248, 249, 250),
                    ["TertiaryBackgroundColor"] = Color.FromRgb(233, 236, 239),
                    ["PrimaryTextColor"] = Color.FromRgb(33, 37, 41),
                    ["SecondaryTextColor"] = Color.FromRgb(108, 117, 125),
                    ["DisabledTextColor"] = Color.FromRgb(173, 181, 189),
                    ["AccentColor"] = Color.FromRgb(0, 123, 255),
                    ["AccentHoverColor"] = Color.FromRgb(0, 86, 179),
                    ["AccentPressedColor"] = Color.FromRgb(0, 69, 143),
                    ["BorderColor"] = Color.FromRgb(206, 212, 218),
                    ["FocusBorderColor"] = Color.FromRgb(0, 123, 255),
                    ["SuccessColor"] = Color.FromRgb(40, 167, 69),
                    ["WarningColor"] = Color.FromRgb(255, 193, 7),
                    ["ErrorColor"] = Color.FromRgb(220, 53, 69),
                    ["InfoColor"] = Color.FromRgb(23, 162, 184)
                }
            };

            // Blue Theme
            _themes["Blue"] = new ThemeDefinition
            {
                Name = "Blue",
                Colors = new Dictionary<string, Color>
                {
                    ["PrimaryBackgroundColor"] = Color.FromRgb(15, 23, 42),
                    ["SecondaryBackgroundColor"] = Color.FromRgb(30, 41, 59),
                    ["TertiaryBackgroundColor"] = Color.FromRgb(51, 65, 85),
                    ["PrimaryTextColor"] = Color.FromRgb(248, 250, 252),
                    ["SecondaryTextColor"] = Color.FromRgb(203, 213, 225),
                    ["DisabledTextColor"] = Color.FromRgb(148, 163, 184),
                    ["AccentColor"] = Color.FromRgb(59, 130, 246),
                    ["AccentHoverColor"] = Color.FromRgb(96, 165, 250),
                    ["AccentPressedColor"] = Color.FromRgb(37, 99, 235),
                    ["BorderColor"] = Color.FromRgb(71, 85, 105),
                    ["FocusBorderColor"] = Color.FromRgb(59, 130, 246),
                    ["SuccessColor"] = Color.FromRgb(34, 197, 94),
                    ["WarningColor"] = Color.FromRgb(251, 191, 36),
                    ["ErrorColor"] = Color.FromRgb(239, 68, 68),
                    ["InfoColor"] = Color.FromRgb(14, 165, 233)
                }
            };
        }

        /// <summary>
        /// Applies a theme to the application
        /// </summary>
        /// <param name="themeName">Name of the theme to apply</param>
        /// <returns>True if theme was applied successfully, false otherwise</returns>
        public static bool ApplyTheme(string themeName)
        {
            if (!_themes.ContainsKey(themeName))
                return false;

            try
            {
                var theme = _themes[themeName];
                var appResources = Application.Current.Resources;

                // Update color resources
                foreach (var colorPair in theme.Colors)
                {
                    if (appResources.Contains(colorPair.Key))
                    {
                        appResources[colorPair.Key] = colorPair.Value;
                    }
                }

                // Update brush resources
                foreach (var colorPair in theme.Colors)
                {
                    var brushKey = colorPair.Key.Replace("Color", "Brush");
                    if (appResources.Contains(brushKey))
                    {
                        appResources[brushKey] = new SolidColorBrush(colorPair.Value);
                    }
                }

                _currentTheme = themeName;
                return true;
            }
            catch (Exception ex)
            {
                // Log error if logging is available
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme '{themeName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the theme from configuration
        /// </summary>
        /// <param name="configuredTheme">Theme name from configuration</param>
        public static void LoadThemeFromConfig(string configuredTheme)
        {
            if (!string.IsNullOrEmpty(configuredTheme) && _themes.ContainsKey(configuredTheme))
            {
                ApplyTheme(configuredTheme);
            }
            else
            {
                // Default to Dark theme if configuration is invalid
                ApplyTheme("Dark");
            }
        }

        /// <summary>
        /// Gets the color value for a specific color key in the current theme
        /// </summary>
        /// <param name="colorKey">The color key to retrieve</param>
        /// <returns>Color value or default color if not found</returns>
        public static Color GetColor(string colorKey)
        {
            if (_themes.ContainsKey(_currentTheme) && _themes[_currentTheme].Colors.ContainsKey(colorKey))
            {
                return _themes[_currentTheme].Colors[colorKey];
            }
            return Colors.Transparent;
        }

        /// <summary>
        /// Gets a brush for a specific color key in the current theme
        /// </summary>
        /// <param name="colorKey">The color key to retrieve</param>
        /// <returns>SolidColorBrush or null if not found</returns>
        public static SolidColorBrush GetBrush(string colorKey)
        {
            var color = GetColor(colorKey);
            return color != Colors.Transparent ? new SolidColorBrush(color) : null;
        }

        /// <summary>
        /// Registers a custom theme
        /// </summary>
        /// <param name="theme">Theme definition to register</param>
        public static void RegisterTheme(ThemeDefinition theme)
        {
            if (theme != null && !string.IsNullOrEmpty(theme.Name))
            {
                _themes[theme.Name] = theme;
            }
        }
    }

    /// <summary>
    /// Represents a theme definition with colors
    /// </summary>
    public class ThemeDefinition
    {
        public string Name { get; set; }
        public Dictionary<string, Color> Colors { get; set; } = new Dictionary<string, Color>();
    }
}
