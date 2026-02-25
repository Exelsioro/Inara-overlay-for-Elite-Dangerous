using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Windows.Media;
using InaraTools;
using ED_Inara_Overlay.Utils;
using ED_Inara_Overlay.UserControls;

namespace ED_Inara_Overlay.Windows
{
    /// <summary>
    /// Pinned Route Overlay Window - displays a single pinned trade route at the top center of the screen
    /// </summary>
    public partial class PinnedRouteOverlay : Window
    {
        private IntPtr targetWindow;
        private uint targetProcessId;
        private DispatcherTimer? updateTimer;
        private bool disposed = false;
        private MainWindow? parentMainWindow;
        private TradeRouteCard? currentPinnedCard;

        public PinnedRouteOverlay(MainWindow? parentWindow = null)
        {
            parentMainWindow = parentWindow;
            Logger.Logger.Info("Initializing PinnedRouteOverlay");
            
            InitializeComponent();
            SetupOverlay();
            SetupUpdateTimer();
            
            Logger.Logger.Info("PinnedRouteOverlay initialization complete");
        }

        private void SetupOverlay()
        {
            // Set up overlay behavior when window is loaded
            this.Loaded += (s, e) =>
            {
                WindowsAPI.SetupOverlayWindow(this);
                WindowsAPI.SetClickThrough(this, true);
                
                // Position at top center of screen initially
                PositionOverlay();
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

            if (OverlayVisibilityState.SuppressAll)
            {
                if (this.IsVisible)
                {
                    this.Hide();
                }
                return;
            }

            // If form is not visible, we still need to update position for when it becomes visible
            if (targetWindow != IntPtr.Zero)
            {
                // Check if target window still exists
                if (!WindowsAPI.IsWindow(targetWindow))
                {
                    this.Hide();
                    this.Close();
                    return;
                }

                // Always update overlay position to match target window (positioned at top center)
                PositionOverlay();

                // Check if target window has focus
                IntPtr foregroundWindow = WindowsAPI.GetForegroundWindow();
                bool targetHasFocus = (foregroundWindow == targetWindow);
                bool overlayHasFocus = WindowsAPI.IsOverlayWindow(foregroundWindow);

                // Check if target window is minimized or not visible
                bool targetMinimized = WindowsAPI.IsIconic(targetWindow);
                bool targetVisible = WindowsAPI.IsWindowVisible(targetWindow);

                // Determine if overlay should be visible based on target window state and focus
                // Should be visible if target has focus OR any overlay window has focus
                bool shouldBeVisible = targetVisible && !targetMinimized && (targetHasFocus || overlayHasFocus);
                
                // Set topmost only when target or overlay has focus
                bool shouldBeTopmost = targetHasFocus || overlayHasFocus;

                if (shouldBeVisible && !this.IsVisible)
                {
                    Logger.Logger.Info("PinnedRouteOverlay showing - target window has focus and is visible");
                    this.Show();
                }
                else if (!shouldBeVisible && this.IsVisible)
                {
                    Logger.Logger.Info("PinnedRouteOverlay hiding - target window lost focus or is not visible");
                    this.Hide();
                }
                
                // Apply topmost state conditionally using WindowInteropHelper
                if (this.IsVisible && this.IsLoaded)
                {
                    WindowsAPI.SetTopmost(this, shouldBeTopmost);
                }

                if (updateTimer != null)
                {
                    updateTimer.Tag = ((int?)updateTimer.Tag ?? 0) + 1;
                }
            }
        }

        private void PositionOverlay()
        {
            if (targetWindow != IntPtr.Zero && WindowsAPI.GetWindowRect(targetWindow, out WindowsAPI.RECT rect))
            {
                Rect workArea = WindowsAPI.GetMonitorWorkArea(targetWindow);

                // Position pinned overlay at top center of the target window
                int targetWidth = rect.Right - rect.Left;
                int overlayWidth = Math.Min(
                    (int)(workArea.Width * OverlayLayoutSettings.PinnedWidthByMonitor),
                    Math.Min(OverlayLayoutSettings.PinnedMaxWidth, (int)(targetWidth * OverlayLayoutSettings.PinnedWidthByTarget)));
                
                // Calculate dynamic height based on content, with minimum and maximum bounds
                int overlayHeight = CalculateRequiredHeight();

                // Center horizontally, position at very top of window
                var (centerX, topY) = OverlayLayoutHelper.GetTopCenteredPosition(rect, overlayWidth, OverlayLayoutSettings.PinnedTopOffset);
                OverlayLayoutHelper.ClampPosition(
                    ref centerX,
                    ref topY,
                    overlayWidth,
                    overlayHeight,
                    workArea,
                    OverlayLayoutSettings.DefaultMargin,
                    OverlayLayoutSettings.PinnedClampMarginY);
                
                this.Left = centerX;
                this.Top = topY;
                this.Width = overlayWidth;
                this.Height = overlayHeight;
            }
            else
            {
                // Fallback to screen center top if no target window
                var workArea = WindowsAPI.GetMonitorWorkArea(targetWindow);
                
                this.Left = workArea.Left + ((workArea.Width - this.Width) / 2);
                this.Top = workArea.Top + OverlayLayoutSettings.PinnedFallbackTopOffset;
            }
        }

        public void SetTargetWindow(IntPtr windowHandle, uint processId)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(PinnedRouteOverlay));
                
