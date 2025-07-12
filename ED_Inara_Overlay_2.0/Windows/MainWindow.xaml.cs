using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Collections.Generic;
using ED_Inara_Overlay_2._0.Utils;
using ED_Inara_Overlay_2._0.Windows;
using System.Diagnostics;
using InaraTools;

namespace ED_Inara_Overlay_2._0
{
    /// <summary>
    /// Main overlay window - equivalent to OverlayForm in the Windows Forms version
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum OverlayState { Waiting, ForceShow, Auto };
        
        private IntPtr targetWindow;
        private uint targetProcessId;
        private DispatcherTimer? updateTimer;
        private bool disposed;
        private TradeRouteWindow? tradeRouteWindow;
        private ResultsOverlayWindow? resultsOverlayWindow;
        private PinnedRouteOverlay? pinnedRouteOverlay;
        private bool isToggleActive = false;
        private bool isResultsActive = false;
        private bool isPinnedRouteActive = false;
        private bool forceVisible = false; // Flag to ensure visibility after target detection
        private OverlayState currentState = OverlayState.Waiting;

        public MainWindow(string processName = "notepad", Process? foundProcess = null)
        {
            InitializeComponent();
            
            // Start hidden - only show when target has focus
            this.Visibility = Visibility.Hidden;
            
            Logger.Logger.Info($"Initializing MainWindow for process: {processName}");
            if(foundProcess != null)
            {
                targetProcessId = (uint)foundProcess.Id;
                targetWindow = WindowsAPI.FindWindowByPID(targetProcessId);
                Logger.Logger.Info($"Found target process {processName} with PID {targetProcessId}");
            }
            else
            {
                // Find target process with retry mechanism
                FindTargetProcessWithRetry(processName);
            }
            SetupOverlay();
            SetupUpdateTimer();
            
            Logger.Logger.Info("MainWindow initialization complete - starting hidden");
        }

        private void FindTargetProcessWithRetry(string processName)
        {
            var process = WindowsAPI.FindProcessByName(processName);
            if (process != null)
            {
                targetProcessId = (uint)process.Id;
                targetWindow = WindowsAPI.FindWindowByPID(targetProcessId);
                if (targetWindow != IntPtr.Zero)
                {
                    Logger.Logger.Info($"Found target process {processName} with PID {targetProcessId} and window handle {targetWindow} immediately");
                    return;
                }
                Logger.Logger.Info($"Found target process {processName} with PID {targetProcessId} but no window handle yet");
            }
            else
            {
                Logger.Logger.Info($"Target process {processName} not found initially");
            }
            
            SetupRetryTimer(processName);
        }
        
        private DispatcherTimer? retryTimer;
        private int retryAttempts = 0;
        private string retryProcessName = "";
        private const int maxRetryAttempts = 20; // 10 seconds with 500ms intervals
        
        private void SetupRetryTimer(string processName)
        {
            retryProcessName = processName;
            retryAttempts = 0;
            
            retryTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            retryTimer.Tick += RetryTimer_Tick;
            retryTimer.Start();
            
            Logger.Logger.Info($"Started retry timer for target process {processName}");
        }
        
        private void RetryTimer_Tick(object? sender, EventArgs e)
        {
            retryAttempts++;
            
            var process = WindowsAPI.FindProcessByName(retryProcessName);
            if (process != null)
            {
                targetProcessId = (uint)process.Id;
                targetWindow = WindowsAPI.FindWindowByPID(targetProcessId);
                
                if (targetWindow != IntPtr.Zero)
                {
                    Logger.Logger.Info($"Found target process {retryProcessName} with PID {targetProcessId} and window handle {targetWindow} on retry attempt {retryAttempts}");
                    
                    // Stop the retry timer
                    retryTimer?.Stop();
                    retryTimer = null;
                    
                    // Force visibility after successful detection
                    EnsureVisibleAfterTargetDetection();
                    return;
                }
                else
                {
                    Logger.Logger.Info($"Found target process {retryProcessName} with PID {targetProcessId} but no window handle yet on retry attempt {retryAttempts}");
                }
            }
            else
            {
                Logger.Logger.Info($"Target process {retryProcessName} not found on retry attempt {retryAttempts}");
            }
            
            // Stop retrying after max attempts
            if (retryAttempts >= maxRetryAttempts)
            {
                Logger.Logger.Info($"Target process {retryProcessName} not found after {maxRetryAttempts} retry attempts - giving up");
                retryTimer?.Stop();
                retryTimer = null;
            }
        }

