using System;
using System.Windows.Forms;

namespace MockTargetApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Create main form
            var mainForm = new Form
            {
                Text = "Mock Target Application",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            // Create status label
            var statusLabel = new Label
            {
                Text = "Mock Target App Running - Use this for overlay testing",
                Location = new Point(10, 10),
                Size = new Size(380, 30),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Green
            };

            // Create button
            var button1 = new Button
            {
                Text = "Click Me!",
                Location = new Point(150, 100),
                Size = new Size(100, 50),
                BackColor = Color.LightBlue
            };

            button1.Click += (sender, e) => 
            {
                statusLabel.Text = $"Button clicked at {DateTime.Now:HH:mm:ss}";
                statusLabel.ForeColor = Color.Blue;
            };

            // Set up timer for some activity
            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += (sender, e) => 
            {
                mainForm.Text = $"Mock Target Application - {DateTime.Now:HH:mm:ss}";
            };
            timer.Start();

            // Add controls to form
            mainForm.Controls.Add(statusLabel);
            mainForm.Controls.Add(button1);

            // Handle form closing
            mainForm.FormClosing += (sender, e) => 
            {
                timer?.Stop();
                timer?.Dispose();
            };

            // Run the application
            Application.Run(mainForm);
        }
    }
}
