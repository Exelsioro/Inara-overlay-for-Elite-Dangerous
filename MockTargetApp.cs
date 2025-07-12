using System;
using System.Threading;
using System.Windows.Forms;

namespace MockTargetApp
{
    /// <summary>
    /// Simple mock target application that can be used to test overlay behavior
    /// </summary>
    public class MockTargetApp
    {
        private Form mainForm;
        private Button button1;
        private Label statusLabel;
        private Timer timer;

        public MockTargetApp()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Create main form
            mainForm = new Form
            {
                Text = "Mock Target Application",
                Size = new System.Drawing.Size(400, 300),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            // Create status label
            statusLabel = new Label
            {
                Text = "Mock Target App Running - Use this for overlay testing",
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(380, 30),
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.Green
            };

            // Create button
            button1 = new Button
            {
                Text = "Click Me!",
                Location = new System.Drawing.Point(150, 100),
                Size = new System.Drawing.Size(100, 50),
                BackColor = System.Drawing.Color.LightBlue
            };

            button1.Click += Button1_Click;

            // Add controls to form
            mainForm.Controls.Add(statusLabel);
            mainForm.Controls.Add(button1);

            // Set up timer for some activity
            timer = new Timer();
            timer.Interval = 2000; // 2 seconds
            timer.Tick += Timer_Tick;
            timer.Start();

            // Handle form closing
            mainForm.FormClosing += MainForm_FormClosing;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            statusLabel.Text = $"Button clicked at {DateTime.Now:HH:mm:ss}";
            statusLabel.ForeColor = System.Drawing.Color.Blue;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Just update the window title with timestamp to show activity
            mainForm.Text = $"Mock Target Application - {DateTime.Now:HH:mm:ss}";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer?.Stop();
            timer?.Dispose();
        }

        public void Show()
        {
            mainForm.Show();
        }

        public void Run()
        {
            Application.Run(mainForm);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var app = new MockTargetApp();
            app.Run();
        }
    }
}
