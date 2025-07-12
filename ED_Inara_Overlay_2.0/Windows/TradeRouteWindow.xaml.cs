using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using InaraTools;
using ED_Inara_Overlay_2._0.Utils;

namespace ED_Inara_Overlay_2._0.Windows
{
    /// <summary>
    /// Trade Route Window - allows searching and displaying trade routes
    /// </summary>
    public partial class TradeRouteWindow : Window
    {
        private DispatcherTimer? updateTimer;
        private bool disposed;
        private IntPtr targetWindow;
        private uint targetProcessId;

        public TradeRouteWindow(Window owner)
        {
            Owner = owner;
            InitializeComponent();
            SetupOverlay();
            SetupUpdateTimer();
            
            Logger.Logger.Info("TradeRouteWindow constructor completed");
        }
        
        private void SetupOverlay()
        {
            // Set up overlay behavior when window is loaded
            this.Loaded += (s, e) =>
            {
                WindowsAPI.SetupOverlayWindow(this);
                WindowsAPI.SetClickThrough(this, false); // Allow interaction with controls
                
                Logger.Logger.Info("TradeRouteWindow overlay setup completed");
            };
        }

        private void SetupUpdateTimer()
        {
            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS for smooth positioning
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (disposed || updateTimer == null)
                return;

            // Update window position to stay relative to target window
            if (targetWindow != IntPtr.Zero)
            {
                // Check if target window still exists
                if (!WindowsAPI.IsWindow(targetWindow))
                {
                    this.Hide();
                    return;
                }

                // Update position to follow target window at right center
                try
                {
                    if (WindowsAPI.GetWindowRect(targetWindow, out WindowsAPI.RECT targetRect))
                    {
                        var screenWidth = System.Windows.SystemParameters.WorkArea.Width;
                        var screenHeight = System.Windows.SystemParameters.WorkArea.Height;
                        
                        // Calculate target window dimensions and center
                        double targetHeight = targetRect.Bottom - targetRect.Top;
                        double targetCenterY = targetRect.Top + (targetHeight / 2);
                        
                        // Calculate right center position with gap
                        double gapSize = 10;
                        double newLeft = targetRect.Right - gapSize - this.Width; // Right edge + gap
                        double newTop = targetCenterY - (this.Height / 2); // Vertically centered
                        
                        // Check if window fits on the right side
                        //bool fitsOnRight = (newLeft + this.Width) <= screenWidth;
                        
                        //if (!fitsOnRight)
                        //{
                        //    // Position to the left of target if right doesn't fit
                        //    newLeft = targetRect.Left - this.Width - gapSize;
                        //}
                        
                        //if (newLeft < 0)
                        //{
                        //    newLeft = 10; // Screen edge fallback
                        //}
                        
                        // Vertical bounds checking
                        if (newTop < 0)
                        {
                            newTop = 10;
                        }
                        
                        if (newTop + this.Height > screenHeight)
                        {
                            newTop = screenHeight - this.Height - 10;
                        }
                        
                        this.Left = newLeft;
                        this.Top = newTop;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"Error updating TradeRouteWindow position: {ex.Message}");
                }
                
                // Check target window focus and visibility state
                IntPtr foregroundWindow = WindowsAPI.GetForegroundWindow();
                bool targetHasFocus = (foregroundWindow == targetWindow);
                bool overlayHasFocus = WindowsAPI.IsOverlayWindow(foregroundWindow);
                bool targetMinimized = WindowsAPI.IsIconic(targetWindow);
                bool targetVisible = WindowsAPI.IsWindowVisible(targetWindow);
                
                // This window should be visible when the target window is focused and visible
                // OR when any overlay window has focus
                bool shouldBeVisible = targetVisible && !targetMinimized && (targetHasFocus || overlayHasFocus);
                
                // Set topmost only when target or overlay has focus
                bool shouldBeTopmost = targetHasFocus || overlayHasFocus;
                
                if (!shouldBeVisible && this.IsVisible)
                {
                    Logger.Logger.Info("TradeRouteWindow hiding - target window lost focus or is not visible");
                    this.Hide();
                }
                else if (shouldBeVisible && !this.IsVisible)
                {
                    Logger.Logger.Info("TradeRouteWindow showing - target window has focus and is visible");
                    this.Show();
                }
                
                // Apply topmost state conditionally using WindowInteropHelper
                if (this.IsVisible && this.IsLoaded)
                {
                    WindowsAPI.SetTopmost(this, shouldBeTopmost);
                }
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Searching...";
            Logger.Logger.Info("Initiating trade route search...");

            var searchParams = new TradeRouteSearchParams
            {
                NearStarSystem = NearStarSystemTextBox.Text,
                CargoCapacity = int.Parse(CargoCapacityTextBox.Text),
                MaxRouteDistance = MaxRouteDistanceComboBox.Text,
                MaxPriceAge = MaxPriceAgeComboBox.Text,
                IncludeRoundTrips = IncludeRoundTripsCheckBox.IsChecked == true,
                DisplayPowerplayBonuses = DisplayPowerplayBonusesCheckBox.IsChecked == true,
                // Additional filters
                MinLandingPad = GetComboBoxIndex(MinLandingPadComboBox),
                MaxStationDistance = GetComboBoxIndex(MaxStationDistanceComboBox),
                UseSurfaceStations = GetComboBoxIndex(UseSurfaceStationsComboBox),
                SourceStationPower = GetComboBoxIndex(SourceStationPowerComboBox),
                TargetStationPower = GetComboBoxIndex(TargetStationPowerComboBox),
                MinSupply = GetComboBoxIndex(MinSupplyComboBox),
                MinDemand = GetComboBoxIndex(MinDemandComboBox),
                OrderBy = GetComboBoxIndex(OrderByComboBox)
            };

            try
            {
                var tradeRoutes = await InaraCommunicator.SearchInaraTradeRoutes(searchParams);
                StatusText.Text = $"Found {tradeRoutes.Count} routes";
                Logger.Logger.Info($"Found {tradeRoutes.Count} trade routes.");
                
                // Show results in overlay window
                if (tradeRoutes.Count > 0)
                {
                    ((MainWindow)Owner).ShowResultsOverlay(tradeRoutes);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error during search.";
                Logger.Logger.Error($"Error searching trade routes: {ex.Message}");
            }
        }
        
        private void TestDataButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading test data...";
            Logger.Logger.Info("Loading test data for trade routes...");

            try
            {
                var testRoutes = TestDataGenerator.GenerateTestData();
                StatusText.Text = $"Loaded {testRoutes.Count} test routes";
                Logger.Logger.Info($"Loaded {testRoutes.Count} test trade routes.");
                
                // Show results in overlay window
                ((MainWindow)Owner).ShowResultsOverlay(testRoutes);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading test data.";
                Logger.Logger.Error($"Error loading test data: {ex.Message}");
            }
        }

        private void ShowFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle visibility of additional filters
            if (AdditionalFiltersGroupBox.Visibility == Visibility.Collapsed)
            {
                AdditionalFiltersGroupBox.Visibility = Visibility.Visible;
                ShowFiltersButton.Content = "Hide Additional Filters";
                Logger.Logger.Info("Additional filters shown");
            }
            else
            {
                AdditionalFiltersGroupBox.Visibility = Visibility.Collapsed;
                ShowFiltersButton.Content = "Show Additional Filters";
                Logger.Logger.Info("Additional filters hidden");
            }
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            StopUpdateTimer();
            this.Hide();
            
            // Notify main window if owner is set
            if (Owner is MainWindow mainWindow)
            {
                mainWindow.OnTradeRouteWindowClosed();
            }
        }

