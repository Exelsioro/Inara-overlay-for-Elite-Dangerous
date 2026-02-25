using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ED_Inara_Overlay.Services;
using System.Collections.Generic;
using System.Linq;

namespace ED_Inara_Overlay.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private sealed class HotkeyOption
        {
            public string Label { get; init; } = string.Empty;
            public string Value { get; init; } = string.Empty;
        }

        private static readonly List<HotkeyOption> ModifierOptions = new()
        {
            new HotkeyOption { Label = "Ctrl", Value = "Ctrl" },
            new HotkeyOption { Label = "Ctrl + Shift", Value = "Ctrl+Shift" },
            new HotkeyOption { Label = "Ctrl + Alt", Value = "Ctrl+Alt" },
            new HotkeyOption { Label = "Alt + Shift", Value = "Alt+Shift" },
            new HotkeyOption { Label = "Alt", Value = "Alt" },
            new HotkeyOption { Label = "Shift", Value = "Shift" }
        };

        private static readonly List<HotkeyOption> KeyOptions = new()
        {
            new HotkeyOption { Label = "0", Value = "D0" },
            new HotkeyOption { Label = "1", Value = "D1" },
            new HotkeyOption { Label = "2", Value = "D2" },
            new HotkeyOption { Label = "3", Value = "D3" },
            new HotkeyOption { Label = "4", Value = "D4" },
            new HotkeyOption { Label = "5", Value = "D5" },
            new HotkeyOption { Label = "6", Value = "D6" },
            new HotkeyOption { Label = "7", Value = "D7" },
            new HotkeyOption { Label = "8", Value = "D8" },
            new HotkeyOption { Label = "9", Value = "D9" },
            new HotkeyOption { Label = "F1", Value = "F1" },
            new HotkeyOption { Label = "F2", Value = "F2" },
            new HotkeyOption { Label = "F3", Value = "F3" },
            new HotkeyOption { Label = "F4", Value = "F4" },
            new HotkeyOption { Label = "F5", Value = "F5" },
            new HotkeyOption { Label = "F6", Value = "F6" },
            new HotkeyOption { Label = "F7", Value = "F7" },
            new HotkeyOption { Label = "F8", Value = "F8" },
            new HotkeyOption { Label = "F9", Value = "F9" },
            new HotkeyOption { Label = "F10", Value = "F10" },
            new HotkeyOption { Label = "F11", Value = "F11" },
            new HotkeyOption { Label = "F12", Value = "F12" }
        };

        private static readonly List<HotkeyOption> TimeoutOptions = new()
        {
            new HotkeyOption { Label = "Off", Value = "0" },
            new HotkeyOption { Label = "5 sec", Value = "5" },
            new HotkeyOption { Label = "8 sec", Value = "8" },
            new HotkeyOption { Label = "10 sec", Value = "10" },
            new HotkeyOption { Label = "15 sec", Value = "15" }
        };

        public SettingsWindow()
        {
            InitializeComponent();
            LoadThemes();
            LoadHotkeySettings();
        }

        private void LoadThemes()
        {
            ThemeComboBox.ItemsSource = ThemeManager.Instance.AvailableThemes;
            
            // Select the currently applied theme
            var currentTheme = ThemeManager.Instance.CurrentTheme;
            if (currentTheme != null)
            {
                ThemeComboBox.SelectedItem = currentTheme;
            }
            else if (ThemeComboBox.Items.Count > 0)
            {
                ThemeComboBox.SelectedIndex = 0;
            }
            
            UpdateThemeDetails();
            UpdateColorSwatches();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateThemeDetails();
            UpdateColorSwatches();
            
            // Apply theme in real-time for preview
            var selectedTheme = ThemeComboBox.SelectedItem as Models.Theme;
            if (selectedTheme != null)
            {
                ThemeManager.Instance.ApplyTheme(selectedTheme);
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SaveHotkeySettings();

            var selectedTheme = ThemeComboBox.SelectedItem as Models.Theme;
            if (selectedTheme != null)
            {
                ThemeManager.Instance.ApplyTheme(selectedTheme);
                MessageBox.Show($"Applied theme: {selectedTheme.Name}", "Theme Applied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Settings applied.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ImportThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Import Theme"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (ThemeManager.Instance.ImportTheme(openFileDialog.FileName))
                {
                    MessageBox.Show("Theme imported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadThemes();
                }
                else
                {
                    MessageBox.Show("Failed to import theme.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTheme = ThemeComboBox.SelectedItem as Models.Theme;
            if (selectedTheme == null)
            {
                MessageBox.Show("Please select a theme to export.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Export Theme",
                FileName = selectedTheme.Name + ".xml"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                if (ThemeManager.Instance.ExportTheme(selectedTheme.Name, saveFileDialog.FileName))
                {
                    MessageBox.Show("Theme exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to export theme.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshThemesButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.LoadAvailableThemes();
            LoadThemes();
            MessageBox.Show("Themes refreshed successfully!", "Refresh Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateThemeDetails()
        {
            try
            {
                if (ThemeComboBox.SelectedItem is Models.Theme selectedTheme)
                {
                    ThemeNameText.Text = selectedTheme.Name ?? "Unknown";
                    ThemeDescriptionText.Text = selectedTheme.Description ?? "No description available";
                    ThemeAuthorText.Text = $"Author: {selectedTheme.Author ?? "Unknown"}";
                }
                else
                {
                    ThemeNameText.Text = "No theme selected";
                    ThemeDescriptionText.Text = "Select a theme to view details";
                    ThemeAuthorText.Text = "Author: Unknown";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating theme details: {ex.Message}");
            }
        }

        private void UpdateColorSwatches()
        {
            try
            {
                ColorSwatchGrid.Children.Clear();
                if (ThemeComboBox.SelectedItem is Models.Theme selectedTheme && selectedTheme.Colors != null)
                {
                    int column = 0;
                    foreach (var color in selectedTheme.Colors)
                    {
                        if (column >= ColorSwatchGrid.ColumnDefinitions.Count)
                        {
                            // Only show a limited number of swatches for brevity
                            break;
                        }

                        try
                        {
                            var colorValue = (Color)ColorConverter.ConvertFromString(color.Value);
                            var border = new Border
                            {
                                Background = new SolidColorBrush(colorValue),
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = new Thickness(1),
                                Width = 40,
                                Height = 40,
                                Margin = new Thickness(5)
                            };

                            var textBlock = new TextBlock
                            {
                                Text = color.Key,
                                Foreground = new SolidColorBrush(Colors.White),
                                FontSize = 8,
                                FontWeight = FontWeights.Bold,
                                TextAlignment = TextAlignment.Center
                            };

                            var stackPanel = new StackPanel
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            stackPanel.Children.Add(border);
                            stackPanel.Children.Add(textBlock);

                            Grid.SetColumn(stackPanel, column);
                            ColorSwatchGrid.Children.Add(stackPanel);

                            column++;
                        }
                        catch (Exception ex)
                        {
                            // Skip invalid colors
                            System.Diagnostics.Debug.WriteLine($"Error parsing color {color.Key}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating color swatches: {ex.Message}");
            }
        }

        private void LoadHotkeySettings()
        {
            HotkeyModifierComboBox.ItemsSource = ModifierOptions;
            HotkeyModifierComboBox.DisplayMemberPath = nameof(HotkeyOption.Label);

            HotkeyKeyComboBox.ItemsSource = KeyOptions;
            HotkeyKeyComboBox.DisplayMemberPath = nameof(HotkeyOption.Label);

            InteractiveModifierComboBox.ItemsSource = ModifierOptions;
            InteractiveModifierComboBox.DisplayMemberPath = nameof(HotkeyOption.Label);

            InteractiveKeyComboBox.ItemsSource = KeyOptions;
            InteractiveKeyComboBox.DisplayMemberPath = nameof(HotkeyOption.Label);

            TimeoutComboBox.ItemsSource = TimeoutOptions;
            TimeoutComboBox.DisplayMemberPath = nameof(HotkeyOption.Label);

            var settings = SettingsService.Instance.Settings;
            var (modifiers, key) = SettingsService.Instance.GetToggleHotkey();
            var (interactiveModifiers, interactiveKey) = SettingsService.Instance.GetInteractiveHotkey();

            HotkeyModifierComboBox.SelectedItem = ModifierOptions.FirstOrDefault(o => o.Value == modifiers) ?? ModifierOptions[0];
            HotkeyKeyComboBox.SelectedItem = KeyOptions.FirstOrDefault(o => o.Value == key) ?? KeyOptions.FirstOrDefault(o => o.Value == "D5");
            InteractiveModifierComboBox.SelectedItem = ModifierOptions.FirstOrDefault(o => o.Value == interactiveModifiers) ?? ModifierOptions[0];
            InteractiveKeyComboBox.SelectedItem = KeyOptions.FirstOrDefault(o => o.Value == interactiveKey) ?? KeyOptions.FirstOrDefault(o => o.Value == "D6");

            EnableInteractionModeCheckBox.IsChecked = settings.EnableInteractionMode;
            ReturnOnFocusLossCheckBox.IsChecked = settings.ReturnOnFocusLoss;
            ShowCursorWhenInteractiveCheckBox.IsChecked = settings.ShowCursorWhenInteractive;
            TimeoutComboBox.SelectedItem = TimeoutOptions.FirstOrDefault(o => o.Value == settings.AutoReturnTimeoutSeconds.ToString()) ?? TimeoutOptions.FirstOrDefault(o => o.Value == "8");
        }

        private void SaveHotkeySettings()
        {
            if (HotkeyModifierComboBox.SelectedItem is not HotkeyOption modifierOption ||
                HotkeyKeyComboBox.SelectedItem is not HotkeyOption keyOption)
            {
                return;
            }

            SettingsService.Instance.SetToggleHotkey(modifierOption.Value, keyOption.Value);

            if (InteractiveModifierComboBox.SelectedItem is HotkeyOption interactiveModifierOption
                && InteractiveKeyComboBox.SelectedItem is HotkeyOption interactiveKeyOption)
            {
                SettingsService.Instance.SetInteractiveHotkey(interactiveModifierOption.Value, interactiveKeyOption.Value);
            }

            bool enableInteractionMode = EnableInteractionModeCheckBox.IsChecked == true;
            bool returnOnFocusLoss = ReturnOnFocusLossCheckBox.IsChecked == true;
            bool showCursor = ShowCursorWhenInteractiveCheckBox.IsChecked == true;
            int timeout = 8;
            if (TimeoutComboBox.SelectedItem is HotkeyOption timeoutOption
                && int.TryParse(timeoutOption.Value, out int parsedTimeout))
            {
                timeout = parsedTimeout;
            }

            SettingsService.Instance.SetInteractionBehavior(enableInteractionMode, timeout, returnOnFocusLoss, showCursor);
        }
    }
}
