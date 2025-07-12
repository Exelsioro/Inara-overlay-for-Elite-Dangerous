using System.Windows;

namespace OverlayTestHarness
{
    public partial class TestHarnessApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize logger
            Logger.Logger.Initialize("TestHarness", "logs");
            Logger.Logger.Info("Test Harness Application Starting");
        }
    }
}
