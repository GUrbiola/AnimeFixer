namespace AnimeFixer.Models
{
    public class SubtitleTrack
    {
        public int    Index    { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Title    { get; set; } = string.Empty;
        public string Codec    { get; set; } = string.Empty;
        public bool   Selected { get; set; } = true;

        public string DisplayTitle =>
            string.IsNullOrEmpty(Title) ? Language : $"{Language} - {Title}";

        public string SuggestedFileSuffix
        {
            get
            {
                var lang = string.IsNullOrEmpty(Language) ? "und" : Language.ToLowerInvariant();
                if (string.IsNullOrEmpty(Title))
                    return lang;
                var safeTitle = Title.ToLowerInvariant()
                    .Replace(" ", "")
                    .Replace("/", "")
                    .Replace("\\", "");
                return $"{lang}.{safeTitle}";
            }
        }
    }
}
