using System;
using System.Collections.Generic;
using System.IO;
using AnimeFixer.Models;

namespace AnimeFixer.Services
{
    public class ScannerService
    {
        private static readonly string[] SupportedExtensions = { ".mp4", ".m4v", ".mov" };

        public List<MediaFile> Scan(string rootFolder, Action<string> onFileFound = null)
        {
            var files = new List<MediaFile>();

            try
            {
                var allFiles = Directory.EnumerateFiles(rootFolder, "*.*", SearchOption.AllDirectories);

                foreach (var filePath in allFiles)
                {
                    try
                    {
                        var ext = Path.GetExtension(filePath).ToLowerInvariant();

                        // Check if extension is supported
                        bool isSupported = false;
                        foreach (var supportedExt in SupportedExtensions)
                        {
                            if (ext == supportedExt)
                            {
                                isSupported = true;
                                break;
                            }
                        }

                        if (!isSupported)
                            continue;

                        var fileInfo = new FileInfo(filePath);
                        var mediaFile = new MediaFile
                        {
                            FilePath = filePath,
                            Status = FileStatus.Pending,
                            FileSizeBytes = fileInfo.Length
                        };

                        files.Add(mediaFile);
                        onFileFound?.Invoke(filePath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip inaccessible files
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip inaccessible folders
            }

            return files;
        }

        public static string FormatSize(long bytes)
        {
            const long GB = 1024L * 1024L * 1024L;
            const long MB = 1024L * 1024L;
            const long KB = 1024L;

            if (bytes >= GB)
                return $"{bytes / (double)GB:F1} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:F1} MB";
            if (bytes >= KB)
                return $"{bytes / (double)KB:F1} KB";
            return $"{bytes} B";
        }
    }
}
