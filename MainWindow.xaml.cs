using Microsoft.Win32;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;

namespace OmniTransfer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = $"OmniTransfer v{GetAppVersion()}";
            this.Loaded += (s, e) => CheckIntegrationStatus();
        }
        public string GetAppVersion()
        {
            // Pulls the version directly from Assembly metadata
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
        }
        private void CheckIntegrationStatus()
        {
            string menuName = "Transfer with OmniTransfer";
            using (RegistryKey? key = Registry.ClassesRoot.OpenSubKey($@"*\shell\{menuName}"))
            {
                if (key != null)
                {
                    StatusLabel.Text = "Status: Integrated";
                    StatusLabel.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    StatusLabel.Text = "Status: Not Integrated";
                }
            }
        }
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private void NotifyShell()
        {
            // SHCNE_ASSOCCHANGED = 0x08000000, SHCNF_IDLIST = 0x0000
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        private void InstallShell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string exePath = Environment.ProcessPath ?? string.Empty;
                string menuName = "Transfer with OmniTransfer";
                string command = $"\"{exePath}\" \"%1\"";

                // Register for Files
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"*\shell\" + menuName))
                {
                    key.SetValue("", "Transfer with OmniTransfer");
                    using (RegistryKey cmdKey = key.CreateSubKey("command"))
                    {
                        cmdKey.SetValue("", command);
                    }
                }

                // Register for Folders
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"Directory\shell\" + menuName))
                {
                    key.SetValue("", "Transfer with OmniTransfer");
                    using (RegistryKey cmdKey = key.CreateSubKey("command"))
                    {
                        cmdKey.SetValue("", command);
                    }
                }

                StatusLabel.Text = "Installation Successful!";
                StatusLabel.Foreground = System.Windows.Media.Brushes.Green;
                NotifyShell();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Please run as Administrator.\n" + ex.Message);
            }
        }

        private void RemoveShell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string menuName = "Transfer with OmniTransfer";

                // 1. Remove from File shell
                Registry.ClassesRoot.DeleteSubKeyTree($@"*\shell\{menuName}", throwOnMissingSubKey: false);

                // 2. Remove from Directory shell
                Registry.ClassesRoot.DeleteSubKeyTree($@"Directory\shell\{menuName}", throwOnMissingSubKey: false);

                StatusLabel.Text = "Successfully removed from Shell.";
                StatusLabel.Foreground = System.Windows.Media.Brushes.OrangeRed;
                NotifyShell();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Please run as Administrator.\n" + ex.Message);
            }
        }
    }
}