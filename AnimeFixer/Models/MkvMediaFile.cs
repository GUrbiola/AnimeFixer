using System.Collections.Generic;
using System.IO;

namespace AnimeFixer.Models
{
    public enum MkvFileStatus
    {
        Pending,
        Analyzing,
        Clean,
        NeedsFixing,
        Processing,
        Done,
        Failed,
        Skipped
    }

    public enum MkvAction
    {
        None,
        ReencodeVideo,
        ExtractSubtitles,
        Both
    }

    public class MkvMediaFile
    {
        public string              FilePath       { get; set; }
        public string              FileName       => Path.GetFileName(FilePath);
        public long                FileSizeBytes  { get; set; }
        public MkvFileStatus       Status         { get; set; } = MkvFileStatus.Pending;
        public bool                IsHevc10bit    { get; set; }
        public List<SubtitleTrack> SubtitleTracks { get; set; } = new List<SubtitleTrack>();
        public MkvAction           Action         { get; set; } = MkvAction.None;
        public string              ErrorMessage   { get; set; }

        public string StatusDisplay
        {
            get
            {
                switch (Status)
                {
                    case MkvFileStatus.Pending:     return "Pendiente";
                    case MkvFileStatus.Analyzing:   return "Analizando...";
                    case MkvFileStatus.Clean:       return "✔ Sin HEVC 10-bit";
                    case MkvFileStatus.NeedsFixing: return "⚠ HEVC 10-bit detectado";
                    case MkvFileStatus.Processing:  return "Procesando...";
                    case MkvFileStatus.Done:        return "✔ Completado";
                    case MkvFileStatus.Failed:      return "✘ Error";
                    case MkvFileStatus.Skipped:     return "— Omitido";
                    default:                        return Status.ToString();
                }
            }
        }

        public string ActionDisplay
        {
            get
            {
                switch (Action)
                {
                    case MkvAction.ReencodeVideo:    return "Re-encode video";
                    case MkvAction.ExtractSubtitles: return "Extraer subtítulos";
                    case MkvAction.Both:             return "Ambos";
                    default:                         return "—";
                }
            }
        }
    }
}
