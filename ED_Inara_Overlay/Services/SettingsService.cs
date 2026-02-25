using System;
using System.IO;
using System.Text.Json;
using ED_Inara_Overlay.Models;

namespace ED_Inara_Overlay.Services
{
    /// <summary>
    /// Service for managing application settings persistence
    /// </summary>
    public class SettingsService
    {
        private static SettingsService? _instance;
        private readonly string _settingsFilePath;
        private AppSettings _settings;

        public static SettingsService Instance => _instance ??= new SettingsService();

        /// <summary>
        /// Event fired when settings are changed
        /// </summary>
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        /// <summary>
        /// Current application settings
        /// </summary>
        public AppSettings Settings => _settings;

        private SettingsService()
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ED_Inara_Overlay");
            Directory.CreateDirectory(appDataFolder);
            _settingsFilePath = Path.Combine(appDataFolder, "settings.json");
            _settings = LoadSettings();
        }

        /// <summary>
        /// Load settings from file or create default settings
        /// </summary>
        private AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        Logger.Logger.Info($"Settings loaded from {_settingsFilePath}");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error loading settings: {ex.Message}");
            }

            // Return default settings if loading failed
            Logger.Logger.Info("Using default settings");
            return new AppSettings();
        }

        /// <summary>
        /// Save current settings to file
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(_settings, options);
                File.WriteAllText(_settingsFilePath, json);
                
                Logger.Logger.Info($"Settings saved to {_settingsFilePath}");
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs(_settings));
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the selected theme setting
        /// </summary>
        public void SetSelectedTheme(string themeName)
        {
            if (_settings.SelectedTheme != themeName)
            {
                _settings.SelectedTheme = themeName;
                SaveSettings();
                Logger.Logger.Info($"Selected theme updated to: {themeName}");
            }
        }

        /// <summary>
        /// Get the currently selected theme name
        /// </summary>
        public string GetSelectedTheme()
        {
            return _settings.SelectedTheme;
        }

        /// <summary>
        /// Update global overlay toggle hotkey settings.
        /// </summary>
        public void SetToggleHotkey(string modifiers, string key)
        {
            if (_settings.ToggleHotkeyModifiers != modifiers || _settings.ToggleHotkeyKey != key)
            {
                _settings.ToggleHotkeyModifiers = modifiers;
                _settings.ToggleHotkeyKey = key;
                SaveSettings();
                Logger.Logger.Info($"Toggle hotkey updated to: {modifiers}+{key}");
            }
        }

        /// <summary>
        /// Get configured global overlay toggle hotkey.
        /// </summary>
        public (string Modifiers, string Key) GetToggleHotkey()
        {
            return (_settings.ToggleHotkeyModifiers, _settings.ToggleHotkeyKey);
        }

        /// <summary>
        /// Update interactive overlay mode hotkey settings.
        /// </summary>
        public void SetInteractiveHotkey(string modifiers, string key)
        {
            if (_settings.InteractiveHotkeyModifiers != modifiers || _settings.InteractiveHotkeyKey != key)
            {
                _settings.InteractiveHotkeyModifiers = modifiers;
                _settings.InteractiveHotkeyKey = key;
                SaveSettings();
                Logger.Logger.Info($"Interactive hotkey updated to: {modifiers}+{key}");
            }
        }

        /// <summary>
        /// Get configured interactive mode hotkey.
        /// </summary>
        public (string Modifiers, string Key) GetInteractiveHotkey()
        {
            return (_settings.InteractiveHotkeyModifiers, _settings.InteractiveHotkeyKey);
        }

        /// <summary>
        /// Update interactive mode behavior settings.
        /// </summary>
        public void SetInteractionBehavior(
            bool enableInteractionMode,
            int autoReturnTimeoutSeconds,
            bool returnOnFocusLoss,
            bool showCursorWhenInteractive)
        {
            if (_settings.EnableInteractionMode == enableInteractionMode
                && _settings.AutoReturnTimeoutSeconds == autoReturnTimeoutSeconds
                && _settings.ReturnOnFocusLoss == returnOnFocusLoss
                && _settings.ShowCursorWhenInteractive == showCursorWhenInteractive)
            {
                return;
            }

            _settings.EnableInteractionMode = enableInteractionMode;
            _settings.AutoReturnTimeoutSeconds = autoReturnTimeoutSeconds;
            _settings.ReturnOnFocusLoss = returnOnFocusLoss;
            _settings.ShowCursorWhenInteractive = showCursorWhenInteractive;
            SaveSettings();
            Logger.Logger.Info(
                $"Interaction settings updated: enabled={enableInteractionMode}, timeout={autoReturnTimeoutSeconds}s, returnOnFocusLoss={returnOnFocusLoss}, showCursor={showCursorWhenInteractive}");
        }

        /// <summary>
        /// Reset settings to default values
        /// </summary>
        public void ResetToDefaults()
        {
            _settings = new AppSettings();
            SaveSettings();
            Logger.Logger.Info("Settings reset to defaults");
        }
    }

    /// <summary>
    /// Application settings model
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Currently selected theme name
        /// </summary>
        public string SelectedTheme { get; set; } = "Default";

        /// <summary>
        /// Application version when settings were last saved
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Timestamp when settings were last saved
        /// </summary>
        public DateTime LastSaved { get; set; } = DateTime.Now;

        /// <summary>
        /// Global overlay toggle hotkey modifiers (for example: Ctrl, Ctrl+Shift).
        /// </summary>
        public string ToggleHotkeyModifiers { get; set; } = "Ctrl";

        /// <summary>
        /// Global overlay toggle hotkey key value (WPF Key enum string, for example: D5, F1).
        /// </summary>
        public string ToggleHotkeyKey { get; set; } = "D5";

        /// <summary>
        /// Enables/disables entering interactive mode for overlay windows.
        /// </summary>
        public bool EnableInteractionMode { get; set; } = true;

        /// <summary>
        /// Auto-return timeout from interactive mode in seconds. 0 disables timeout.
        /// </summary>
        public int AutoReturnTimeoutSeconds { get; set; } = 8;

        /// <summary>
        /// Return to passive mode when interactive overlay focus is lost.
        /// </summary>
        public bool ReturnOnFocusLoss { get; set; } = true;

        /// <summary>
        /// Force cursor visibility when interactive mode is enabled.
        /// </summary>
        public bool ShowCursorWhenInteractive { get; set; } = true;

        /// <summary>
        /// Interactive mode hotkey modifiers.
        /// </summary>
        public string InteractiveHotkeyModifiers { get; set; } = "Ctrl";

        /// <summary>
        /// Interactive mode hotkey key value.
        /// </summary>
        public string InteractiveHotkeyKey { get; set; } = "D6";
    }

    /// <summary>
    /// Event args for settings changed event
    /// </summary>
    public class SettingsChangedEventArgs : EventArgs
    {
        public AppSettings Settings { get; }

        public SettingsChangedEventArgs(AppSettings settings)
        {
            Settings = settings;
        }
    }
}
