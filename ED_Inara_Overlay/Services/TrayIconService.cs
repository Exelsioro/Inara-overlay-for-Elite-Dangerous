using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;

namespace ED_Inara_Overlay.Services
{
    [SupportedOSPlatform("windows")]
    public sealed class TrayIconService : IDisposable
    {
        private TaskbarIcon? taskbarIcon;
        private MenuItem? openItem;
        private MenuItem? settingsItem;
        private MenuItem? exitItem;

        public event EventHandler? OpenRequested;
        public event EventHandler? SettingsRequested;
        public event EventHandler? ExitRequested;

        public void Initialize()
        {
            if (taskbarIcon != null)
            {
                return;
            }

            openItem = new MenuItem { Header = "Open Control Window" };
            openItem.Click += OpenItem_Click;

            settingsItem = new MenuItem { Header = "Settings" };
            settingsItem.Click += SettingsItem_Click;

            exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += ExitItem_Click;

            var menu = new ContextMenu();
            menu.Items.Add(openItem);
            menu.Items.Add(settingsItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(exitItem);

            taskbarIcon = new TaskbarIcon
            {
                IconSource = LoadTrayIconSource(),
                ToolTipText = "ED Inara Overlay",
                ContextMenu = menu
            };
            taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_TrayMouseDoubleClick;
        }

        public void ShowWaitingHint()
        {
            if (taskbarIcon == null)
            {
                return;
            }

            taskbarIcon.ShowBalloonTip(
                "ED Inara Overlay",
                "Overlay is running in tray. If the game is not detected yet, it keeps waiting. Use tray menu to exit.",
                BalloonIcon.Info);
        }

        public void Dispose()
        {
            if (taskbarIcon != null)
            {
                taskbarIcon.TrayMouseDoubleClick -= TaskbarIcon_TrayMouseDoubleClick;
                taskbarIcon.Dispose();
                taskbarIcon = null;
            }

            if (openItem != null)
            {
                openItem.Click -= OpenItem_Click;
                openItem = null;
            }

            if (exitItem != null)
            {
                exitItem.Click -= ExitItem_Click;
                exitItem = null;
            }

            if (settingsItem != null)
            {
                settingsItem.Click -= SettingsItem_Click;
                settingsItem = null;
            }
        }

        private static BitmapSource? LoadTrayIconSource()
        {
            try
            {
                var processPath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(processPath))
                {
                    var icon = Icon.ExtractAssociatedIcon(processPath);
                    if (icon != null)
                    {
                        return Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            System.Windows.Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }
            }
            catch
            {
                // Fallback icon is used below.
            }

            return Imaging.CreateBitmapSourceFromHIcon(
                SystemIcons.Application.Handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            OpenRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenItem_Click(object? sender, EventArgs e)
        {
            OpenRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExitItem_Click(object? sender, EventArgs e)
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SettingsItem_Click(object? sender, EventArgs e)
        {
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