            targetWindow = windowHandle;
            targetProcessId = processId;

            if (windowHandle != IntPtr.Zero)
            {
                PositionOverlay();
            }
        }

        private int CalculateRequiredHeight()
        {
            if (currentPinnedCard != null && PinnedRouteContainer.Children.Count > 0)
            {
                // Force a layout update to get accurate measurements
                currentPinnedCard.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                
                // Use the desired height of the card plus margin
                double cardHeight = currentPinnedCard.DesiredSize.Height;
                if (cardHeight > 0)
                {
                    int requiredHeight = (int)Math.Ceiling(cardHeight + OverlayLayoutSettings.PinnedContentMargin);
                    return Math.Max(OverlayLayoutSettings.PinnedMinHeight, Math.Min(OverlayLayoutSettings.PinnedMaxHeight, requiredHeight));
                }
            }
            
            return OverlayLayoutSettings.PinnedMinHeight;
        }

        public void PinTradeRoute(TradeRoute tradeRoute)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(PinnedRouteOverlay));

            Logger.Logger.Info($"Pinning trade route: {tradeRoute.CardHeader.FromStation.System} -> {tradeRoute.CardHeader.ToStation.System}");

            // Clear any existing pinned card
            PinnedRouteContainer.Children.Clear();

            // Create a new card for the pinned route
            currentPinnedCard = new TradeRouteCard(tradeRoute);
            
            // Ensure card stretches to fill available width
            currentPinnedCard.HorizontalAlignment = HorizontalAlignment.Stretch;
            
            // Adjust card for pinned display (more compact)
            currentPinnedCard.MinHeight = tradeRoute.IsRoundTrip ? 180 : 120;
            
            // Hide the pin button on the pinned card (avoid recursive pinning)
            if (currentPinnedCard.FindName("PinRouteButton") is System.Windows.Controls.Button pinButton)
            {
                pinButton.Visibility = Visibility.Collapsed;
            }

            PinnedRouteContainer.Children.Add(currentPinnedCard);
            
            // Force immediate layout update to get accurate measurements
            currentPinnedCard.UpdateLayout();
            this.UpdateLayout();
            
            // Position the overlay with the correct size
            PositionOverlay();
            
            // Show the overlay
            this.Show();
            
            Logger.Logger.LogUserAction($"Trade route pinned successfully: {tradeRoute.CardHeader.FromStation.System} -> {tradeRoute.CardHeader.ToStation.System}");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Logger.Info("Pinned route overlay close button clicked");
            Logger.Logger.LogUserAction("Pinned route overlay closed by user");
            
            // Clear the pinned route
            PinnedRouteContainer.Children.Clear();
            currentPinnedCard = null;
            
            this.Close();
        }

        public new void Close()
        {
            if (!disposed)
            {
                disposed = true;
                updateTimer?.Stop();
                updateTimer = null;
                
                PinnedRouteContainer.Children.Clear();
                currentPinnedCard = null;
                
                Logger.Logger.Info("PinnedRouteOverlay disposed");
            }
            
            base.Close();
        }
    }
}
