using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utils.Config
{
    public class UserConfig
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "Default";

        [JsonPropertyName("window_positions")]
        public WindowPositions WindowPositions { get; set; } = new WindowPositions();

        [JsonPropertyName("overlay_settings")]
        public OverlaySettings OverlaySettings { get; set; } = new OverlaySettings();
    }

    public class WindowPositions
    {
        [JsonPropertyName("main_window")]
        public WindowPosition MainWindow { get; set; } = new WindowPosition();

        [JsonPropertyName("trade_route_window")]
        public WindowPosition TradeRouteWindow { get; set; } = new WindowPosition();
    }

    public class WindowPosition
    {
        [JsonPropertyName("x")]
        public double X { get; set; } = double.NaN;

        [JsonPropertyName("y")]
        public double Y { get; set; } = double.NaN;

        [JsonPropertyName("width")]
        public double Width { get; set; } = double.NaN;

        [JsonPropertyName("height")]
        public double Height { get; set; } = double.NaN;
    }

    public class OverlaySettings
    {
        [JsonPropertyName("opacity")]
        public double Opacity { get; set; } = 1.0;

        [JsonPropertyName("always_on_top")]
        public bool AlwaysOnTop { get; set; } = true;

        [JsonPropertyName("click_through")]
        public bool ClickThrough { get; set; } = false;
    }

    public static class ConfigManager
    {
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "ED_Inara_Overlay");
        
        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");
        private static UserConfig _config = new UserConfig();
        private static readonly object _lock = new object();

        public static UserConfig Config => _config;

        static ConfigManager()
        {
            LoadConfig();
        }

        public static void LoadConfig()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        string json = File.ReadAllText(ConfigFilePath);
                        var loadedConfig = JsonSerializer.Deserialize<UserConfig>(json);
                        if (loadedConfig != null)
                        {
                            _config = loadedConfig;
                            Logger.Logger.Info($"Configuration loaded from {ConfigFilePath}");
                        }
                        else
                        {
                            Logger.Logger.Warning("Failed to deserialize config, using defaults");
                            _config = new UserConfig();
                        }
                    }
                    else
                    {
                        Logger.Logger.Info("No config file found, using default configuration");
                        _config = new UserConfig();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"Error loading config: {ex.Message}");
                    _config = new UserConfig();
                }
            }
        }

        public static void SaveConfig()
        {
            lock (_lock)
            {
                try
                {
                    // Ensure directory exists
                    Directory.CreateDirectory(ConfigDirectory);

                    var options = new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    };

                    string json = JsonSerializer.Serialize(_config, options);
                    File.WriteAllText(ConfigFilePath, json);
                    Logger.Logger.Info($"Configuration saved to {ConfigFilePath}");
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"Error saving config: {ex.Message}");
                }
            }
        }

        public static void SetTheme(string theme)
        {
            lock (_lock)
            {
                _config.Theme = theme;
                SaveConfig();
            }
        }

        public static string GetTheme()
        {
            lock (_lock)
            {
                return _config.Theme;
            }
        }

        public static void SetWindowPosition(string windowName, double x, double y, double width, double height)
        {
            lock (_lock)
            {
                WindowPosition position = windowName.ToLower() switch
                {
                    "main" => _config.WindowPositions.MainWindow,
                    "traderoute" => _config.WindowPositions.TradeRouteWindow,
                    _ => _config.WindowPositions.MainWindow
                };

                position.X = x;
                position.Y = y;
                position.Width = width;
                position.Height = height;

                SaveConfig();
            }
        }

        public static WindowPosition GetWindowPosition(string windowName)
        {
            lock (_lock)
            {
                return windowName.ToLower() switch
                {
                    "main" => _config.WindowPositions.MainWindow,
                    "traderoute" => _config.WindowPositions.TradeRouteWindow,
                    _ => _config.WindowPositions.MainWindow
                };
            }
        }
    }
}
