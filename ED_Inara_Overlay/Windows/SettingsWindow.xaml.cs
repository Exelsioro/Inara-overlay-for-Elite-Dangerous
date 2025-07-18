using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ED_Inara_Overlay.Services;

namespace ED_Inara_Overlay.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadThemes();
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
            var selectedTheme = ThemeComboBox.SelectedItem as Models.Theme;
            if (selectedTheme != null)
            {
                ThemeManager.Instance.ApplyTheme(selectedTheme);
                MessageBox.Show($"Applied theme: {selectedTheme.Name}", "Theme Applied", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
