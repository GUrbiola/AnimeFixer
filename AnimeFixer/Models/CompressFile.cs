using System.IO;

namespace AnimeFixer.Models
{
    public enum CompressStatus
    {
        Pending,
        Processing,
        Done,
        Failed,
        Skipped
    }

    public class CompressFile
    {
        public string        FilePath        { get; set; }
        public long          FileSizeBytes   { get; set; }
        public bool          IsRecommended   { get; set; }
        public bool          IsSelected      { get; set; }
        public CompressStatus Status         { get; set; } = CompressStatus.Pending;
        public long          OutputSizeBytes { get; set; }
        public string        ErrorMessage    { get; set; }

        public string FileName       => Path.GetFileName(FilePath);
        public string FileDirectory  => Path.GetDirectoryName(FilePath);
        public string NameWithoutExt => Path.GetFileNameWithoutExtension(FilePath);
        public string Extension      => Path.GetExtension(FilePath);

        public string OutputPath =>
            Path.Combine(FileDirectory, NameWithoutExt + "_compressed" + Extension);
    }
}
