using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ED_Inara_Overlay.Utils
{
    public static class WindowsAPI
    {
        #region Windows API Constants
        
        public const int GWL_EXSTYLE = -20;
        public const uint WS_EX_LAYERED = 0x80000;
        public const uint WS_EX_TOPMOST = 0x8;
        public const uint WS_EX_NOACTIVATE = 0x8000000;
        public const uint WS_EX_TRANSPARENT = 0x20;
        
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        
        // SetWindowPos constants
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        
        #endregion

        #region Windows API Imports

        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        // Global hotkey API functions
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Hotkey modifier constants
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        // Virtual key codes
        public const uint VK_0 = 0x30;
        public const uint VK_1 = 0x31;
        public const uint VK_2 = 0x32;
        public const uint VK_3 = 0x33;
        public const uint VK_4 = 0x34;
        public const uint VK_5 = 0x35;
        public const uint VK_6 = 0x36;
        public const uint VK_7 = 0x37;
        public const uint VK_8 = 0x38;
        public const uint VK_9 = 0x39;

        // Windows message constants
        public const int WM_HOTKEY = 0x0312;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        #endregion

        #region Helper Methods

        /// <summary>
        /// Finds a process by name and returns the first match
        /// </summary>
        public static Process? FindProcessByName(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0 ? processes[0] : null;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"WindowsAPI.FindProcessByName: Error finding process '{processName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Finds the main window handle for a process ID
        /// </summary>
        public static IntPtr FindWindowByPID(uint processId)
        {
            IntPtr targetWindow = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                uint windowProcessId;
                GetWindowThreadProcessId(hWnd, out windowProcessId);
                
                if (windowProcessId == processId && IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        var title = new System.Text.StringBuilder(length + 1);
                        GetWindowText(hWnd, title, title.Capacity);
                        
                        // Skip windows without meaningful titles
                        if (!string.IsNullOrWhiteSpace(title.ToString()))
                        {
                            targetWindow = hWnd;
                            return false; // Stop enumeration
                        }
                    }
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            return targetWindow;
        }

        /// <summary>
        /// Gets the window bounds for a window handle
        /// </summary>
        public static Rect GetWindowBounds(IntPtr hWnd)
        {
            if (GetWindowRect(hWnd, out RECT rect))
            {
                return new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            return Rect.Empty;
        }

        /// <summary>
        /// Gets working area for the monitor containing the target window.
        /// Falls back to primary working area if monitor detection fails.
        /// </summary>
        public static Rect GetMonitorWorkArea(IntPtr targetWindow)
        {
            try
            {
                if (targetWindow != IntPtr.Zero)
                {
                    IntPtr monitor = MonitorFromWindow(targetWindow, MONITOR_DEFAULTTONEAREST);
                    if (monitor != IntPtr.Zero)
                    {
                        var monitorInfo = new MONITORINFO
                        {
                            cbSize = Marshal.SizeOf<MONITORINFO>()
                        };

                        if (GetMonitorInfo(monitor, ref monitorInfo))
                        {
                            return new Rect(
                                monitorInfo.rcWork.Left,
                                monitorInfo.rcWork.Top,
                                monitorInfo.rcWork.Right - monitorInfo.rcWork.Left,
                                monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"WindowsAPI.GetMonitorWorkArea: fallback to primary work area due to error: {ex.Message}");
            }

            return SystemParameters.WorkArea;
        }

        /// <summary>
        /// Attempts to activate and focus a target window.
        /// </summary>
        public static bool TryActivateWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero || !IsWindow(hWnd))
            {
                return false;
            }

            try
            {
                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }
                else
                {
                    ShowWindow(hWnd, SW_SHOW);
                }

                return SetForegroundWindow(hWnd);
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"WindowsAPI.TryActivateWindow failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets up a WPF window for overlay behavior without global topmost
        /// </summary>
        public static void SetupOverlayWindow(Window window)
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle != IntPtr.Zero)
            {
                uint exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
                // Remove WS_EX_TOPMOST from global setup - will be set conditionally
                // Removed WS_EX_NOACTIVATE to allow window activation for combo box dropdowns
                SetWindowLong(helper.Handle, GWL_EXSTYLE, 
                    exStyle | WS_EX_LAYERED);
            }
        }

        /// <summary>
        /// Sets the topmost state of a window conditionally
        /// </summary>
        public static void SetTopmost(Window window, bool topmost)
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle != IntPtr.Zero)
            {
                SetWindowPos(helper.Handle, 
                    topmost ? HWND_TOPMOST : HWND_NOTOPMOST,
                    0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }

        /// <summary>
        /// Positions a window relative to a target window using WindowInteropHelper
        /// </summary>
        public static void PositionWindowRelativeToTarget(Window window, IntPtr targetWindow, RelativePosition position)
        {
            if (targetWindow == IntPtr.Zero || !GetWindowRect(targetWindow, out RECT targetRect))
                return;

            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero)
                return;

            var (newX, newY) = OverlayLayoutHelper.GetRelativePosition(targetRect, window.Width, window.Height, position);
            Rect workArea = GetMonitorWorkArea(targetWindow);
            OverlayLayoutHelper.ClampPosition(ref newX, ref newY, window.Width, window.Height, workArea, 0, 0);

            window.Left = newX;
            window.Top = newY;
        }

        /// <summary>
        /// Makes a WPF window click-through where appropriate
        /// </summary>
        public static void SetClickThrough(Window window, bool clickThrough)
        {
            var helper = new WindowInteropHelper(window);
            if (helper.Handle != IntPtr.Zero)
            {
                uint exStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
                if (clickThrough)
                {
                    SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
                }
                else
                {
                    SetWindowLong(helper.Handle, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
                }
            }
        }

        /// <summary>
        /// Checks if a window handle belongs to the current process (overlay application)
        /// </summary>
        public static bool IsOverlayWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;
                
            uint windowProcessId;
            GetWindowThreadProcessId(hWnd, out windowProcessId);
            
            uint currentProcessId = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
            return windowProcessId == currentProcessId;
        }

        /// <summary>
        /// Ensures cursor is visible and positioned at center of the specified window.
        /// Useful when showing a newly opened interactive overlay window.
        /// </summary>
        public static void EnsureCursorVisibleOnWindow(Window window)
        {
            try
            {
                if (window == null || !window.IsVisible)
                {
                    return;
                }

                var helper = new WindowInteropHelper(window);
                if (helper.Handle == IntPtr.Zero)
                {
                    return;
                }

                while (ShowCursor(true) < 0)
                {
                }

                int centerX = (int)(window.Left + window.Width / 2);
                int centerY = (int)(window.Top + window.Height / 2);
                SetCursorPos(centerX, centerY);
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"WindowsAPI.EnsureCursorVisibleOnWindow failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Positions a window relative to a target window
        /// </summary>
       
        #endregion
    }

    public enum RelativePosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        MiddleLeft,
        MiddleRight,
        RightCenter,
        Center
    }
}