        private void SetupOverlay()
        {
            // Set up overlay behavior when window is loaded
            this.Loaded += (s, e) =>
            {
                try
                {
                    WindowsAPI.SetupOverlayWindow(this);
                    WindowsAPI.SetClickThrough(this, false); // Allow interaction with toggle button
                    
                    // Position at a safe location initially
                    var screenWidth = System.Windows.SystemParameters.WorkArea.Width;
                    var screenHeight = System.Windows.SystemParameters.WorkArea.Height;
                    
                    // Default safe position (bottom-left of screen)
                    this.Left = 50;
                    this.Top = screenHeight - this.Height - 50;
                    
                    Logger.Logger.Info($"MainWindow positioned at safe location: Left={this.Left}, Top={this.Top}, Screen={screenWidth}x{screenHeight}");
                    
                    // If target is set, try relative positioning with bounds checking
                    if (targetWindow != IntPtr.Zero)
                    {
                        try
                        {
                            // Use new WindowInteropHelper positioning method
                            WindowsAPI.PositionWindowRelativeToTarget(this, targetWindow, RelativePosition.BottomLeft);
                            Logger.Logger.Info($"MainWindow repositioned relative to target: Left={this.Left}, Top={this.Top}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Error($"Error positioning MainWindow relative to target: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"Error setting up overlay: {ex.Message}");
                }
            };
        }

        private void SetupUpdateTimer()
        {
            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (disposed || updateTimer == null)
                return;

            if (targetWindow != IntPtr.Zero)
            {
                // State machine transition from Waiting to ForceShow on target detection
                if (currentState == OverlayState.Waiting)
                {
                    currentState = OverlayState.ForceShow;
                    Logger.Logger.Info("State transition: Waiting -> ForceShow");
                }
                
                // Check if target window still exists
                if (!WindowsAPI.IsWindow(targetWindow))
                {
                    // If target window is completely gone, reset state and close entire application
                    Logger.Logger.Info("Target window no longer exists - shutting down application");
                    ShutdownApplication("Target window closed");
                    return;
                }
                
                // Additional check: verify the target process is still running
                if (!IsTargetProcessRunning())
                {
                    Logger.Logger.Info("Target process is no longer running - shutting down application");
                    ShutdownApplication("Target process terminated");
                    return;
                }

                // Update position to follow target window with bounds checking
                try
                {
                    // Use new WindowInteropHelper positioning method
                    WindowsAPI.PositionWindowRelativeToTarget(this, targetWindow, RelativePosition.BottomLeft);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Error($"Error updating MainWindow position: {ex.Message}");
                }
                
                // Check target window focus and visibility state
                IntPtr foregroundWindow = WindowsAPI.GetForegroundWindow();
                bool targetHasFocus = (foregroundWindow == targetWindow);
                bool overlayHasFocus = WindowsAPI.IsOverlayWindow(foregroundWindow);
                bool targetMinimized = WindowsAPI.IsIconic(targetWindow);
                bool targetVisible = WindowsAPI.IsWindowVisible(targetWindow);
                
                // Determine if main overlay should be visible based on state machine
                bool shouldBeVisible = targetVisible && !targetMinimized;
                
                // Determine if overlay should be topmost based on focus and state
                bool shouldBeTopmost = false;
                
                if (currentState == OverlayState.ForceShow)
                {
                    shouldBeTopmost = targetHasFocus || overlayHasFocus;
                }
                else if (currentState == OverlayState.Auto)
                {
                    shouldBeVisible = shouldBeVisible && (targetHasFocus || overlayHasFocus);
                    shouldBeTopmost = targetHasFocus || overlayHasFocus;
                }
                
                if (this.IsVisible && this.IsLoaded)
                {
                    WindowsAPI.SetTopmost(this, shouldBeTopmost);
                }
                
                if (forceVisible && targetVisible && !targetMinimized)
                {
                    shouldBeVisible = true;
                    Logger.Logger.Info("Using forceVisible flag for initial display");
                }
                
                if (shouldBeVisible && !this.IsVisible)
                {
                    Logger.Logger.Info($"MainWindow showing - state: {currentState}, targetVisible: {targetVisible}, targetFocus: {targetHasFocus}, overlayFocus: {overlayHasFocus}, forced: {forceVisible}");
                    this.Visibility = Visibility.Visible;
                    this.WindowState = WindowState.Normal;
                    this.Show();
                    
                    if (forceVisible)
                    {
                        forceVisible = false;
                        Logger.Logger.Info("Resetting forceVisible flag after successful show");
                    }
                    
                    if (isToggleActive && tradeRouteWindow != null && !tradeRouteWindow.IsVisible)
                    {
                        tradeRouteWindow.Show();
                    }
                    
                    if (isResultsActive && resultsOverlayWindow != null && !resultsOverlayWindow.IsVisible)
                    {
                        resultsOverlayWindow.Show();
                    }
                }
                else if (!shouldBeVisible && this.IsVisible)
                {
                    Logger.Logger.Info($"MainWindow hiding - state: {currentState}, targetFocus: {targetHasFocus}, overlayFocus: {overlayHasFocus}");
                    this.Hide();
                    this.Visibility = Visibility.Hidden;
                    
                    if (tradeRouteWindow != null && tradeRouteWindow.IsVisible)
                    {
                        tradeRouteWindow.Hide();
                    }
                    
                    if (resultsOverlayWindow != null && resultsOverlayWindow.IsVisible)
                    {
                        resultsOverlayWindow.Hide();
                    }
                }
                
                if (currentState == OverlayState.ForceShow && this.IsVisible)
                {
                    if (!targetHasFocus || overlayHasFocus)
                    {
                        currentState = OverlayState.Auto;
                        Logger.Logger.Info("State transition: ForceShow -> Auto (focus change detected)");
                    }
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isToggleActive = !isToggleActive;
            UpdateToggleButtonState();
            
            Logger.Logger.Info($"Toggle button state changed: {(isToggleActive ? "Active" : "Inactive")}");
            Logger.Logger.LogUserAction($"Toggle button clicked", new { IsActive = isToggleActive });
            
            if (isToggleActive)
            {
                // Show trade route window
                if (tradeRouteWindow == null || !tradeRouteWindow.IsLoaded)
                {
                    Logger.Logger.Info("Creating new TradeRouteWindow instance");
                    tradeRouteWindow = new TradeRouteWindow(this);
                    Logger.Logger.Info($"TradeRouteWindow created, IsLoaded: {tradeRouteWindow.IsLoaded}");
                    
                    // Set target window for positioning
                    tradeRouteWindow.SetTargetWindow(targetWindow, targetProcessId);
                    Logger.Logger.Info($"TradeRouteWindow target set, IsLoaded: {tradeRouteWindow.IsLoaded}");
                }
                
                if (!tradeRouteWindow.IsVisible)
                {
                    Logger.Logger.Info($"Showing TradeRouteWindow, targetWindow: {targetWindow}, targetProcessId: {targetProcessId}");
                    tradeRouteWindow.SetTargetWindow(targetWindow, targetProcessId);
                    tradeRouteWindow.Show();
                    Logger.Logger.Info($"TradeRouteWindow.Show() called, IsVisible: {tradeRouteWindow.IsVisible}");
                }
            }
            else
            {
                // Hide trade route window
                if (tradeRouteWindow != null && tradeRouteWindow.IsVisible)
                {
                    Logger.Logger.Info("Hiding TradeRouteWindow");
                    tradeRouteWindow.StopUpdateTimer();
                    tradeRouteWindow.Hide();
                }
            }
        }

        private void UpdateToggleButtonState()
        {
            ToggleButton.Background = isToggleActive ?
                new SolidColorBrush(Color.FromArgb(20, 128, 128, 128)) : 
                new SolidColorBrush(Color.FromArgb(20, 20, 20, 0)); 
        }

        public void OnTradeRouteWindowClosed()
        {
            isToggleActive = false;
            UpdateToggleButtonState();
        }
        
        public void OnResultsWindowClosed()
        {
            isResultsActive = false;
            Logger.Logger.Info("Results overlay window closed");
        }
        
        public void OnPinRouteRequested(TradeRoute tradeRoute)
        {
            Logger.Logger.Info($"Pin route requested from MainWindow: {tradeRoute.CardHeader.FromStation.System} -> {tradeRoute.CardHeader.ToStation.System}");
            
            // Create pinned route overlay if it doesn't exist
            if (pinnedRouteOverlay == null || !pinnedRouteOverlay.IsLoaded)
            {
                Logger.Logger.Info("Creating new PinnedRouteOverlay instance");
                pinnedRouteOverlay = new PinnedRouteOverlay(this);
            }
            
            // Set target window for positioning
            pinnedRouteOverlay.SetTargetWindow(targetWindow, targetProcessId);
            
            // Pin the route
            pinnedRouteOverlay.PinTradeRoute(tradeRoute);
            
            isPinnedRouteActive = true;
            
            // Close other overlay windows as requested
            CloseOverlayWindows();
            
            Logger.Logger.Info($"Route pinned successfully, closing other overlays");
        }
        
        private void CloseOverlayWindows()
        {
            Logger.Logger.Info("Closing overlay windows for pin route functionality");
            
            // Hide and close trade route window
            if (tradeRouteWindow != null && tradeRouteWindow.IsVisible)
            {
                Logger.Logger.Info("Closing TradeRouteWindow");
                tradeRouteWindow.StopUpdateTimer();
                tradeRouteWindow.Hide();
                isToggleActive = false;
                UpdateToggleButtonState();
            }
            
            // Properly close results overlay window
            if (resultsOverlayWindow != null && resultsOverlayWindow.IsVisible)
            {
                Logger.Logger.Info("Closing ResultsOverlayWindow");
                
                // Stop the update timer properly
                resultsOverlayWindow.StopUpdateTimer();
                
                // Clear trade route controls and events
                resultsOverlayWindow.Dispose();
                
                // Hide and reset state
                resultsOverlayWindow.Hide();
                isResultsActive = false;
                
                // Nullify the reference so a new instance will be created next time
                resultsOverlayWindow = null;
                
                Logger.Logger.Info("ResultsOverlayWindow closed and state reset");
            }
            
            Logger.Logger.Info("All overlay windows closed");
        }
        
        /// <summary>
        /// Checks if the target process is still running
        /// </summary>
        private bool IsTargetProcessRunning()
        {
            try
            {
                if (targetProcessId == 0)
                    return false;
                    
                var process = Process.GetProcessById((int)targetProcessId);
                return !process.HasExited;
            }
            catch (ArgumentException)
            {
                // Process not found
                return false;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"Error checking target process status: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Initiates application shutdown with proper cleanup
        /// </summary>
        private void ShutdownApplication(string reason)
        {
            Logger.Logger.Info($"Initiating application shutdown: {reason}");
            
            isToggleActive = false;
            UpdateToggleButtonState();
            
            // Close all overlay windows first
            CloseAllOverlayWindows();
            
            // Schedule application shutdown on the UI thread
            this.Dispatcher.BeginInvoke(new Action(() => 
            {
                Logger.Logger.Info($"Application shutdown initiated: {reason}");
                Application.Current.Shutdown();
            }));
        }
        
        /// <summary>
        /// Closes all overlay windows when target window is closed
        /// </summary>
        private void CloseAllOverlayWindows()
        {
            Logger.Logger.Info("Closing all overlay windows due to target window closure");
            
            try
            {
                // Stop the main update timer first to prevent further updates
                if (updateTimer != null)
                {
                    updateTimer.Stop();
                    Logger.Logger.Info("Main update timer stopped");
                }
                
                // Close trade route window
                if (tradeRouteWindow != null)
                {
                    Logger.Logger.Info("Closing TradeRouteWindow");
                    try
                    {
                        tradeRouteWindow.StopUpdateTimer();
                        tradeRouteWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Warning($"Error closing TradeRouteWindow: {ex.Message}");
                    }
                    tradeRouteWindow = null;
                }
                
                // Close results overlay window
                if (resultsOverlayWindow != null)
                {
                    Logger.Logger.Info("Closing ResultsOverlayWindow");
                    try
                    {
                        resultsOverlayWindow.StopUpdateTimer();
                        resultsOverlayWindow.Close();
                        resultsOverlayWindow.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Warning($"Error closing ResultsOverlayWindow: {ex.Message}");
                    }
                    resultsOverlayWindow = null;
                }
                
                // Close pinned route overlay
                if (pinnedRouteOverlay != null)
                {
                    Logger.Logger.Info("Closing PinnedRouteOverlay");
                    try
                    {
                        pinnedRouteOverlay.Close();
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Warning($"Error closing PinnedRouteOverlay: {ex.Message}");
                    }
                    pinnedRouteOverlay = null;
                }
                
                // Reset all state flags
                isToggleActive = false;
                isResultsActive = false;
                isPinnedRouteActive = false;
                
                Logger.Logger.Info("All overlay windows closed successfully");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error during overlay windows cleanup: {ex.Message}");
            }
        }
        
        public void ShowResultsOverlay(List<TradeRoute> tradeRoutes)
        {
            // Check if we need to create a new instance (first time or after disposal)
            bool needNewInstance = resultsOverlayWindow == null || 
                                   !resultsOverlayWindow.IsLoaded || 
                                   (resultsOverlayWindow.Tag?.ToString() == "disposed");
            
            if (needNewInstance)
            {
                Logger.Logger.Info("Creating new ResultsOverlayWindow instance");
                resultsOverlayWindow = new ResultsOverlayWindow(this);
            }
            
            // Ensure we have a valid window instance before proceeding
            if (resultsOverlayWindow != null)
            {
                resultsOverlayWindow.SetTargetWindow(targetWindow, targetProcessId);
                resultsOverlayWindow.DisplayTradeRoutes(tradeRoutes);
                
                if (!resultsOverlayWindow.IsVisible)
                {
                    Logger.Logger.Info("Showing ResultsOverlayWindow");
                    resultsOverlayWindow.Show();
                }
                
                isResultsActive = true;
                Logger.Logger.Info($"Results overlay active: {isResultsActive}");
            }
            else
            {
                Logger.Logger.Error("Failed to create or access ResultsOverlayWindow instance");
            }
        }

        /// <summary>
        /// Ensures the main window is visible after target detection (called from App.xaml.cs)
        /// </summary>
        public void EnsureVisibleAfterTargetDetection()
        {
            Logger.Logger.Info("Setting forceVisible flag and transitioning to ForceShow state for target detection");
            forceVisible = true;
            
            // Force state transition to ForceShow if we're still waiting
            if (currentState == OverlayState.Waiting)
            {
                currentState = OverlayState.ForceShow;

                Logger.Logger.Info("State transition: Waiting -> ForceShow (triggered by target detection)");
            }
            
            // If the window is already loaded and target is available, try to show immediately
            if (this.IsLoaded && targetWindow != IntPtr.Zero)
            {
                bool targetVisible = WindowsAPI.IsWindowVisible(targetWindow);
                bool targetMinimized = WindowsAPI.IsIconic(targetWindow);
                
                if (targetVisible && !targetMinimized && !this.IsVisible)
                {
                    Logger.Logger.Info("Showing MainWindow immediately after target detection");
                    this.Show();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Logger.Logger.Info("MainWindow is closing - cleaning up resources");
            
            disposed = true;
            
            // Stop the update timer first
            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer = null;
            }
            
            // Stop the retry timer if it's running
            if (retryTimer != null)
            {
                retryTimer.Stop();
                retryTimer = null;
            }
            
            // Close all child windows properly
            CloseAllOverlayWindows();
            
            Logger.Logger.Info("MainWindow closed and all resources cleaned up");
            base.OnClosed(e);
        }
    }
}
