using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Xml.Serialization;
using ED_Inara_Overlay_2._0.Models;

namespace ED_Inara_Overlay_2._0.Services
{
    /// <summary>
    /// Manages theme loading, application, and persistence
    /// </summary>
    public class ThemeManager
    {
        private static ThemeManager? _instance;
        private List<Theme> _availableThemes = new();
        private Theme? _currentTheme;
        private readonly string _themesDirectory;
        private readonly string _userThemesDirectory;

        public static ThemeManager Instance => _instance ??= new ThemeManager();

        /// <summary>
        /// Event fired when a theme is applied
        /// </summary>
        public event EventHandler<ThemeAppliedEventArgs>? ThemeApplied;

        /// <summary>
        /// Event fired when available themes are updated
        /// </summary>
        public event EventHandler? ThemesUpdated;

        /// <summary>
        /// Currently applied theme
        /// </summary>
        public Theme? CurrentTheme => _currentTheme;

        /// <summary>
        /// List of all available themes
        /// </summary>
        public IReadOnlyList<Theme> AvailableThemes => _availableThemes.AsReadOnly();

        private ThemeManager()
        {
            _themesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
            _userThemesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ED_Inara_Overlay", "Themes");
            
            // Ensure directories exist
            Directory.CreateDirectory(_themesDirectory);
            Directory.CreateDirectory(_userThemesDirectory);
            
            LoadAvailableThemes();
        }

        /// <summary>
        /// Load all available themes from both built-in and user directories
        /// </summary>
        public void LoadAvailableThemes()
        {
            _availableThemes.Clear();

            try
            {
                // Load built-in themes
                LoadThemesFromDirectory(_themesDirectory);

                // Load user themes
                LoadThemesFromDirectory(_userThemesDirectory);

                // If no themes found, create default theme
                if (_availableThemes.Count == 0)
                {
                    CreateAndSaveDefaultTheme();
                }

                Logger.Logger.Info($"Loaded {_availableThemes.Count} themes");
                ThemesUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error loading themes: {ex.Message}");
                CreateAndSaveDefaultTheme();
            }
        }

        /// <summary>
        /// Load themes from a specific directory
        /// </summary>
        private void LoadThemesFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return;

            var themeFiles = Directory.GetFiles(directory, "*.xml");
            
