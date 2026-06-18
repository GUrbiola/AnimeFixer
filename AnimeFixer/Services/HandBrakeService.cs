using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AnimeFixer.Models;

namespace AnimeFixer.Services
{
    public class HandBrakeService
    {
        private static readonly Regex _progressRx =
            new Regex(@"([\d.]+) %", RegexOptions.Compiled);

        public string ResolvedPath { get; private set; }

        // Stores the candidate path. Empty/null configuredPath means "use PATH fallback".
        // Does NOT verify the file exists — call TestVersion() for that.
        public void Resolve(string configuredPath)
        {
            ResolvedPath = string.IsNullOrWhiteSpace(configuredPath)
                ? "HandBrakeCLI.exe"
                : configuredPath;
        }

        // Runs --version. Returns (true, version) on success; (false, errorMessage) on failure.
        public bool TestVersion(out string version, out string error)
        {
            version = null;
            error   = null;

            if (string.IsNullOrWhiteSpace(ResolvedPath))
            {
                error = "Path not set — call Resolve() first.";
                return false;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName               = ResolvedPath,
                    Arguments              = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };

                using (var p = Process.Start(psi))
                {
                    string stdout = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    if (p.ExitCode == 0)
                    {
                        var lines = stdout.Split(
                            new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        version = lines.Length > 0 ? lines[0].Trim() : "OK";
                        return true;
                    }

                    error = "Found but failed to execute — check permissions.";
                    return false;
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                if (!string.IsNullOrWhiteSpace(ResolvedPath) &&
                    ResolvedPath != "HandBrakeCLI.exe" &&
                    !File.Exists(ResolvedPath))
                {
                    error = "File not found at specified path.";
                }
                else
                {
                    error = "HandBrakeCLI.exe not found — set path or add to PATH.";
                }
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // Compresses one file. Blocks until done or cancelled.
        // Progress callback receives 0–100 float. logLine receives each stdout/stderr line.
        // Throws OperationCanceledException if ct is cancelled.
        // Returns false and sets errorMessage on non-zero exit code.
        public bool Compress(
            CompressFile      file,
            Action<string>    logLine,
            Action<float>     onProgress,
            CancellationToken ct,
            out string        errorMessage)
        {
            errorMessage = null;

            var outputPath = file.OutputPath;
            var args = string.Format(
                "-i \"{0}\" -o \"{1}\" --preset \"H.264 MKV 1080p30\" " +
                "-e x264 -q 20 -r 30 --pfr " +
                "--all-audio -A copy " +
                "--all-subtitles",
                file.FilePath, outputPath);

            logLine?.Invoke("HandBrakeCLI " + args);

            var psi = new ProcessStartInfo
            {
                FileName               = ResolvedPath,
                Arguments              = args,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var errorBuf = new StringBuilder();

            using (var p = Process.Start(psi))
            {
                p.OutputDataReceived += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    logLine?.Invoke(e.Data);
                    var m = _progressRx.Match(e.Data);
                    if (m.Success && float.TryParse(
                            m.Groups[1].Value,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out float pct))
                    {
                        onProgress?.Invoke(pct);
                    }
                };

                p.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuf.AppendLine(e.Data);
                        logLine?.Invoke(e.Data);
                    }
                };

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                while (!p.WaitForExit(500))
                {
                    if (!ct.IsCancellationRequested) continue;
                    try { p.Kill(); } catch { }
                    try { if (File.Exists(outputPath)) File.Delete(outputPath); } catch { }
                    ct.ThrowIfCancellationRequested();
                }

                if (p.ExitCode == 0 && File.Exists(outputPath) &&
                    new FileInfo(outputPath).Length > 0)
                {
                    file.OutputSizeBytes = new FileInfo(outputPath).Length;
                    return true;
                }

                try { if (File.Exists(outputPath)) File.Delete(outputPath); } catch { }
                errorMessage = errorBuf.Length > 0
                    ? errorBuf.ToString().Trim()
                    : string.Format("Exit code {0}", p.ExitCode);
                return false;
            }
        }
    }
}
