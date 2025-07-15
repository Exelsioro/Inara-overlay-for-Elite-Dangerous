using System;
using System.Windows;
using ED_Inara_Overlay_2._0.Windows;
using ED_Inara_Overlay_2._0.Utils;
using ED_Inara_Overlay_2._0.Services;

namespace ED_Inara_Overlay_2._0
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string targetProcessName = "notepad";
        private WaitingWindow? waitingWindow;
        private MainWindow? mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Get target process from command line args or default to notepad
            if (e.Args.Length > 0)
            {
                targetProcessName = e.Args[0];
            }

            Logger.Logger.Info($"Application starting with target process: {targetProcessName}");

            // Initialize theme system
            try
            {
                ThemeManager.Instance.LoadAvailableThemes();
                if (ThemeManager.Instance.AvailableThemes.Count > 0)
                {
                    // Apply the first available theme (Default)
                    ThemeManager.Instance.ApplyTheme(ThemeManager.Instance.AvailableThemes[0]);
                    Logger.Logger.Info("Theme system initialized successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error initializing theme system: {ex.Message}");
            }

            // Check if target process is already running
            var existingProcess = WindowsAPI.FindProcessByName(targetProcessName);
            
            if (existingProcess != null)
            {
                // Target process is already running - start main overlay directly
                Logger.Logger.Info($"Target process {targetProcessName} already running - starting main overlay");
                StartMainOverlay();
            }
            else
            {
                // Target process not running - show waiting window
                Logger.Logger.Info($"Target process {targetProcessName} not running - showing waiting window");
                ShowWaitingWindow();
            }
        }

        private void ShowWaitingWindow()
        {
            Logger.Logger.Info("Creating and showing WaitingWindow");
            
            waitingWindow = new WaitingWindow(targetProcessName);
            waitingWindow.TargetProcessFound += OnTargetProcessFound;
            waitingWindow.Show();
            
            Logger.Logger.Info("WaitingWindow displayed");
        }

        private void OnTargetProcessFound(object? sender, string processName)
        {
            Logger.Logger.Info($"Target process found event received: {processName}");
            
            // Close waiting window
            if (waitingWindow != null)
            {
                waitingWindow.TargetProcessFound -= OnTargetProcessFound;
                waitingWindow.CloseWaitingWindow();
                waitingWindow = null;
            }
            
            // Start main overlay
            this.Dispatcher.BeginInvoke(new Action(() => 
            {
                StartMainOverlay();
            }));
        }

        private void StartMainOverlay()
        {
            Logger.Logger.Info($"Starting main overlay for target process: {targetProcessName}");
            
            try
            {
                // Create main window (starts hidden)
                mainWindow = new MainWindow(targetProcessName);
                
                // Set the main window as the shutdown target
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                this.MainWindow = mainWindow;
                
                // Ensure it will be visible after target detection
                mainWindow.EnsureVisibleAfterTargetDetection();
                
                // Note: MainWindow starts hidden and will show when target has focus
                
                Logger.Logger.Info("Main overlay window created and displayed with forced visibility");
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error starting main overlay: {ex.Message}");
                
                // Show error message and shutdown
                MessageBox.Show(
                    $"Failed to start overlay: {ex.Message}", 
                    "ED Inara Overlay Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                
                this.Shutdown();
            }
        }


        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Logger.Info("Application is exiting");
            
            // Clean up waiting window
            if (waitingWindow != null)
            {
                waitingWindow.TargetProcessFound -= OnTargetProcessFound;
                waitingWindow.Close();
                waitingWindow = null;
            }
            
            // Clean up main window
            if (mainWindow != null)
            {
                mainWindow.Close();
                mainWindow = null;
            }
            
            Logger.Logger.Info("Application exit cleanup completed");
            base.OnExit(e);
        }
    }
}

