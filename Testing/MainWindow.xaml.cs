using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Drawing;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;

namespace OverlayTestHarness
{
    public partial class MainWindow : Window
    {
        private IntPtr mainWindowHandle;
        private AutomationElement? automationElement;
        
        public MainWindow()
        {
            InitializeComponent();
            Title = "Overlay Test Harness";
            Width = 300;
            Height = 200;
            
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Create a dummy Win32 window
            CreateDummyWin32Window();
            
            // Set up automation to change focus
            SetupAutomation();
        }

        private void CreateDummyWin32Window()
        {
            // Create a basic WinForms window
            Form dummyForm = new Form
            {
                Text = "Dummy Target Window",
                Width = 400,
                Height = 300,
                StartPosition = FormStartPosition.CenterScreen
            };
            dummyForm.Show();
            dummyForm.TopMost = true;
            
            // Obtain the handle of the WinForms window
            mainWindowHandle = dummyForm.Handle;
        }

        private void SetupAutomation()
        {
            // Set up automation element
            automationElement = AutomationElement.FromHandle(mainWindowHandle);
            
            // Start a timer to automate focus changes
            var focusTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            focusTimer.Tick += FocusTimer_Tick;
            focusTimer.Start();
        }

        private void FocusTimer_Tick(object? sender, EventArgs e)
        {
            if (automationElement != null)
            {
                // Try to set focus to the dummy window
                try
                {
                    var pattern = (WindowPattern)automationElement.GetCurrentPattern(WindowPattern.Pattern);
                    pattern.SetWindowVisualState(WindowVisualState.Normal);
                    pattern.WaitForInputIdle(3000);
                    Application.Current.MainWindow?.Activate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Automation failed: " + ex.Message);
                }
            }
        }
    }
}
