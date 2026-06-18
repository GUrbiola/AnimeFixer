using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AnimeFixer.Models;

namespace AnimeFixer.Services
{
    public class MkvProbeResult
    {
        public bool                IsHevc10bit    { get; set; }
        public List<SubtitleTrack> SubtitleTracks { get; set; } = new List<SubtitleTrack>();
    }

    public class FFmpegService
    {
        private static readonly Regex _rxFrame = new Regex(@"frame=\s*(\d+)",       RegexOptions.Compiled);
        private static readonly Regex _rxFps   = new Regex(@"fps=\s*([\d.]+)",      RegexOptions.Compiled);
        private static readonly Regex _rxSpeed = new Regex(@"speed=\s*([\d.]+)x",  RegexOptions.Compiled);

        private readonly string _ffprobePath;
        private readonly string _ffmpegPath;

        public FFmpegService(string ffprobePath, string ffmpegPath)
        {
            _ffprobePath = ffprobePath;
            _ffmpegPath = ffmpegPath;
        }

        public bool Validate(out string error)
        {
            error = null;

            if (!File.Exists(_ffprobePath))
            {
                error = "No se encontró ffprobe.exe en la ruta especificada.";
                return false;
            }

            if (!File.Exists(_ffmpegPath))
            {
                error = "No se encontró ffmpeg.exe en la ruta especificada.";
                return false;
            }

            return true;
        }

        private List<string> SplitStreamBlocks(string json)
        {
            var blocks = new List<string>();
            int depth = 0;
            bool inString = false;
            int blockStart = -1;

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];

                // Handle string escaping
                if (c == '\\' && inString && i + 1 < json.Length)
                {
                    i++; // Skip escaped character
                    continue;
                }

                // Toggle string state
                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                // Skip depth tracking inside strings
                if (inString)
                    continue;

