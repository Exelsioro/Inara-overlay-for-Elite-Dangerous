using System;
using System.Windows;
using System.Windows.Threading;
using ED_Inara_Overlay_2._0.Utils;

namespace ED_Inara_Overlay_2._0.Windows
{
    /// <summary>
    /// Waiting window that shows while target application is not running
    /// </summary>
    public partial class WaitingWindow : Window
    {
        private readonly string targetProcessName;
        private DispatcherTimer? checkTimer;
        private bool shouldClose = false;
        private bool targetFound = false; // Track if closure is due to target being found

        public event EventHandler<string>? TargetProcessFound;

        public WaitingWindow(string processName = "notepad")
        {
            InitializeComponent();
            
            targetProcessName = processName;
            
            Logger.Logger.Info($"WaitingWindow initialized for target process: {processName}");
            
            SetupUI();
            StartMonitoring();
        }

        private void SetupUI()
        {
            // Update UI based on target process
            TargetProcessText.Text = $"Looking for: {targetProcessName}.exe";
            
            string displayName = GetDisplayName(targetProcessName);
            TargetAppName.Text = displayName;
            TargetAppName.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0, 255, 0)); // Green
            
            Logger.Logger.Info($"WaitingWindow UI configured for: {displayName}");
        }

        private string GetDisplayName(string processName)
        {
            return processName.ToLower() switch
            {
                "notepad" => "Notepad",
                "elitedangerous64" => "Elite Dangerous",
                "elitedangerous32" => "Elite Dangerous",
                "steam" => "Steam",
                _ => processName
            };
        }

        private void StartMonitoring()
        {
            checkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Check every second
            };
            
            checkTimer.Tick += CheckTimer_Tick;
            checkTimer.Start();
            
            Logger.Logger.Info($"Started monitoring for target process: {targetProcessName}");
        }

        private void CheckTimer_Tick(object? sender, EventArgs e)
        {
            if (shouldClose || checkTimer == null)
                return;

            try
            {
                // Check if target process is running
                var process = WindowsAPI.FindProcessByName(targetProcessName);
                
                if (process != null)
                {
                    Logger.Logger.Info($"Target process found: {targetProcessName} (PID: {process.Id})");
                    
                    // Stop monitoring
                    checkTimer.Stop();
                    checkTimer = null;
                    
                    // Mark that target was found
                    targetFound = true;
                    shouldClose = true;
                    
                    // Notify parent that target was found
                    TargetProcessFound?.Invoke(this, targetProcessName);
                    
                    // Close this waiting window
                    this.Close();
                }
                else
                {
                    // Update status to show we're still looking
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error checking for target process: {ex.Message}");
            }
        }

        private void UpdateStatus()
        {
            // Add some visual feedback that we're actively searching
            var now = DateTime.Now;
            var dots = (now.Second % 4) switch
            {
                0 => "",
                1 => ".",
                2 => "..",
                _ => "..."
            };
            
            StatusText.Text = $"Waiting for target application{dots}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Logger.Info("User clicked Exit Overlay button in WaitingWindow");
            Logger.Logger.LogUserAction("Exit button clicked in waiting window", new { TargetProcess = targetProcessName });
            
            shouldClose = true;
            
            // Stop monitoring
            if (checkTimer != null)
            {
                checkTimer.Stop();
                checkTimer = null;
            }
            
            // Close the application
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            Logger.Logger.Info($"WaitingWindow is closing - targetFound: {targetFound}, shouldClose: {shouldClose}");
            
            // Stop monitoring
            if (checkTimer != null)
            {
                checkTimer.Stop();
                checkTimer = null;
            }
            
            // Only shutdown application if window was closed manually (not due to target being found)
            if (!shouldClose || (!targetFound && shouldClose))
            {
                Logger.Logger.Info("WaitingWindow closed by user - shutting down application");
                Application.Current.Shutdown();
            }
            else if (targetFound)
            {
                Logger.Logger.Info("WaitingWindow closed due to target process found - transitioning to main overlay");
                // Don't shutdown - let the app transition to main overlay
            }
            
            base.OnClosed(e);
        }

        /// <summary>
        /// Manually close the waiting window (called when target process is found)
        /// </summary>
        public void CloseWaitingWindow()
        {
            Logger.Logger.Info("CloseWaitingWindow called - target was found");
            targetFound = true;
            shouldClose = true;
            
            if (checkTimer != null)
            {
                checkTimer.Stop();
                checkTimer = null;
            }
            
            this.Close();
        }
    }
}
