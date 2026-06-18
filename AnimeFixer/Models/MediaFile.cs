using System;
using System.Collections.Generic;
using System.IO;

namespace AnimeFixer.Models
{
    public enum FileStatus
    {
        Pending,      // Initial state, not yet analyzed
        Analyzing,    // ffprobe running on this file
        Clean,        // No invalid streams found
        NeedsFixing,  // Invalid GPAC streams detected, ready to remux
        Fixing,       // ffmpeg remux in progress
        Fixed,        // Successfully remuxed and original replaced
        Failed,       // Remux failed; original file kept untouched
        Skipped       // No video/audio streams found, or user skipped
    }

    public class StreamInfo
    {
        public int    Index          { get; set; }
        public string CodecType      { get; set; }
        public string CodecName      { get; set; }
        public string CodecTagString { get; set; }
        public string HandlerName    { get; set; }
        public int    CodecId        { get; set; } = -1;

        public bool IsInvalid =>
            CodecType == "data" &&
            (CodecId == 0 || // Invalid codec ID
             CodecName == "none" || CodecName == "mp4s" || string.IsNullOrEmpty(CodecName) ||
             (HandlerName != null &&
              (HandlerName.Contains("GPAC") ||
               HandlerName.Contains("OD Handler") ||
               HandlerName.Contains("Scene Description"))));
    }

    public class ProbeResult
    {
        public List<StreamInfo> Streams        { get; set; } = new List<StreamInfo>();
        public bool             HasInvalidStreams => Streams.Exists(s => s.IsInvalid);
        public List<StreamInfo> ValidStreams      => Streams.FindAll(s => !s.IsInvalid);
        public List<StreamInfo> InvalidStreams    => Streams.FindAll(s => s.IsInvalid);
    }

    public class MediaFile
    {
        public string      FilePath      { get; set; }
        public string      FileName      => Path.GetFileName(FilePath);
        public FileStatus  Status        { get; set; } = FileStatus.Pending;
        public ProbeResult ProbeResult   { get; set; }
        public string      ErrorMessage  { get; set; }
        public long        FileSizeBytes { get; set; }

        public string StatusDisplay
        {
            get
            {
                switch (Status)
                {
                    case FileStatus.Pending:     return "Pendiente";
                    case FileStatus.Analyzing:   return "Analizando...";
                    case FileStatus.Clean:       return "✔ Sin problemas";
                    case FileStatus.NeedsFixing: return "⚠ Necesita corrección";
                    case FileStatus.Fixing:      return "Corrigiendo...";
                    case FileStatus.Fixed:       return "✔ Corregido";
                    case FileStatus.Failed:      return "✘ Falló (original conservado)";
                    case FileStatus.Skipped:     return "— Omitido";
                    default:                     return Status.ToString();
                }
            }
        }
    }
}
