using System;
using System.IO;
using System.Text.Json;

namespace OmniTransfer
{
    // 1. The Data Structure
    public class AppSettings
    {
        public string DefaultDestination { get; set; } = string.Empty;
        public int MaxThreads { get; set; } = 8;
        public int Retries { get; set; } = 0;
        public int WaitTime { get; set; } = 0;
    }

    // 2. The Logic to Save/Load
    public static class SettingsManager
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniTransfer");
        private static readonly string SettingsFile = Path.Combine(AppFolder, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFile)) return new AppSettings();
                string json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch { return new AppSettings(); }
        }

        public static void Save(AppSettings settings)
        {
            Directory.CreateDirectory(AppFolder);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
    }
}