using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OmniTransfer;

namespace OmniTransfer
{
    public partial class TransferWindow : Window
    {
        private List<string> _sourcePaths;
        private AppSettings _settings;
        private string? _selectedDestination;

        // update the constructor to receive the list of paths
        public TransferWindow(List<string> paths)
        {
            InitializeComponent();
            _sourcePaths = paths;

            // Load settings and calculate CPU recommendation
            _settings = SettingsManager.Load();
            InitializeUiFromSettings();

            // show how many files were selected
            Title = $"Transferring {_sourcePaths.Count} items...";
        }
       

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Destination for OmniTransfer",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedDestination = dialog.FolderName;
                TxtDestination.Text = _selectedDestination;

                // Only allow clicking "Start" if valid path
                ChkSetDefault.IsEnabled = true;
                BtnStart.IsEnabled = true;
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedDestination)) return;

            BtnStart.IsEnabled = false; // Prevent double-clicking
            BtnBrowse.IsEnabled = false;

            // trigger Robocopy logic
            await StartRobocopyTask();
        }
        private async Task StartRobocopyTask()
        {
            // 1. Sanitize the destination once before the loop
            string cleanDestination = _selectedDestination?.TrimEnd('\\') ?? string.Empty;
            string flags = $"/MT:{_settings.MaxThreads} /Z /XJ /R:{_settings.Retries} /W:{_settings.WaitTime}";
            int _itemsProcessed, _totalItems;

            _itemsProcessed = 0;
            _totalItems = _sourcePaths.Count;
            // Set initial text 
            Dispatcher.Invoke(() => TxtCounter.Text = $"0 / {_totalItems} items moved");

            foreach (var sourcePath in _sourcePaths)
            {
                string arguments = "";

                // Check if the item is a Directory (Folder)
                if (System.IO.Directory.Exists(sourcePath))
                {
                    // Get just the folder name 
                    string folderName = System.IO.Path.GetFileName(sourcePath);
                    // Create the full destination path 
                    string targetFolder = System.IO.Path.Combine(cleanDestination, folderName);

                    // /E = All subfolders (including empty)
                    // /MT:8 = 8 Threads
                    // /XJ = Exclude Junctions (prevents infinite loops in Windows system folders)
                    arguments = $"\"{sourcePath}\" \"{targetFolder}\" /E " + flags;
                }
                else
                {
                    // single file
                    string directory = System.IO.Path.GetDirectoryName(sourcePath) ?? "";
                    string fileName = System.IO.Path.GetFileName(sourcePath);

                    arguments = $"\"{directory}\" \"{cleanDestination}\" \"{fileName}\" " + flags;
                }

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "robocopy.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Debug.WriteLine($"Executing: robocopy {startInfo.Arguments}");
                using (var process = new System.Diagnostics.Process { StartInfo = startInfo })
                {
                    process.Start();

                    // while(true) loop with an async read
                    while (true)
                    {
                        // ReadLineAsync
                        string? line = await process.StandardOutput.ReadLineAsync();

                        // ReadLineAsync returns null close process stream 
                        if (line == null) break;

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            UpdateStatus(line);
                        }
                    }
                    await process.WaitForExitAsync();
                    int exitCode = process.ExitCode;
                    if (exitCode >= 8)
                    {
                        MessageBox.Show($"Serious error detected in Robocopy. Error Code: {exitCode}");
                    }
                }
                _itemsProcessed++;
                Dispatcher.Invoke(() =>
                {
                    TxtCounter.Text = $"{_itemsProcessed} / {_totalItems} items moved";
                });
            }

            MessageBox.Show("Transfer Complete!", "OmniTransfer", MessageBoxButton.OK, MessageBoxImage.Information);
            Dispatcher.Invoke(() =>
            {
                TxtCounter.Text = "COMPLETE";
                TxtCounter.Foreground = System.Windows.Media.Brushes.Green;
            });
            //this.Close();
        }


        private void InitializeUiFromSettings()
        {
            int recThreads = Environment.ProcessorCount * 2;
            LblThreads.Text = $"Max Threads (Recommended: {Math.Min(recThreads, 128)}):";

            SldThreads.Value = _settings.MaxThreads;
            SldRetries.Value = _settings.Retries;
            SldWait.Value = _settings.WaitTime;

            if (!string.IsNullOrEmpty(_settings.DefaultDestination) && System.IO.Directory.Exists(_settings.DefaultDestination))
            {
                _selectedDestination = _settings.DefaultDestination;
                TxtDestination.Text = _selectedDestination;
                BtnStart.IsEnabled = true;
                ChkSetDefault.IsEnabled = true;
                ChkSetDefault.IsChecked = true; // Visual confirmation saved by default
            }
            else
            {
                // not null but doesn't exist, reset the UI
                _selectedDestination = null;
                TxtDestination.Text = "Saved destination no longer available...";
                ChkSetDefault.IsChecked = false;
                ChkSetDefault.IsEnabled = false;
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _settings.MaxThreads = (int)SldThreads.Value;
            _settings.Retries = (int)SldRetries.Value;
            _settings.WaitTime = (int)SldWait.Value;

            if (ChkSetDefault.IsChecked == true && !string.IsNullOrEmpty(_selectedDestination))
                _settings.DefaultDestination = _selectedDestination;

            SettingsManager.Save(_settings);
            MessageBox.Show("Settings saved!");
        }



        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                // Add to the log
                FileListBox.Items.Add(message);
                FileListBox.ScrollIntoView(FileListBox.Items[FileListBox.Items.Count - 1]);

                // Look for percentage patterns
                var match = Regex.Match(message, @"(\d+(\.\d+)?)%");
                if (match.Success)
                {
                    if (double.TryParse(match.Groups[1].Value, out double percent))
                    {
                        TransferProgressBar.Value = percent;
                        TxtPercent.Text = $"{Math.Round(percent)}%";
                    }
                }
            });
        }
    }
}