        public void SetTargetWindow(IntPtr window, uint pid)
        {
            targetWindow = window;
            targetProcessId = pid;
            
            Logger.Logger.Info($"TradeRouteWindow.SetTargetWindow: window={window}, pid={pid}");
            
            // Position window at a safe visible location
            try
            {
                // Get screen working area to ensure window is visible
                var screenWidth = System.Windows.SystemParameters.WorkArea.Width;
                var screenHeight = System.Windows.SystemParameters.WorkArea.Height;
                
                // Position at right side of screen initially (for testing)
                this.Left = screenWidth - this.Width - 50; // Right side with margin
                this.Top = Math.Max(50, (screenHeight - this.Height) / 2);
                
                Logger.Logger.Info($"TradeRouteWindow positioned at screen right: Left={this.Left}, Top={this.Top}, Screen={screenWidth}x{screenHeight}");
                
                // If we have a target window, calculate right center position
                if (targetWindow != IntPtr.Zero)
                {
                    Logger.Logger.Info($"Target window found: {targetWindow}, attempting relative positioning");
                }
                else
                {
                    Logger.Logger.Info("No target window set, keeping center position");
                }
                
                if (targetWindow != IntPtr.Zero)
                {
                    try
                    {
                        if (WindowsAPI.GetWindowRect(targetWindow, out WindowsAPI.RECT targetRect))
                        {
                            // Calculate target window dimensions
                            double targetWidth = targetRect.Right - targetRect.Left;
                            double targetHeight = targetRect.Bottom - targetRect.Top;
                            double targetCenterY = targetRect.Top + (targetHeight / 2);
                            
                            // Calculate right center position
                            // Position at right edge of target window with small gap, vertically centered
                            double gapSize = 10; // Small gap between target and TradeRouteWindow
                            double newLeft = targetRect.Right - this.Width - gapSize;
                            double newTop = targetCenterY - (this.Height / 2);
                            
                            Logger.Logger.Info($"Calculated right center position: Target=({targetRect.Left},{targetRect.Top},{targetRect.Right},{targetRect.Bottom}), TargetSize={targetWidth}x{targetHeight}, TargetCenter={targetCenterY}, Calculated=({newLeft},{newTop}), WindowSize=({this.Width}x{this.Height})");
                            
                            // Check if window fits on the right side
                            bool fitsOnRight = (newLeft + this.Width) <= screenWidth;
                            Logger.Logger.Info($"Right side fit check: newLeft={newLeft}, windowWidth={this.Width}, total={newLeft + this.Width}, screenWidth={screenWidth}, fitsOnRight={fitsOnRight}");
                            
                            if (!fitsOnRight)
                            {
                                // If right side doesn't fit, try positioning to the left of target
                                newLeft = targetRect.Left - this.Width - gapSize;
                                Logger.Logger.Info($"Right side doesn't fit, trying left side: newLeft={newLeft}");
                            }
                            
                            if (newLeft < 0)
                            {
                                // If left side also doesn't fit, position at screen edge
                                newLeft = 10;
                                Logger.Logger.Info($"Left side doesn't fit either, using screen edge: newLeft={newLeft}");
                            }
                            
                            // Ensure vertical position is on screen
                            if (newTop < 0)
                            {
                                newTop = 10;
                                Logger.Logger.Info($"Top position adjusted to screen: newTop={newTop}");
                            }
                            
                            if (newTop + this.Height > screenHeight)
                            {
                                newTop = screenHeight - this.Height - 10;
                                Logger.Logger.Info($"Bottom position adjusted to screen: newTop={newTop}");
                            }
                            
                            this.Left = newLeft;
                            this.Top = newTop;
                            
                            Logger.Logger.Info($"TradeRouteWindow positioned at right center: Final=({this.Left},{this.Top}), WindowSize=({this.Width},{this.Height}), Screen=({screenWidth},{screenHeight})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Error($"Error calculating right center position: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error positioning TradeRouteWindow: {ex.Message}");
            }
        }

        public void StopUpdateTimer()
        {
            updateTimer?.Stop();
        }

        private int GetComboBoxIndex(ComboBox comboBox)
        {
            if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                return comboBox.SelectedIndex;
            }
            return 0;
        }
        protected override void OnClosed(EventArgs e)
        {
            disposed = true;
            updateTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