            foreach (var themeFile in themeFiles)
            {
                try
                {
                    var theme = LoadThemeFromFile(themeFile);
                    if (theme != null)
                    {
                        _availableThemes.Add(theme);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"Error loading theme from {themeFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Load a theme from an XML file
        /// </summary>
        private Theme? LoadThemeFromFile(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Theme));
                using var reader = new FileStream(filePath, FileMode.Open);
                var theme = (Theme?)serializer.Deserialize(reader);
                
                if (theme != null)
                {
                    Logger.Logger.Info($"Loaded theme: {theme.Name} from {filePath}");
                }
                
                return theme;
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Failed to load theme from {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Apply a theme to the application
        /// </summary>
        public bool ApplyTheme(string themeName)
        {
            var theme = _availableThemes.FirstOrDefault(t => t.Name == themeName);
            if (theme == null)
            {
                Logger.Logger.Error($"Theme not found: {themeName}");
                return false;
            }

            return ApplyTheme(theme);
        }

        /// <summary>
        /// Apply a theme to the application
        /// </summary>
        public bool ApplyTheme(Theme theme)
        {
            try
            {
                Logger.Logger.Info($"Applying theme: {theme.Name}");

                var resources = Application.Current.Resources;
                
                // Apply colors
                foreach (var color in theme.Colors)
                {
                    try
                    {
                        var colorValue = (Color)ColorConverter.ConvertFromString(color.Value);
                        resources[color.Key] = colorValue;
                        //resources[color.Key + "Brush"] = new SolidColorBrush(colorValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Error($"Error applying color {color.Key}: {ex.Message}");
                    }
                }

                // Apply fonts
                foreach (var font in theme.Fonts)
                {
                    try
                    {
                        resources[font.Key + "Family"] = new FontFamily(font.Family);
                        resources[font.Key + "Size"] = font.Size;
                        resources[font.Key + "Weight"] = GetFontWeight(font.Weight);
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Error($"Error applying font {font.Key}: {ex.Message}");
                    }
                }

                // Apply dimensions
                foreach (var dimension in theme.Dimensions)
                {
                    try
                    {
                        resources[dimension.Key] = dimension.Value;
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Error($"Error applying dimension {dimension.Key}: {ex.Message}");
                    }
                }

                _currentTheme = theme;
                
                // Force re-rendering of all UI elements
                ForceUIRerendering();
                
                // Fire theme applied event
                ThemeApplied?.Invoke(this, new ThemeAppliedEventArgs(theme));
                
                Logger.Logger.Info($"Theme applied successfully: {theme.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error applying theme {theme.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save a theme to the user themes directory
        /// </summary>
        public bool SaveTheme(Theme theme, bool overwrite = false)
        {
            try
            {
                var fileName = $"{theme.Name}.xml";
                var filePath = Path.Combine(_userThemesDirectory, fileName);

                if (File.Exists(filePath) && !overwrite)
                {
                    Logger.Logger.Warning($"Theme file already exists: {filePath}");
                    return false;
                }

                var serializer = new XmlSerializer(typeof(Theme));
                using var writer = new FileStream(filePath, FileMode.Create);
                serializer.Serialize(writer, theme);

                Logger.Logger.Info($"Theme saved: {theme.Name} to {filePath}");
                
                // Reload themes to include the new one
                LoadAvailableThemes();
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error saving theme {theme.Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Import a theme from a file
        /// </summary>
        public bool ImportTheme(string filePath)
        {
            try
            {
                var theme = LoadThemeFromFile(filePath);
                if (theme == null)
                {
                    return false;
                }

                return SaveTheme(theme);
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error importing theme from {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Export a theme to a file
        /// </summary>
        public bool ExportTheme(string themeName, string filePath)
        {
            try
            {
                var theme = _availableThemes.FirstOrDefault(t => t.Name == themeName);
                if (theme == null)
                {
                    Logger.Logger.Error($"Theme not found: {themeName}");
                    return false;
                }

                var serializer = new XmlSerializer(typeof(Theme));
                using var writer = new FileStream(filePath, FileMode.Create);
                serializer.Serialize(writer, theme);

                Logger.Logger.Info($"Theme exported: {themeName} to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error exporting theme {themeName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create and save the default theme
        /// </summary>
        private void CreateAndSaveDefaultTheme()
        {
            var defaultTheme = new Theme
            {
                Name = "Default",
                Description = "Default Elite Dangerous inspired theme",
                Author = "ED Inara Overlay",
                Version = "1.0"
            };

            // Add default colors
            defaultTheme.Colors.AddRange(new[]
            {
                new ThemeColor("PrimaryBackgroundColor", "#FF1E1E1E", "Primary background color"),
                new ThemeColor("SecondaryBackgroundColor", "#FF2A2A2A", "Secondary background color"),
                new ThemeColor("HighlightBackgroundColor", "#FF3A3A3A", "Highlight background color"),
                new ThemeColor("BorderColor", "#FF404040", "Border color"),
                new ThemeColor("ButtonBackground", "#FF404040", "Button background color"),
                new ThemeColor("PrimaryColor", "#FF8000", "Primary accent color"),
                new ThemeColor("AccentColor", "#FFCC00", "Accent color"),
                new ThemeColor("SuccessColor", "#00FF00", "Success color"),
                new ThemeColor("FailureColor", "#FF0000", "Failure color"),
                new ThemeColor("SecondaryTextColor", "#404040", "Secondary text color"),
                new ThemeColor("MutedTextColor", "#FFB0B0B0", "Muted text color"),
                new ThemeColor("PrimaryTextColor", "#FF8000", "Primary text color")
            });

            // Add default fonts
            defaultTheme.Fonts.AddRange(new[]
            {
                new ThemeFont("HeaderFont", "Segoe UI", 16, "Bold", "Header font"),
                new ThemeFont("BodyFont", "Segoe UI", 12, "Normal", "Body font"),
                new ThemeFont("ButtonFont", "Segoe UI", 12, "Bold", "Button font")
            });

            // Add default dimensions
            defaultTheme.Dimensions.AddRange(new[]
            {
                new ThemeDimension("HeaderFontSize", 16, "Header font size"),
                new ThemeDimension("BodyFontSize", 12, "Body font size"),
                new ThemeDimension("ButtonHeight", 32, "Button height"),
                new ThemeDimension("BorderRadius", 4, "Border radius")
            });

            SaveTheme(defaultTheme, true);
        }

        /// <summary>
        /// Force re-rendering of all UI elements to apply theme changes
        /// </summary>
        private void ForceUIRerendering()
        {
            try
            {
                Logger.Logger.Info("Forcing UI re-rendering for theme changes");
                
                // Force re-rendering on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Re-render all windows
                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window != null)
                            {
                                RerenderWindow(window);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Error($"Error during UI re-rendering: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error forcing UI re-rendering: {ex.Message}");
            }
        }

        /// <summary>
        /// Re-render a specific window and all its controls
        /// </summary>
        private void RerenderWindow(Window window)
        {
            try
            {
                // Force invalidation of the window
                window.InvalidateVisual();
                window.UpdateLayout();
                
                // Re-render all controls in the window
                RerenderVisualTree(window);
                
                Logger.Logger.Info($"Re-rendered window: {window.GetType().Name}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error re-rendering window {window.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively re-render all elements in the visual tree
        /// </summary>
        private void RerenderVisualTree(DependencyObject parent)
        {
            if (parent == null) return;
            
            try
            {
                // Handle specific control types that need special attention
                switch (parent)
                {
                    case ToggleButton toggleButton:
                        RerenderToggleButton(toggleButton);
                        break;
                    case Button button:
                        RerenderButton(button);
                        break;
                    case ComboBox comboBox:
                        RerenderComboBox(comboBox);
                        break;
                    case FrameworkElement element:
                        element.InvalidateVisual();
                        element.UpdateLayout();
                        break;
                }
                
                // Recursively process child elements
                int childCount = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    RerenderVisualTree(child);
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error re-rendering visual tree element: {ex.Message}");
            }
        }

        /// <summary>
        /// Re-render toggle button with current theme
        /// </summary>
        private void RerenderToggleButton(ToggleButton toggleButton)
        {
            try
            {
                // Force re-evaluation of styles
                toggleButton.InvalidateVisual();
                toggleButton.UpdateLayout();
                
                // Update background based on current state
                if (toggleButton.IsChecked == true)
                {
                    var activeColor = Application.Current.Resources.Contains("HighlightBackgroundColor") 
                        ? (Color)Application.Current.Resources["HighlightBackgroundColor"]
                        : Colors.Gray;
                    toggleButton.Background = new SolidColorBrush(activeColor);
                }
                else
                {
                    var inactiveColor = Application.Current.Resources.Contains("SecondaryBackgroundColor") 
                        ? (Color)Application.Current.Resources["SecondaryBackgroundColor"]
                        : Colors.DarkGray;
                    toggleButton.Background = new SolidColorBrush(inactiveColor);
                }
                
                Logger.Logger.Info($"Re-rendered toggle button: {toggleButton.Name}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error re-rendering toggle button: {ex.Message}");
            }
        }

        /// <summary>
        /// Re-render button with current theme
        /// </summary>
        private void RerenderButton(Button button)
        {
            try
            {
                // Force re-evaluation of styles
                button.InvalidateVisual();
                button.UpdateLayout();
                
                // Apply theme colors if available
                if (Application.Current.Resources.Contains("ButtonBackground"))
                {
                    var buttonColor = (Color)Application.Current.Resources["ButtonBackground"];
                    button.Background = new SolidColorBrush(buttonColor);
                }
                
                if (Application.Current.Resources.Contains("PrimaryTextColor"))
                {
                    var textColor = (Color)Application.Current.Resources["PrimaryTextColor"];
                    button.Foreground = new SolidColorBrush(textColor);
                }
                
                Logger.Logger.Info($"Re-rendered button: {button.Name}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error re-rendering button: {ex.Message}");
            }
        }

        /// <summary>
        /// Re-render ComboBox and its popup with current theme
        /// </summary>
        private void RerenderComboBox(ComboBox comboBox)
        {
            try
            {
                // Force re-evaluation of styles
                comboBox.InvalidateVisual();
                comboBox.UpdateLayout();
                
                // Apply theme colors
                if (Application.Current.Resources.Contains("SecondaryBackgroundColor"))
                {
                    var bgColor = (Color)Application.Current.Resources["SecondaryBackgroundColor"];
                    comboBox.Background = new SolidColorBrush(bgColor);
                }
                
                if (Application.Current.Resources.Contains("PrimaryTextColor"))
                {
                    var textColor = (Color)Application.Current.Resources["PrimaryTextColor"];
                    comboBox.Foreground = new SolidColorBrush(textColor);
                }
                
                if (Application.Current.Resources.Contains("AccentColor"))
                {
                    var accentColor = (Color)Application.Current.Resources["AccentColor"];
                    comboBox.BorderBrush = new SolidColorBrush(accentColor);
                }
                
                // Force re-render of popup if it exists
                var popup = GetComboBoxPopup(comboBox);
                if (popup != null)
                {
                    RerenderPopup(popup);
                }
                
                // Re-render all items
                foreach (var item in comboBox.Items)
                {
                    var container = comboBox.ItemContainerGenerator.ContainerFromItem(item);
                    if (container is ComboBoxItem comboBoxItem)
                    {
                        RerenderComboBoxItem(comboBoxItem);
                    }
                }
                
                Logger.Logger.Info($"Re-rendered ComboBox: {comboBox.Name}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error re-rendering ComboBox: {ex.Message}");
            }
        }

        /// <summary>
        /// Re-render ComboBox popup with current theme
        /// </summary>
        private void RerenderPopup(Popup popup)
        {
            try
            {
                popup.InvalidateVisual();
                
                // Re-render popup child elements
                if (popup.Child != null)
                {
                    RerenderVisualTree(popup.Child);
                }
                
                Logger.Logger.Info("Re-rendered ComboBox popup");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error re-rendering popup: {ex.Message}");
            }
        }

        /// <summary>
        /// Re-render ComboBox item with current theme
        /// </summary>
        private void RerenderComboBoxItem(ComboBoxItem item)
        {
            try
            {
                item.InvalidateVisual();
                item.UpdateLayout();
                
                // Apply theme colors
                if (Application.Current.Resources.Contains("SecondaryBackgroundColor"))
                {
                    var bgColor = (Color)Application.Current.Resources["SecondaryBackgroundColor"];
                    item.Background = new SolidColorBrush(bgColor);
                }
                
                if (Application.Current.Resources.Contains("PrimaryTextColor"))
                {
                    var textColor = (Color)Application.Current.Resources["PrimaryTextColor"];
                    item.Foreground = new SolidColorBrush(textColor);
                }
                
                Logger.Logger.Info($"Re-rendered ComboBox item");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error re-rendering ComboBox item: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the popup from a ComboBox using reflection
        /// </summary>
        private Popup? GetComboBoxPopup(ComboBox comboBox)
        {
            try
            {
                // Get the ComboBox template
                var template = comboBox.Template;
                if (template != null)
                {
                    // Find the popup in the template
                    var popup = template.FindName("PART_Popup", comboBox) as Popup;
                    return popup;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error getting ComboBox popup: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Convert string to FontWeight
        /// </summary>
        private FontWeight GetFontWeight(string weight)
        {
            return weight.ToLower() switch
            {
                "thin" => FontWeights.Thin,
                "light" => FontWeights.Light,
                "normal" => FontWeights.Normal,
                "medium" => FontWeights.Medium,
                "semibold" => FontWeights.SemiBold,
                "bold" => FontWeights.Bold,
                "extrabold" => FontWeights.ExtraBold,
                "black" => FontWeights.Black,
                _ => FontWeights.Normal
            };
        }
    }

    /// <summary>
    /// Event args for theme applied event
    /// </summary>
    public class ThemeAppliedEventArgs : EventArgs
    {
        public Theme Theme { get; }

        public ThemeAppliedEventArgs(Theme theme)
        {
            Theme = theme;
        }
    }
}
