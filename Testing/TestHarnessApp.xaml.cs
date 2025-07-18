using System.Windows;

namespace OverlayTestHarness
{
    public partial class TestHarnessApp : System.Windows.Application
    {
        [System.STAThread]
        public static void Main()
        {
            TestHarnessApp app = new TestHarnessApp();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize logger
            Logger.Logger.Info("Test Harness Application Starting");
        }
    }
}
