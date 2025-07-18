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