                // Track brace depth
                if (c == '{')
                {
                    if (depth == 0)
                        blockStart = i;
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && blockStart >= 0)
                    {
                        blocks.Add(json.Substring(blockStart, i - blockStart + 1));
                        blockStart = -1;
                    }
                }
            }

            return blocks;
        }

        private string ParseStringField(string json, string field)
        {
            var pattern = $"\"{Regex.Escape(field)}\"\\s*:\\s*\"([^\"\\\\]*(?:\\\\.[^\"\\\\]*)*)\"";
            var match = Regex.Match(json, pattern);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private string ParseHandlerName(string json)
        {
            // First try to find handler_name directly in the stream
            string handlerName = ParseStringField(json, "handler_name");
            if (!string.IsNullOrEmpty(handlerName))
                return handlerName;

            // If not found, try to extract from tags section
            var tagsMatch = Regex.Match(json, "\"tags\"\\s*:\\s*\\{([^}]*)\\}");
            if (tagsMatch.Success)
            {
                var tagsContent = tagsMatch.Groups[1].Value;
                handlerName = ParseStringField("{" + tagsContent + "}", "handler_name");
                if (!string.IsNullOrEmpty(handlerName))
                    return handlerName;
            }

            return string.Empty;
        }

        private int ParseIntField(string json, string field)
        {
            var pattern = $"\"{Regex.Escape(field)}\"\\s*:\\s*(\\d+)";
            var match = Regex.Match(json, pattern);
            return match.Success && int.TryParse(match.Groups[1].Value, out int v) ? v : -1;
        }

        private ProbeResult ParseProbeOutput(string json, Action<string> debugLog = null)
        {
            var result = new ProbeResult();

            // Extract just the streams array
            string streamsJson = json;
            int streamsStart = json.IndexOf("\"streams\"");
            if (streamsStart >= 0)
            {
                int bracketStart = json.IndexOf("[", streamsStart);
                if (bracketStart >= 0)
                {
                    int depth = 0;
                    int bracketEnd = -1;
                    for (int i = bracketStart; i < json.Length; i++)
                    {
                        if (json[i] == '[') depth++;
                        else if (json[i] == ']') depth--;
                        if (depth == 0)
                        {
                            bracketEnd = i;
                            break;
                        }
                    }
                    if (bracketEnd > bracketStart)
                    {
                        streamsJson = json.Substring(bracketStart, bracketEnd - bracketStart + 1);
                        debugLog?.Invoke($"      Extracted streams array ({streamsJson.Length} bytes)");
                    }
                }
            }

            var blocks = SplitStreamBlocks(streamsJson);
            debugLog?.Invoke($"      JSON Parser: Found {blocks.Count} stream block(s)");

            foreach (var block in blocks)
            {
                int index = ParseIntField(block, "index");
                string codecType = ParseStringField(block, "codec_type");
                string codecName = ParseStringField(block, "codec_name");
                string codecTagString = ParseStringField(block, "codec_tag_string");
                string handlerName = ParseHandlerName(block);
                int codecId = ParseIntField(block, "codec_id");

                var stream = new StreamInfo
                {
                    Index = index,
                    CodecType = codecType,
                    CodecName = codecName,
                    CodecTagString = codecTagString,
                    HandlerName = handlerName,
                    CodecId = codecId
                };

                result.Streams.Add(stream);

                // Debug logging with validation details
                string debugInfo = $"      Stream#{index}: type={codecType}, codec={codecName}, tag={codecTagString}";
                if (codecId >= 0)
                    debugInfo += $", codecId={codecId}";
                if (!string.IsNullOrEmpty(handlerName))
                    debugInfo += $", handler={handlerName}";

                // Validation check
                bool isInvalid = stream.IsInvalid;
                if (isInvalid)
                {
                    debugInfo += " [INVALID]";
                }
                debugLog?.Invoke(debugInfo);
            }

            return result;
        }

        public ProbeResult Probe(string filePath, Action<string> debugLog = null)
        {
            // Try with comprehensive show_format and show_streams
            var args = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"";
            string output = RunProcess(_ffprobePath, args, out int exitCode);

            if (exitCode != 0)
            {
                throw new Exception($"ffprobe failed with exit code {exitCode}: {output}");
            }

            // Log raw JSON (first 1000 chars for preview)
            debugLog?.Invoke($"      [RAW JSON - {output.Length} bytes] {(output.Length > 1000 ? output.Substring(0, 1000) + "..." : output)}");

            return ParseProbeOutput(output, debugLog);
        }

        public MkvProbeResult ProbeMkv(string filePath, Action<string> debugLog = null)
        {
            var result = new MkvProbeResult();

            // Probe video stream 0 for HEVC + pix_fmt
            var videoArgs = $"-v warning -select_streams v:0 -show_entries stream=codec_name,pix_fmt -of json \"{filePath}\"";
            string videoJson = RunProcess(_ffprobePath, videoArgs, out int videoExit);
            debugLog?.Invoke($"[MKV probe video] exit={videoExit}");

            if (videoExit == 0)
            {
                string codecName = ParseStringField(videoJson, "codec_name");
                string pixFmt    = ParseStringField(videoJson, "pix_fmt");
                result.IsHevc10bit = codecName == "hevc" && pixFmt == "yuv420p10le";
                debugLog?.Invoke($"  codec={codecName} pix_fmt={pixFmt} isHevc10bit={result.IsHevc10bit}");
            }

            // Probe subtitle streams
            var subArgs = $"-v warning -select_streams s -show_entries stream=index,codec_name:stream_tags=language,title -of json \"{filePath}\"";
            string subJson = RunProcess(_ffprobePath, subArgs, out int subExit);
            debugLog?.Invoke($"[MKV probe subs] exit={subExit}");

            if (subExit == 0)
            {
                string subStreams = subJson;
                int subStreamsIdx = subJson.IndexOf("\"streams\"");
                if (subStreamsIdx >= 0)
                {
                    int bracketStart = subJson.IndexOf("[", subStreamsIdx);
                    if (bracketStart >= 0)
                    {
                        int depth2 = 0, bracketEnd = -1;
                        for (int i = bracketStart; i < subJson.Length; i++)
                        {
                            if (subJson[i] == '[') depth2++;
                            else if (subJson[i] == ']') depth2--;
                            if (depth2 == 0) { bracketEnd = i; break; }
                        }
                        if (bracketEnd > bracketStart)
                            subStreams = subJson.Substring(bracketStart, bracketEnd - bracketStart + 1);
                    }
                }

                var blocks = SplitStreamBlocks(subStreams);
                foreach (var block in blocks)
                {
                    int    index = ParseIntField(block, "index");
                    string codec = ParseStringField(block, "codec_name");
                    string lang  = string.Empty;
                    string title = string.Empty;

                    var tagsMatch = Regex.Match(block, "\"tags\"\\s*:\\s*\\{([^}]*)\\}");
                    if (tagsMatch.Success)
                    {
                        var tagsContent = "{" + tagsMatch.Groups[1].Value + "}";
                        lang  = ParseStringField(tagsContent, "language");
                        title = ParseStringField(tagsContent, "title");
                    }

                    result.SubtitleTracks.Add(new SubtitleTrack
                    {
                        Index    = index,
                        Language = lang,
                        Title    = title,
                        Codec    = codec,
                        Selected = true
                    });

                    debugLog?.Invoke($"  sub#{index}: codec={codec} lang={lang} title={title}");
                }
            }

            return result;
        }

        private string RunProcess(string exe, string args, out int exitCode)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            using (var process = Process.Start(psi))
            {
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorBuilder.AppendLine(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }

            return outputBuilder.ToString() + errorBuilder.ToString();
        }

        private string RunProcessCancellable(string exe, string args, CancellationToken ct,
            Action<string> onLine, out int exitCode)
        {
            var psi = new ProcessStartInfo
            {
                FileName               = exe,
                Arguments              = args,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var outputBuilder = new StringBuilder();
            exitCode = -1;

            using (var process = Process.Start(psi))
            {
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                        onLine?.Invoke(e.Data);
                    }
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                        onLine?.Invoke(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.WaitForExit(500))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { process.Kill(); } catch { }
                        ct.ThrowIfCancellationRequested();
                    }
                }

                exitCode = process.ExitCode;
            }

            return outputBuilder.ToString();
        }

        private string ExtractFFmpegError(string stderr)
        {
            var lines = stderr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Scan lines in reverse
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("[") || line.Contains("Error") || line.Contains("Invalid") || line.Contains("failed"))
                    return line;
            }

            // Fall back to last 300 chars
            if (stderr.Length > 300)
                return stderr.Substring(stderr.Length - 300);

            return stderr;
        }

        private void CleanupTemp(string tempPath)
        {
            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch
            {
                // Silently ignore cleanup errors
            }
        }

        public bool Remux(MediaFile file, Action<string> logLine, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                var tempPath = file.FilePath + ".mp4fixer_tmp.mp4";

                // Build -map arguments from valid streams
                var mapArgs = new StringBuilder();
                foreach (var stream in file.ProbeResult.ValidStreams)
                {
                    mapArgs.Append($" -map 0:{stream.Index}");
                }

                var args = $"-y -i \"{file.FilePath}\"{mapArgs} -c copy \"{tempPath}\"";
                logLine?.Invoke(args);

                string output = RunProcess(_ffmpegPath, args, out int exitCode);

                // Success check
                if (exitCode == 0 && File.Exists(tempPath) && new FileInfo(tempPath).Length > 0)
                {
                    var backupPath = file.FilePath + ".bak";

                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    File.Move(file.FilePath, backupPath);

                    if (File.Exists(file.FilePath))
                        File.Delete(file.FilePath);
                    File.Move(tempPath, file.FilePath);

                    File.Delete(backupPath);

                    return true;
                }
                else
                {
                    CleanupTemp(tempPath);
                    errorMessage = ExtractFFmpegError(output);
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        // Returns total video frame count. Falls back to duration × r_frame_rate if nb_frames is unset.
        // Returns 0 when count cannot be determined (progress bar will stay hidden).
        public int GetTotalFrames(string filePath)
        {
            // First attempt: nb_frames stored in stream metadata
            var args1 = $"-v warning -select_streams v:0 -show_entries stream=nb_frames -of csv=p=0 \"{filePath}\"";
            string out1 = RunProcess(_ffprobePath, args1, out int exit1);
            if (exit1 == 0)
            {
                var line = out1.Trim();
                if (int.TryParse(line, out int n) && n > 0)
                    return n;
            }

            // Second attempt: duration × r_frame_rate (common with HEVC MKV)
            var args2 = $"-v warning -select_streams v:0 -show_entries stream=duration,r_frame_rate -of json \"{filePath}\"";
            string out2 = RunProcess(_ffprobePath, args2, out int exit2);
            if (exit2 == 0)
            {
                var mDur  = Regex.Match(out2, "\"duration\"\\s*:\\s*\"([\\d.]+)\"");
                var mRate = Regex.Match(out2, "\"r_frame_rate\"\\s*:\\s*\"(\\d+)/(\\d+)\"");
                if (mDur.Success && mRate.Success)
                {
                    double duration = double.Parse(mDur.Groups[1].Value,
                        System.Globalization.CultureInfo.InvariantCulture);
                    double num = double.Parse(mRate.Groups[1].Value);
                    double den = double.Parse(mRate.Groups[2].Value);
                    if (den > 0 && duration > 0)
                        return (int)(duration * num / den);
                }
            }

            return 0;
        }

        public bool ReencodeToH264(MkvMediaFile file, string preset, int totalFrames,
            bool useNvenc,
            Action<string> logLine, Action<int, double, double> onProgress,
            CancellationToken ct, out string errorMessage)
        {
            errorMessage = null;

            var dir        = Path.GetDirectoryName(file.FilePath);
            var nameNoExt  = Path.GetFileNameWithoutExtension(file.FilePath);
            var outputPath = Path.Combine(dir, nameNoExt + ".roku.mp4");

            if (string.IsNullOrWhiteSpace(preset)) preset = "veryfast";

            try
            {
                // Output is MP4 (not MKV) because Roku seeks MP4 via pure byte-range arithmetic
                // using the moov atom at the front (-movflags +faststart). MKV seeking requires
                // Jellyfin to consult the seek table server-side on every seek, which stalls the
                // Roku player. MP4+faststart eliminates that round-trip entirely.
                //
                // -sn: drop subtitle streams — MP4 doesn't support ASS; extract subs separately as .srt.
                // -force_key_frames every 2 s: caps worst-case seek stall to 2 s.
                // -movflags +faststart: writes moov atom before mdat so HTTP clients can seek
                //   without downloading the whole file first.
                const string keyframeArgs = "-force_key_frames \"expr:gte(t,n_forced*2)\"";

                string videoArgs = useNvenc
                    ? string.Format("-c:v h264_nvenc -cq 20 -preset p4 -profile:v high -level 4.1 -pix_fmt yuv420p {0}", keyframeArgs)
                    : string.Format("-c:v libx264 -crf 23 -preset {0} -profile:v high -level 4.1 -pix_fmt yuv420p {1}", preset, keyframeArgs);

                var args = string.Format(
                    "-y -nostats -i \"{0}\" {1} -c:a copy -sn -movflags +faststart -progress pipe:1 \"{2}\"",
                    file.FilePath, videoArgs, outputPath);

                logLine?.Invoke($"ffmpeg {args}");

                // State accumulator for -progress pipe:1 key=value blocks
                int    lastFrame = 0;
                double lastFps   = 0;
                double lastSpeed = 0;

                string result = RunProcessCancellable(_ffmpegPath, args, ct, line =>
                {
                    // Progress lines from stdout (-progress pipe:1): key=value, no spaces
                    int eq = line.IndexOf('=');
                    if (eq > 0 && eq == line.LastIndexOf('=') && !line.Contains(" "))
                    {
                        string key = line.Substring(0, eq);
                        string val = line.Substring(eq + 1).TrimEnd('x');

                        switch (key)
                        {
                            case "frame":
                                int.TryParse(val, out lastFrame);
                                break;
                            case "fps":
                                double.TryParse(val,
                                    System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out lastFps);
                                break;
                            case "speed":
                                double.TryParse(val,
                                    System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out lastSpeed);
                                break;
                            case "progress":
                                onProgress?.Invoke(lastFrame, lastFps, lastSpeed);
                                break;
                        }
                        return; // don't echo raw key=value to verbose log
                    }

                    // Everything else (stderr: codec info, warnings, errors) → verbose log
                    logLine?.Invoke(line);
                }, out int exitCode);

                if (exitCode == 0 && File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                {
                    logLine?.Invoke(string.Format(
                        "Roku-friendly MP4 creado: {0}", Path.GetFileName(outputPath)));
                    return true;
                }

                CleanupTemp(outputPath);
                errorMessage = ExtractFFmpegError(result);
                return false;
            }
            catch (OperationCanceledException)
            {
                CleanupTemp(outputPath);
                throw;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public bool ExtractSubtitle(string inputPath, SubtitleTrack track, string outputPath,
            Action<string> logLine, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                var args = $"-y -i \"{inputPath}\" -map 0:{track.Index} -c:s srt \"{outputPath}\"";
                logLine?.Invoke($"ffmpeg {args}");

                string result = RunProcess(_ffmpegPath, args, out int exitCode);

                if (exitCode == 0 && File.Exists(outputPath) && new FileInfo(outputPath).Length > 0)
                {
                    CleanSrtFile(outputPath);
                    return true;
                }

                errorMessage = ExtractFFmpegError(result);
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        // Strips ASS override codes ({...}) and non-standard HTML tags left behind by
        // ffmpeg's ASS→SRT conversion. Roku and most SRT players only honour <b>, <i>, <u>.
        private void CleanSrtFile(string path)
        {
            try
            {
                string text = File.ReadAllText(path, System.Text.Encoding.UTF8);

                // Remove all ASS override blocks: {\an7}, {\pos(x,y)}, {\b1}, etc.
                text = Regex.Replace(text, @"\{[^}]*\}", string.Empty);

                // Strip any HTML tag that is NOT a basic formatting tag.
                // Keep: <b>, </b>, <i>, </i>, <u>, </u>  (case-insensitive, no attributes)
                text = Regex.Replace(text,
                    @"<(?!/?(?:b|i|u)\b)[^>]+>",
                    string.Empty,
                    RegexOptions.IgnoreCase);

                File.WriteAllText(path, text, System.Text.Encoding.UTF8);
            }
            catch
            {
                // Cleaning is best-effort; leave the file as-is if anything fails.
            }
        }
    }
}
