using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Collections.Generic;
using ED_Inara_Overlay.Utils;
using ED_Inara_Overlay.Windows;
using ED_Inara_Overlay.Services;
using System.Diagnostics;
using InaraTools;

namespace ED_Inara_Overlay
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
        private bool overlaysSuppressedByHotkey = false;
        private bool restoreTradeVisible = false;
        private bool restoreResultsVisible = false;
        private bool restorePinnedVisible = false;
        private bool forceVisible = false; // Flag to ensure visibility after target detection
        private OverlayState currentState = OverlayState.Waiting;
        private const int HOTKEY_ID_TOGGLE = 9001;
        private const int HOTKEY_ID_INTERACTIVE = 9002;
        private const int HOTKEY_ID_UNPIN = 9003;
        private HwndSource? hwndSource; // For handling Windows messages
        private uint toggleHotkeyModifiers = WindowsAPI.MOD_CONTROL;
        private uint toggleHotkeyVirtualKey = WindowsAPI.VK_5;
        private uint interactiveHotkeyModifiers = WindowsAPI.MOD_CONTROL;
        private uint interactiveHotkeyVirtualKey = WindowsAPI.VK_6;
        private uint unpinHotkeyModifiers = WindowsAPI.MOD_CONTROL;
        private uint unpinHotkeyVirtualKey = WindowsAPI.VK_7;
        private bool interactionModeEnabled = true;
        private bool interactiveModeActive;
        private bool returnOnFocusLoss = true;
        private bool showCursorWhenInteractive = true;
        private int autoReturnTimeoutSeconds = 8;
        private DateTime interactiveModeEnteredAtUtc;
        private DateTime interactiveFocusLossGraceUntilUtc;
        private static readonly TimeSpan InteractiveFocusLossGracePeriod = TimeSpan.FromMilliseconds(1500);
        private readonly double baseWindowWidth;
        private readonly double baseWindowHeight;
        private double lastAppliedScale = 1.0;

        public MainWindow(string processName = "notepad", Process? foundProcess = null)
        {
            InitializeComponent();
            baseWindowWidth = Width;
            baseWindowHeight = Height;
            
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
            LoadConfiguredSettings();
            UpdateOverlayInteractionModes();
            UpdateInteractionStatusUi();
            
            // Listen for theme changes
            ThemeManager.Instance.ThemeApplied += OnThemeApplied;
            SettingsService.Instance.SettingsChanged += OnSettingsChanged;
            
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
                    
                    // Position at a safe location initially on the target monitor (or primary monitor fallback)
                    var workArea = WindowsAPI.GetMonitorWorkArea(targetWindow);
                    
                    // Default safe position (bottom-left of screen)
                    this.Left = workArea.Left + 50;
                    this.Top = workArea.Bottom - this.Height - 50;
                    
                    Logger.Logger.Info($"MainWindow positioned at safe location: Left={this.Left}, Top={this.Top}, WorkArea={workArea.Width}x{workArea.Height}");
                    
                    // If target is set, try relative positioning with bounds checking
                    if (targetWindow != IntPtr.Zero)
                    {
                        try
                        {
                            ApplyAdaptiveSizeForTarget();
                            // Use new WindowInteropHelper positioning method
                            WindowsAPI.PositionWindowRelativeToTarget(this, targetWindow, RelativePosition.BottomLeft);
                            Logger.Logger.Info($"MainWindow repositioned relative to target: Left={this.Left}, Top={this.Top}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Error($"Error positioning MainWindow relative to target: {ex.Message}");
                        }
                    }
                    
                    // Set up global hotkeys after window is loaded
                    SetupGlobalHotkeys();
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
                Interval = TimeSpan.FromMilliseconds(33) // ~30 FPS
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
                    ApplyAdaptiveSizeForTarget();
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
                EvaluateInteractiveAutoReturn(foregroundWindow);
                
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

        private void ApplyAdaptiveSizeForTarget()
        {
            if (targetWindow == IntPtr.Zero || !WindowsAPI.GetWindowRect(targetWindow, out WindowsAPI.RECT targetRect))
            {
                return;
            }

            double targetWidth = targetRect.Right - targetRect.Left;
            double targetHeight = targetRect.Bottom - targetRect.Top;
            OverlayLayoutHelper.TryApplyAdaptiveSize(
                this,
                baseWindowWidth,
                baseWindowHeight,
                targetWidth,
                targetHeight,
                OverlayLayoutSettings.MainMinScale,
                OverlayLayoutSettings.MainMaxScale,
                ref lastAppliedScale);
        }

        private void LoadConfiguredSettings()
        {
            var settings = SettingsService.Instance.Settings;
            interactionModeEnabled = settings.EnableInteractionMode;
            autoReturnTimeoutSeconds = NormalizeAutoReturnTimeout(settings.AutoReturnTimeoutSeconds);
            returnOnFocusLoss = settings.ReturnOnFocusLoss;
            showCursorWhenInteractive = settings.ShowCursorWhenInteractive;

            if (TryResolveHotkey(settings.ToggleHotkeyModifiers, settings.ToggleHotkeyKey, out var resolvedToggleModifiers, out var resolvedToggleKey))
            {
                toggleHotkeyModifiers = resolvedToggleModifiers;
                toggleHotkeyVirtualKey = resolvedToggleKey;
            }
            else
            {
                toggleHotkeyModifiers = WindowsAPI.MOD_CONTROL;
                toggleHotkeyVirtualKey = WindowsAPI.VK_5;
                Logger.Logger.Warning($"Invalid toggle hotkey ({settings.ToggleHotkeyModifiers}+{settings.ToggleHotkeyKey}). Falling back to Ctrl+D5.");
            }

            if (TryResolveHotkey(settings.InteractiveHotkeyModifiers, settings.InteractiveHotkeyKey, out var resolvedInteractiveModifiers, out var resolvedInteractiveKey))
            {
                interactiveHotkeyModifiers = resolvedInteractiveModifiers;
                interactiveHotkeyVirtualKey = resolvedInteractiveKey;
            }
            else
            {
                interactiveHotkeyModifiers = WindowsAPI.MOD_CONTROL;
                interactiveHotkeyVirtualKey = WindowsAPI.VK_6;
                Logger.Logger.Warning($"Invalid interactive hotkey ({settings.InteractiveHotkeyModifiers}+{settings.InteractiveHotkeyKey}). Falling back to Ctrl+D6.");
            }
        }

        private bool TryResolveHotkey(string modifiersText, string keyText, out uint modifiers, out uint virtualKey)
        {
            modifiers = 0;
            virtualKey = 0;

            modifiers = modifiersText switch
            {
                "Ctrl" => WindowsAPI.MOD_CONTROL,
                "Shift" => WindowsAPI.MOD_SHIFT,
                "Alt" => WindowsAPI.MOD_ALT,
                "Ctrl+Shift" => WindowsAPI.MOD_CONTROL | WindowsAPI.MOD_SHIFT,
                "Ctrl+Alt" => WindowsAPI.MOD_CONTROL | WindowsAPI.MOD_ALT,
                "Alt+Shift" => WindowsAPI.MOD_ALT | WindowsAPI.MOD_SHIFT,
                _ => 0
            };

            if (modifiers == 0)
            {
                return false;
            }

            if (!Enum.TryParse<Key>(keyText, true, out var key))
            {
                return false;
            }

            int vk = KeyInterop.VirtualKeyFromKey(key);
            if (vk <= 0)
            {
                return false;
            }

            virtualKey = (uint)vk;
            return true;
        }

        private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
        {
            bool hotkeysChanged = false;

            if (TryResolveHotkey(e.Settings.ToggleHotkeyModifiers, e.Settings.ToggleHotkeyKey, out var newToggleModifiers, out var newToggleVirtualKey)
                && (newToggleModifiers != toggleHotkeyModifiers || newToggleVirtualKey != toggleHotkeyVirtualKey))
            {
                toggleHotkeyModifiers = newToggleModifiers;
                toggleHotkeyVirtualKey = newToggleVirtualKey;
                hotkeysChanged = true;
            }

            if (TryResolveHotkey(e.Settings.InteractiveHotkeyModifiers, e.Settings.InteractiveHotkeyKey, out var newInteractiveModifiers, out var newInteractiveVirtualKey)
                && (newInteractiveModifiers != interactiveHotkeyModifiers || newInteractiveVirtualKey != interactiveHotkeyVirtualKey))
            {
                interactiveHotkeyModifiers = newInteractiveModifiers;
                interactiveHotkeyVirtualKey = newInteractiveVirtualKey;
                hotkeysChanged = true;
            }

            interactionModeEnabled = e.Settings.EnableInteractionMode;
            autoReturnTimeoutSeconds = NormalizeAutoReturnTimeout(e.Settings.AutoReturnTimeoutSeconds);
            returnOnFocusLoss = e.Settings.ReturnOnFocusLoss;
            showCursorWhenInteractive = e.Settings.ShowCursorWhenInteractive;

            if (!interactionModeEnabled && interactiveModeActive)
            {
                SetInteractiveMode(false, "disabled from settings");
            }

            UpdateOverlayInteractionModes();
            UpdateInteractionStatusUi();

            if (hotkeysChanged)
            {
                UnregisterGlobalHotkeys();
                SetupGlobalHotkeys();
                Logger.Logger.Info("Global hotkeys reconfigured from settings");
            }
        }

        private int NormalizeAutoReturnTimeout(int value)
        {
            return value switch
            {
                0 or 5 or 8 or 10 or 15 => value,
                _ => 8
            };
        }

        private void SetInteractiveMode(bool isActive, string reason)
        {
            if (interactiveModeActive == isActive)
            {
                return;
            }

            interactiveModeActive = isActive;
            if (interactiveModeActive)
            {
                interactiveModeEnteredAtUtc = DateTime.UtcNow;
                interactiveFocusLossGraceUntilUtc = interactiveModeEnteredAtUtc + InteractiveFocusLossGracePeriod;
                FocusInteractiveOverlayWindow();
            }

            UpdateOverlayInteractionModes();
            UpdateInteractionStatusUi();
            Logger.Logger.Info($"Interactive mode {(interactiveModeActive ? "ENABLED" : "DISABLED")} ({reason})");
        }

        private void UpdateOverlayInteractionModes()
        {
            bool canInteract = interactionModeEnabled && interactiveModeActive;

            tradeRouteWindow?.ApplyInteractionMode(canInteract, showCursorWhenInteractive);
            resultsOverlayWindow?.ApplyInteractionMode(canInteract, showCursorWhenInteractive);
            if (pinnedRouteOverlay != null)
            {
                WindowsAPI.SetClickThrough(pinnedRouteOverlay, true);
            }
            if (canInteract == false)
            {
                WindowsAPI.TryActivateWindow(targetWindow);
            }
        }

        private void UpdateInteractionStatusUi()
        {
            if (InteractionStatusBadge == null || InteractionHintText == null)
            {
                return;
            }

            bool canInteract = interactionModeEnabled && interactiveModeActive;
            string stateText = canInteract ? "INTERACTIVE: ON" : "INTERACTIVE: OFF";
            InteractionStatusBadge.Text = stateText;

            InteractionStatusBadge.Background = canInteract
                ? new SolidColorBrush(Color.FromArgb(180, 180, 95, 0))
                : new SolidColorBrush(Color.FromArgb(120, 70, 25, 0));

            string interactionHotkeyText = FormatHotkeyDisplay(SettingsService.Instance.Settings.InteractiveHotkeyModifiers, SettingsService.Instance.Settings.InteractiveHotkeyKey);
            string toggleHotkeyText = FormatHotkeyDisplay(SettingsService.Instance.Settings.ToggleHotkeyModifiers, SettingsService.Instance.Settings.ToggleHotkeyKey);
            string overlaysStateText = overlaysSuppressedByHotkey ? "hidden" : "visible";
            InteractionHintText.Text =
                $"Overlays: {overlaysStateText} | Show/Hide: {toggleHotkeyText} | Interactive: {interactionHotkeyText} | Unpin: Ctrl+7";
        }

        private static string FormatHotkeyDisplay(string modifiers, string key)
        {
            if (key.StartsWith("D", StringComparison.OrdinalIgnoreCase) && key.Length == 2 && char.IsDigit(key[1]))
            {
                return $"{modifiers}+{key[1]}";
            }

            return $"{modifiers}+{key}";
        }

        private void EvaluateInteractiveAutoReturn(IntPtr foregroundWindow)
        {
            if (!interactiveModeActive)
            {
                return;
            }

            bool focusIsOnInteractiveOverlay = IsWindowFocused(tradeRouteWindow, foregroundWindow)
                || IsWindowFocused(resultsOverlayWindow, foregroundWindow);

            bool gracePeriodActive = DateTime.UtcNow < interactiveFocusLossGraceUntilUtc;
            if (returnOnFocusLoss && !gracePeriodActive && !focusIsOnInteractiveOverlay)
            {
                SetInteractiveMode(false, "focus loss");
                return;
            }

            if (autoReturnTimeoutSeconds > 0
                && (DateTime.UtcNow - interactiveModeEnteredAtUtc).TotalSeconds >= autoReturnTimeoutSeconds)
            {
                SetInteractiveMode(false, $"timeout {autoReturnTimeoutSeconds}s");
            }
        }

        private static bool IsWindowFocused(Window? window, IntPtr foregroundWindow)
        {
            if (window == null || !window.IsLoaded)
            {
                return false;
            }

            var handle = new WindowInteropHelper(window).Handle;
            return handle != IntPtr.Zero && handle == foregroundWindow;
        }

        private void FocusInteractiveOverlayWindow()
        {
            try
            {
                if (resultsOverlayWindow?.IsVisible == true)
                {
                    resultsOverlayWindow.Activate();
                    return;
                }

                if (tradeRouteWindow?.IsVisible == true)
                {
                    tradeRouteWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"Failed to focus interactive overlay window: {ex.Message}");
            }
        }

    }
}
