using System;
using System.Collections.Generic;
using System.IO;
using AnimeFixer.Models;

namespace AnimeFixer.Services
{
    public class MkvScannerService
    {
        public List<MkvMediaFile> Scan(string rootFolder, Action<string> onFileFound = null)
        {
            var files = new List<MkvMediaFile>();

            try
            {
                var allFiles = Directory.EnumerateFiles(rootFolder, "*.mkv",
                    SearchOption.AllDirectories);

                foreach (var filePath in allFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(filePath);
                        files.Add(new MkvMediaFile
                        {
                            FilePath      = filePath,
                            Status        = MkvFileStatus.Pending,
                            FileSizeBytes = fileInfo.Length
                        });
                        onFileFound?.Invoke(filePath);
                    }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (UnauthorizedAccessException) { }

            return files;
        }
    }
}
