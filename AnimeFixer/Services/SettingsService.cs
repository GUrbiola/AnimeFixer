using System;
using System.IO;

namespace AnimeFixer.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;

        public string FFmpegFolder     { get; set; } = string.Empty;
        public string LastFolder       { get; set; } = string.Empty;
        public string HandBrakeCLIPath { get; set; } = string.Empty;

        public string FFprobePath => Path.Combine(FFmpegFolder, "ffprobe.exe");
        public string FFmpegPath  => Path.Combine(FFmpegFolder, "ffmpeg.exe");

        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MP4Fixer");

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            _settingsPath = Path.Combine(appDataPath, "settings.ini");
            Load();
        }

        public void Load()
        {
            if (!File.Exists(_settingsPath))
                return;

            try
            {
                foreach (var line in File.ReadAllLines(_settingsPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    var parts = line.Split(new[] { '=' }, 2, StringSplitOptions.None);
                    if (parts.Length != 2)
                        continue;

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "FFmpegFolder":
                            FFmpegFolder = value;
                            break;
                        case "LastFolder":
                            LastFolder = value;
                            break;
                        case "HandBrakeCLIPath":
                            HandBrakeCLIPath = value;
                            break;
                    }
                }
            }
            catch
            {
                // If load fails, silently use defaults
            }
        }

        public void Save()
        {
            try
            {
                var lines = new[]
                {
                    $"FFmpegFolder={FFmpegFolder}",
                    $"LastFolder={LastFolder}",
                    $"HandBrakeCLIPath={HandBrakeCLIPath}"
                };

                File.WriteAllLines(_settingsPath, lines);
            }
            catch
            {
                // Silently fail on save
            }
        }

        public static string TryAutoDetectFFmpegFolder()
        {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Jellyfin", "Server"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Jellyfin", "Server"),
                "C:\\Jellyfin",
                "C:\\ffmpeg\\bin",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin")
            };

            foreach (var folder in candidates)
            {
                var ffprobePath = Path.Combine(folder, "ffprobe.exe");
                var ffmpegPath = Path.Combine(folder, "ffmpeg.exe");

                if (File.Exists(ffprobePath) && File.Exists(ffmpegPath))
                    return folder;
            }

            return string.Empty;
        }
    }
}
