using System;
using System.IO;
using System.Text.Json;

namespace ClientPrinterTray
{
    public class AppSettings
    {
        public int Port { get; set; } = 5100;
        public string? DefaultPrinter { get; set; }
        public string DbConnection { get; set; } =
            "Host=localhost;Database=print;Username=postgres;Password=postgres";

        private static readonly string SettingsFile =
            Path.Combine(AppContext.BaseDirectory, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
            }

            return new AppSettings();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(SettingsFile, json);
        }
    }
}
