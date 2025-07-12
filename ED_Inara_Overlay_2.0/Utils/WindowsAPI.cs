using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ED_Inara_Overlay_2._0.Utils
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
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

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

            // Calculate position based on target window and desired relative position
            int newX = targetRect.Left;
            int newY = targetRect.Top;
            
            switch (position)
            {
                case RelativePosition.TopLeft:
                    newX = targetRect.Left;
                    newY = targetRect.Top;
                    break;
                case RelativePosition.TopRight:
                    newX = targetRect.Right - (int)window.Width;
                    newY = targetRect.Top;
                    break;
                case RelativePosition.BottomLeft:
                    newX = targetRect.Left;
                    newY = targetRect.Bottom - (int)window.Height;
                    break;
                case RelativePosition.BottomRight:
                    newX = targetRect.Right - (int)window.Width;
                    newY = targetRect.Bottom - (int)window.Height;
                    break;
                case RelativePosition.Center:
                    newX = targetRect.Left + ((targetRect.Right - targetRect.Left) - (int)window.Width) / 2;
                    newY = targetRect.Top + ((targetRect.Bottom - targetRect.Top) - (int)window.Height) / 2;
                    break;
                case RelativePosition.RightCenter:
                    newX = targetRect.Right + 10;
                    newY = targetRect.Top + ((targetRect.Bottom - targetRect.Top) - (int)window.Height) / 2;
                    break;
            }

            // Ensure window stays within screen bounds
            var screenWidth = System.Windows.SystemParameters.WorkArea.Width;
            var screenHeight = System.Windows.SystemParameters.WorkArea.Height;
            
            if (newX < 0) newX = 0;
            if (newY < 0) newY = 0;
            if (newX + window.Width > screenWidth) newX = (int)(screenWidth - window.Width);
            if (newY + window.Height > screenHeight) newY = (int)(screenHeight - window.Height);

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
