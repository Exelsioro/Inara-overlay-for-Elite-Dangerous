using System;
using System.Windows;
using ED_Inara_Overlay.Windows;
using ED_Inara_Overlay.Utils;
using ED_Inara_Overlay.Services;
using System.Runtime.Versioning;
using System.Windows.Threading;

namespace ED_Inara_Overlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class App : Application
    {
        private string targetProcessName = "EliteDangerous64";
        private WaitingWindow? waitingWindow;
        private MainWindow? mainWindow;
        private TrayIconService? trayIconService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Get target process from command line args or default to notepad
            if (e.Args.Length > 0)
            {
                targetProcessName = e.Args[0];
            }

            Logger.Logger.Info($"Application starting with target process: {targetProcessName}");
            InitializeTrayIcon();

            // Initialize theme system
            try
            {
                ThemeManager.Instance.LoadAvailableThemes();
                if (ThemeManager.Instance.CurrentTheme != null)
                {
                    // Apply the loaded theme (saved or default)
                    ThemeManager.Instance.ApplyTheme(ThemeManager.Instance.CurrentTheme);
                    Logger.Logger.Info("Theme system initialized successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Error($"Error initializing theme system: {ex.Message}");
            }

            // Always show waiting window first, regardless of target process status
            // This gives users control over when to start the overlay
            Logger.Logger.Info($"Starting application - showing waiting window for target process: {targetProcessName}");
            ShowWaitingWindow();
        }

        private void ShowWaitingWindow()
        {
            if (waitingWindow != null)
            {
                if (!waitingWindow.IsVisible)
                {
                    waitingWindow.Show();
                }
                waitingWindow.Activate();
                return;
            }

            Logger.Logger.Info("Creating and showing WaitingWindow");
            
            waitingWindow = new WaitingWindow(targetProcessName);
            waitingWindow.TargetProcessFound += OnTargetProcessFound;
            waitingWindow.Show();
            
            Logger.Logger.Info("WaitingWindow displayed");
        }

        private void OnTargetProcessFound(object? sender, string processName)
        {
            Logger.Logger.Info($"Target process found event received: {processName}");
            
            if (waitingWindow != null)
            {
                waitingWindow.Hide();
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
                if (mainWindow != null)
                {
                    Logger.Logger.Info("Main overlay is already running. Activating existing instance.");
                    mainWindow.EnsureVisibleAfterTargetDetection();
                    trayIconService?.ShowWaitingHint();
                    return;
                }

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
            try
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

                if (trayIconService != null)
                {
                    trayIconService.OpenRequested -= OnTrayOpenRequested;
                    trayIconService.SettingsRequested -= OnTraySettingsRequested;
                    trayIconService.ExitRequested -= OnTrayExitRequested;
                    trayIconService.Dispose();
                    trayIconService = null;
                }
                
                Logger.Logger.Info("Application exit cleanup completed");
            }
            finally
            {
                Logger.Logger.Close();
                base.OnExit(e);
            }
        }

        private void InitializeTrayIcon()
        {
            trayIconService = new TrayIconService();
            trayIconService.OpenRequested += OnTrayOpenRequested;
            trayIconService.SettingsRequested += OnTraySettingsRequested;
            trayIconService.ExitRequested += OnTrayExitRequested;
            trayIconService.Initialize();
            Logger.Logger.Info("Tray icon initialized.");
        }

        private void OnTrayOpenRequested(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ShowWaitingWindow();
            }));
        }

        private void OnTrayExitRequested(object? sender, EventArgs e)
        {
            Logger.Logger.Info("Exit requested from tray menu.");
            Shutdown();
        }

        private void OnTraySettingsRequested(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            }));
        }

        public void ShowTrayWaitingHint()
        {
            trayIconService?.ShowWaitingHint();
            Logger.Logger.Info("Displayed tray notification about tray mode and tray exit.");
        }
    }
}

