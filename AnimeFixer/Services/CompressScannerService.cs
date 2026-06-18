using System;
using System.Collections.Generic;
using System.IO;
using AnimeFixer.Models;

namespace AnimeFixer.Services
{
    public class CompressScannerService
    {
        private static readonly string[] SupportedExtensions =
            { ".mkv", ".mp4", ".m4v", ".mov" };

        public List<CompressFile> Scan(
            string         rootFolder,
            long           thresholdBytes,
            Action<string> onFileFound = null)
        {
            var files = new List<CompressFile>();

            try
            {
                foreach (var filePath in Directory.EnumerateFiles(
                    rootFolder, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var ext = Path.GetExtension(filePath).ToLowerInvariant();
                        bool supported = false;
                        foreach (var s in SupportedExtensions)
                        {
                            if (ext == s) { supported = true; break; }
                        }
                        if (!supported) continue;

                        var fi           = new FileInfo(filePath);
                        bool recommended = fi.Length >= thresholdBytes;

                        files.Add(new CompressFile
                        {
                            FilePath      = filePath,
                            FileSizeBytes = fi.Length,
                            IsRecommended = recommended,
                            IsSelected    = recommended
                        });

                        onFileFound?.Invoke(filePath);
                    }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (UnauthorizedAccessException) { }

            // Largest first
            files.Sort((a, b) => b.FileSizeBytes.CompareTo(a.FileSizeBytes));
            return files;
        }
    }
}
