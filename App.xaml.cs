using System.Configuration;
using System.Data;
using System.Windows;

namespace OmniTransfer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // If no arguments, Show Setup
            if (e.Args.Length == 0)
            {
                MainWindow setupWindow = new MainWindow();
                setupWindow.Show();
            }
            else
            {
                // Windows passed us files -> Show Transfer UI
                var paths = e.Args.ToList();
                TransferWindow transferWindow = new TransferWindow(paths);
                transferWindow.Show();
            }
        }
    }
}
