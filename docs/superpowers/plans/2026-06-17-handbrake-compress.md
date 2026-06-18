# HandBrake Large File Compression — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a "Compress Large Files" form to AnimeFixer that scans for large video files, presents them in a DataGridView with per-row checkboxes, and re-encodes selected files via HandBrakeCLI with real-time progress.

**Architecture:** New `CompressForm` follows the same top-panel + SplitContainer(DataGridView | log) + StatusStrip pattern as `MkvRokuForm`. A dedicated `HandBrakeService` wraps the CLI process with live stdout progress parsing. `CompressScannerService` performs a pure filesystem scan (no ffprobe). `SettingsService` gains a `HandBrakeCLIPath` property. Form1 launcher is widened to host a third panel.

**Tech Stack:** C# 7.3, .NET Framework 4.8, WinForms, HandBrakeCLI.exe (external), System.Diagnostics.Process, System.Text.RegularExpressions, DataGridView with DataGridViewCheckBoxColumn.

---

## File Map

| Action | File |
|---|---|
| **Create** | `AnimeFixer/Models/CompressFile.cs` |
| **Create** | `AnimeFixer/Services/HandBrakeService.cs` |
| **Create** | `AnimeFixer/Services/CompressScannerService.cs` |
| **Create** | `AnimeFixer/CompressForm.cs` |
| **Create** | `AnimeFixer/CompressForm.Designer.cs` |
| **Create** | `AnimeFixer/CompressForm.resx` |
| **Modify** | `AnimeFixer/Services/SettingsService.cs` — add `HandBrakeCLIPath` property |
| **Modify** | `AnimeFixer/Form1.cs` — add `BtnCompressFixer_Click` handler |
| **Modify** | `AnimeFixer/Form1.Designer.cs` — add third panel, widen form to 1030px |
| **Modify** | `AnimeFixer/AnimeFixer.csproj` — register new files |

---

### Task 1: CompressFile model

**Files:**
- Create: `AnimeFixer/AnimeFixer/Models/CompressFile.cs`

- [ ] **Step 1: Create the model**

```csharp
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
```

Save to `AnimeFixer\AnimeFixer\Models\CompressFile.cs`.

---

### Task 2: Extend SettingsService with HandBrakeCLIPath

**Files:**
- Modify: `AnimeFixer/AnimeFixer/Services/SettingsService.cs`

Current file has `FFmpegFolder` and `LastFolder`. We add `HandBrakeCLIPath`.

- [ ] **Step 1: Add the property after `LastFolder`**

In `SettingsService.cs`, after line 11 (`public string LastFolder ...`), add:

```csharp
public string HandBrakeCLIPath { get; set; } = string.Empty;
```

- [ ] **Step 2: Add the switch case in `Load()`**

Inside the `switch (key)` block (around line 48), add:

```csharp
case "HandBrakeCLIPath":
    HandBrakeCLIPath = value;
    break;
```

- [ ] **Step 3: Add the value in `Save()`**

Replace the `lines` array in `Save()` with:

```csharp
var lines = new[]
{
    $"FFmpegFolder={FFmpegFolder}",
    $"LastFolder={LastFolder}",
    $"HandBrakeCLIPath={HandBrakeCLIPath}"
};
```

---

### Task 3: HandBrakeService

**Files:**
- Create: `AnimeFixer/AnimeFixer/Services/HandBrakeService.cs`

- [ ] **Step 1: Create the service**

```csharp
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

        // Returns true if a candidate path was stored. Does NOT verify the file exists —
        // call TestVersion() for that. Empty/null configuredPath means "use PATH fallback".
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
            CompressFile file,
            Action<string> logLine,
            Action<float>  onProgress,
            CancellationToken ct,
            out string errorMessage)
        {
            errorMessage = null;

            var outputPath = file.OutputPath;
            var args = string.Format(
                "-i \"{0}\" -o \"{1}\" --preset \"H.264 MKV 1080p30\" " +
                "-e x264 -q 20 -r 30 --pfr " +
                "-a 1,2,3,4,5,6,7,8 -A copy " +
                "-s 1,2,3,4,5,6,7,8 -S copy " +
                "--all-audio --all-subtitles",
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
                    : $"Exit code {p.ExitCode}";
                return false;
            }
        }
    }
}
```

Save to `AnimeFixer\AnimeFixer\Services\HandBrakeService.cs`.

---

### Task 4: CompressScannerService

**Files:**
- Create: `AnimeFixer/AnimeFixer/Services/CompressScannerService.cs`

- [ ] **Step 1: Create the scanner**

```csharp
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
            string rootFolder,
            long   thresholdBytes,
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

                        var fi          = new FileInfo(filePath);
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
```

Save to `AnimeFixer\AnimeFixer\Services\CompressScannerService.cs`.

---

### Task 5: CompressForm Designer

**Files:**
- Create: `AnimeFixer/AnimeFixer/CompressForm.Designer.cs`
- Create: `AnimeFixer/AnimeFixer/CompressForm.resx`

The form mirrors `MkvRokuForm` layout: white top panel (158px) → 1px separator → SplitContainer filling the rest → StatusStrip. The top panel has 5 rows instead of 3.

**Top panel row layout (form width 1495px):**

| Row | y | Controls |
|---|---|---|
| 1 | 6 | HandBrakeCLI path label + textbox + Browse + Test + status label |
| 2 | 36 | Media folder label + textbox + Browse |
| 3 | 66 | Threshold label + textbox + "GB" label |
| 4 | 95 | Scan + Compress Selected + Check Recommended + Uncheck All + Cancel + progress bar |
| 5 | 131 | Status label (L+R) + verbose checkbox (R) |

- [ ] **Step 1: Create CompressForm.Designer.cs**

```csharp
namespace AnimeFixer
{
    partial class CompressForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._topPanel               = new System.Windows.Forms.Panel();
            this._lblHandBrakePath       = new System.Windows.Forms.Label();
            this._txtHandBrakePath       = new System.Windows.Forms.TextBox();
            this._btnBrowseHandBrake     = new System.Windows.Forms.Button();
            this._btnTestHandBrake       = new System.Windows.Forms.Button();
            this._lblHandBrakeStatus     = new System.Windows.Forms.Label();
            this._lblFolder              = new System.Windows.Forms.Label();
            this._txtFolder              = new System.Windows.Forms.TextBox();
            this._btnBrowseFolder        = new System.Windows.Forms.Button();
            this._lblThreshold           = new System.Windows.Forms.Label();
            this._txtThreshold           = new System.Windows.Forms.TextBox();
            this._lblGB                  = new System.Windows.Forms.Label();
            this._btnScan                = new System.Windows.Forms.Button();
            this._btnCompressSelected    = new System.Windows.Forms.Button();
            this._btnCheckRecommended    = new System.Windows.Forms.Button();
            this._btnUncheckAll          = new System.Windows.Forms.Button();
            this._btnCancel              = new System.Windows.Forms.Button();
            this._progressBarFile        = new System.Windows.Forms.ProgressBar();
            this._lblStatus              = new System.Windows.Forms.Label();
            this._chkVerboseLogging      = new System.Windows.Forms.CheckBox();
            this._splitContainer         = new System.Windows.Forms.SplitContainer();
            this._grid                   = new System.Windows.Forms.DataGridView();
            this._gridColSelected        = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._gridColFileName        = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._gridColFullPath        = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._gridColSize            = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._gridColRecommended     = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._gridColStatus          = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._txtLog                 = new System.Windows.Forms.TextBox();
            this._statusStrip            = new System.Windows.Forms.StatusStrip();
            this._stripLabel             = new System.Windows.Forms.ToolStripStatusLabel();
            this._sep                    = new System.Windows.Forms.Panel();
            this._topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // _topPanel
            //
            this._topPanel.BackColor = System.Drawing.Color.White;
            this._topPanel.Controls.Add(this._lblHandBrakePath);
            this._topPanel.Controls.Add(this._txtHandBrakePath);
            this._topPanel.Controls.Add(this._btnBrowseHandBrake);
            this._topPanel.Controls.Add(this._btnTestHandBrake);
            this._topPanel.Controls.Add(this._lblHandBrakeStatus);
            this._topPanel.Controls.Add(this._lblFolder);
            this._topPanel.Controls.Add(this._txtFolder);
            this._topPanel.Controls.Add(this._btnBrowseFolder);
            this._topPanel.Controls.Add(this._lblThreshold);
            this._topPanel.Controls.Add(this._txtThreshold);
            this._topPanel.Controls.Add(this._lblGB);
            this._topPanel.Controls.Add(this._btnScan);
            this._topPanel.Controls.Add(this._btnCompressSelected);
            this._topPanel.Controls.Add(this._btnCheckRecommended);
            this._topPanel.Controls.Add(this._btnUncheckAll);
            this._topPanel.Controls.Add(this._btnCancel);
            this._topPanel.Controls.Add(this._progressBarFile);
            this._topPanel.Controls.Add(this._lblStatus);
            this._topPanel.Controls.Add(this._chkVerboseLogging);
            this._topPanel.Dock     = System.Windows.Forms.DockStyle.Top;
            this._topPanel.Location = new System.Drawing.Point(0, 0);
            this._topPanel.Name     = "_topPanel";
            this._topPanel.Padding  = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this._topPanel.Size     = new System.Drawing.Size(1495, 158);
            this._topPanel.TabIndex = 2;
            //
            // _lblHandBrakePath
            //
            this._lblHandBrakePath.Location  = new System.Drawing.Point(12, 9);
            this._lblHandBrakePath.Name      = "_lblHandBrakePath";
            this._lblHandBrakePath.Size      = new System.Drawing.Size(180, 20);
            this._lblHandBrakePath.TabIndex  = 0;
            this._lblHandBrakePath.Text      = "HandBrakeCLI.exe:";
            this._lblHandBrakePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _txtHandBrakePath
            //
            this._txtHandBrakePath.Anchor   = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._txtHandBrakePath.Location = new System.Drawing.Point(198, 6);
            this._txtHandBrakePath.Name     = "_txtHandBrakePath";
            this._txtHandBrakePath.Size     = new System.Drawing.Size(1015, 23);
            this._txtHandBrakePath.TabIndex = 1;
            //
            // _btnBrowseHandBrake
            //
            this._btnBrowseHandBrake.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnBrowseHandBrake.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseHandBrake.Location  = new System.Drawing.Point(1219, 4);
            this._btnBrowseHandBrake.Name      = "_btnBrowseHandBrake";
            this._btnBrowseHandBrake.Size      = new System.Drawing.Size(32, 23);
            this._btnBrowseHandBrake.TabIndex  = 2;
            this._btnBrowseHandBrake.Text      = "...";
            //
            // _btnTestHandBrake
            //
            this._btnTestHandBrake.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnTestHandBrake.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnTestHandBrake.Location  = new System.Drawing.Point(1258, 4);
            this._btnTestHandBrake.Name      = "_btnTestHandBrake";
            this._btnTestHandBrake.Size      = new System.Drawing.Size(60, 23);
            this._btnTestHandBrake.TabIndex  = 3;
            this._btnTestHandBrake.Text      = "Test";
            //
            // _lblHandBrakeStatus
            //
            this._lblHandBrakeStatus.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._lblHandBrakeStatus.Location  = new System.Drawing.Point(1325, 6);
            this._lblHandBrakeStatus.Name      = "_lblHandBrakeStatus";
            this._lblHandBrakeStatus.Size      = new System.Drawing.Size(158, 20);
            this._lblHandBrakeStatus.TabIndex  = 4;
            this._lblHandBrakeStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _lblFolder
            //
            this._lblFolder.Location  = new System.Drawing.Point(12, 42);
            this._lblFolder.Name      = "_lblFolder";
            this._lblFolder.Size      = new System.Drawing.Size(180, 20);
            this._lblFolder.TabIndex  = 5;
            this._lblFolder.Text      = "Carpeta de medios:";
            this._lblFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _txtFolder
            //
            this._txtFolder.Anchor   = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._txtFolder.Location = new System.Drawing.Point(198, 41);
            this._txtFolder.Name     = "_txtFolder";
            this._txtFolder.Size     = new System.Drawing.Size(1247, 23);
            this._txtFolder.TabIndex = 6;
            //
            // _btnBrowseFolder
            //
            this._btnBrowseFolder.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnBrowseFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseFolder.Location  = new System.Drawing.Point(1451, 40);
            this._btnBrowseFolder.Name      = "_btnBrowseFolder";
            this._btnBrowseFolder.Size      = new System.Drawing.Size(32, 24);
            this._btnBrowseFolder.TabIndex  = 7;
            this._btnBrowseFolder.Text      = "...";
            //
            // _lblThreshold
            //
            this._lblThreshold.Location  = new System.Drawing.Point(12, 69);
            this._lblThreshold.Name      = "_lblThreshold";
            this._lblThreshold.Size      = new System.Drawing.Size(130, 20);
            this._lblThreshold.TabIndex  = 8;
            this._lblThreshold.Text      = "Tamaño mínimo:";
            this._lblThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _txtThreshold
            //
            this._txtThreshold.Location = new System.Drawing.Point(147, 66);
            this._txtThreshold.Name     = "_txtThreshold";
            this._txtThreshold.Size     = new System.Drawing.Size(55, 23);
            this._txtThreshold.TabIndex = 9;
            this._txtThreshold.Text     = "4";
            //
            // _lblGB
            //
            this._lblGB.Location  = new System.Drawing.Point(207, 69);
            this._lblGB.Name      = "_lblGB";
            this._lblGB.Size      = new System.Drawing.Size(30, 20);
            this._lblGB.TabIndex  = 10;
            this._lblGB.Text      = "GB";
            this._lblGB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _btnScan
            //
            this._btnScan.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this._btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnScan.ForeColor = System.Drawing.Color.White;
            this._btnScan.Location  = new System.Drawing.Point(0, 95);
            this._btnScan.Name      = "_btnScan";
            this._btnScan.Size      = new System.Drawing.Size(130, 28);
            this._btnScan.TabIndex  = 11;
            this._btnScan.Text      = "Escanear";
            this._btnScan.UseVisualStyleBackColor = false;
            //
            // _btnCompressSelected
            //
            this._btnCompressSelected.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this._btnCompressSelected.Enabled   = false;
            this._btnCompressSelected.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCompressSelected.ForeColor = System.Drawing.Color.White;
            this._btnCompressSelected.Location  = new System.Drawing.Point(140, 95);
            this._btnCompressSelected.Name      = "_btnCompressSelected";
            this._btnCompressSelected.Size      = new System.Drawing.Size(155, 28);
            this._btnCompressSelected.TabIndex  = 12;
            this._btnCompressSelected.Text      = "Comprimir Sel.";
            this._btnCompressSelected.UseVisualStyleBackColor = false;
            //
            // _btnCheckRecommended
            //
            this._btnCheckRecommended.Enabled   = false;
            this._btnCheckRecommended.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCheckRecommended.Location  = new System.Drawing.Point(305, 95);
            this._btnCheckRecommended.Name      = "_btnCheckRecommended";
            this._btnCheckRecommended.Size      = new System.Drawing.Size(175, 28);
            this._btnCheckRecommended.TabIndex  = 13;
            this._btnCheckRecommended.Text      = "Marcar Recomendados";
            //
            // _btnUncheckAll
            //
            this._btnUncheckAll.Enabled   = false;
            this._btnUncheckAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnUncheckAll.Location  = new System.Drawing.Point(490, 95);
            this._btnUncheckAll.Name      = "_btnUncheckAll";
            this._btnUncheckAll.Size      = new System.Drawing.Size(120, 28);
            this._btnUncheckAll.TabIndex  = 14;
            this._btnUncheckAll.Text      = "Desmarcar Todo";
            //
            // _btnCancel
            //
            this._btnCancel.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this._btnCancel.Enabled   = false;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.ForeColor = System.Drawing.Color.White;
            this._btnCancel.Location  = new System.Drawing.Point(620, 95);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(100, 28);
            this._btnCancel.TabIndex  = 15;
            this._btnCancel.Text      = "Cancelar";
            this._btnCancel.UseVisualStyleBackColor = false;
            //
            // _progressBarFile
            //
            this._progressBarFile.Anchor   = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBarFile.Location = new System.Drawing.Point(730, 96);
            this._progressBarFile.Name     = "_progressBarFile";
            this._progressBarFile.Size     = new System.Drawing.Size(753, 27);
            this._progressBarFile.Style    = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBarFile.TabIndex = 16;
            //
            // _lblStatus
            //
            this._lblStatus.Anchor    = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._lblStatus.ForeColor = System.Drawing.Color.Gray;
            this._lblStatus.Location  = new System.Drawing.Point(12, 131);
            this._lblStatus.Name      = "_lblStatus";
            this._lblStatus.Size      = new System.Drawing.Size(1314, 18);
            this._lblStatus.TabIndex  = 17;
            //
            // _chkVerboseLogging
            //
            this._chkVerboseLogging.Anchor   = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._chkVerboseLogging.AutoSize = true;
            this._chkVerboseLogging.Location = new System.Drawing.Point(1332, 131);
            this._chkVerboseLogging.Name     = "_chkVerboseLogging";
            this._chkVerboseLogging.Size     = new System.Drawing.Size(151, 19);
            this._chkVerboseLogging.TabIndex = 18;
            this._chkVerboseLogging.Text     = "Logging Mode: Verbose";
            this._chkVerboseLogging.UseVisualStyleBackColor = true;
            //
            // _splitContainer
            //
            this._splitContainer.Dock     = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 159);
            this._splitContainer.Name     = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this._splitContainer.Panel1.Controls.Add(this._grid);
            this._splitContainer.Panel1MinSize = 100;
            this._splitContainer.Panel2.Controls.Add(this._txtLog);
            this._splitContainer.Panel2MinSize = 80;
            this._splitContainer.Size            = new System.Drawing.Size(1495, 500);
            this._splitContainer.SplitterDistance = 300;
            this._splitContainer.TabIndex        = 0;
            //
            // _grid
            //
            this._grid.AllowUserToAddRows    = false;
            this._grid.AllowUserToDeleteRows = false;
            this._grid.BorderStyle           = System.Windows.Forms.BorderStyle.None;
            this._grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this._grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this._gridColSelected,
                this._gridColFileName,
                this._gridColFullPath,
                this._gridColSize,
                this._gridColRecommended,
                this._gridColStatus });
            this._grid.Dock              = System.Windows.Forms.DockStyle.Fill;
            this._grid.Font              = new System.Drawing.Font("Segoe UI", 8.5F);
            this._grid.GridColor         = System.Drawing.Color.FromArgb(220, 220, 225);
            this._grid.Location          = new System.Drawing.Point(0, 0);
            this._grid.MultiSelect       = false;
            this._grid.Name              = "_grid";
            this._grid.ReadOnly          = false;
            this._grid.RowHeadersVisible = false;
            this._grid.SelectionMode     = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._grid.Size              = new System.Drawing.Size(1495, 300);
            this._grid.TabIndex          = 0;
            //
            // _gridColSelected
            //
            this._gridColSelected.HeaderText  = "V";
            this._gridColSelected.Name        = "_gridColSelected";
            this._gridColSelected.ReadOnly     = false;
            this._gridColSelected.Width        = 30;
            //
            // _gridColFileName
            //
            this._gridColFileName.HeaderText  = "Archivo";
            this._gridColFileName.Name        = "_gridColFileName";
            this._gridColFileName.ReadOnly     = true;
            this._gridColFileName.Width        = 380;
            //
            // _gridColFullPath
            //
            this._gridColFullPath.HeaderText  = "Ruta";
            this._gridColFullPath.Name        = "_gridColFullPath";
            this._gridColFullPath.ReadOnly     = true;
            this._gridColFullPath.Width        = 430;
            //
            // _gridColSize
            //
            this._gridColSize.HeaderText  = "Tamano";
            this._gridColSize.Name        = "_gridColSize";
            this._gridColSize.ReadOnly     = true;
            this._gridColSize.Width        = 90;
            //
            // _gridColRecommended
            //
            this._gridColRecommended.HeaderText  = "Recomendado";
            this._gridColRecommended.Name        = "_gridColRecommended";
            this._gridColRecommended.ReadOnly     = true;
            this._gridColRecommended.Width        = 110;
            //
            // _gridColStatus
            //
            this._gridColStatus.HeaderText  = "Estado";
            this._gridColStatus.Name        = "_gridColStatus";
            this._gridColStatus.ReadOnly     = true;
            this._gridColStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            //
            // _txtLog
            //
            this._txtLog.BackColor   = System.Drawing.Color.FromArgb(30, 30, 30);
            this._txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtLog.Dock        = System.Windows.Forms.DockStyle.Fill;
            this._txtLog.Font        = new System.Drawing.Font("Consolas", 8F);
            this._txtLog.ForeColor   = System.Drawing.Color.FromArgb(200, 200, 200);
            this._txtLog.Location    = new System.Drawing.Point(0, 0);
            this._txtLog.Multiline   = true;
            this._txtLog.Name        = "_txtLog";
            this._txtLog.ReadOnly    = true;
            this._txtLog.ScrollBars  = System.Windows.Forms.ScrollBars.Vertical;
            this._txtLog.Size        = new System.Drawing.Size(1495, 196);
            this._txtLog.TabIndex    = 0;
            //
            // _statusStrip
            //
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this._stripLabel });
            this._statusStrip.Location   = new System.Drawing.Point(0, 659);
            this._statusStrip.Name       = "_statusStrip";
            this._statusStrip.Size       = new System.Drawing.Size(1495, 22);
            this._statusStrip.SizingGrip = false;
            this._statusStrip.TabIndex   = 3;
            //
            // _stripLabel
            //
            this._stripLabel.Name      = "_stripLabel";
            this._stripLabel.Size      = new System.Drawing.Size(1480, 17);
            this._stripLabel.Spring    = true;
            this._stripLabel.Text      = "Listo.";
            this._stripLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _sep
            //
            this._sep.BackColor = System.Drawing.Color.FromArgb(220, 220, 225);
            this._sep.Dock      = System.Windows.Forms.DockStyle.Top;
            this._sep.Location  = new System.Drawing.Point(0, 158);
            this._sep.Name      = "_sep";
            this._sep.Size      = new System.Drawing.Size(1495, 1);
            this._sep.TabIndex  = 1;
            //
            // CompressForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(245, 245, 248);
            this.ClientSize          = new System.Drawing.Size(1495, 681);
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._sep);
            this.Controls.Add(this._topPanel);
            this.Controls.Add(this._statusStrip);
            this.Font          = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize   = new System.Drawing.Size(800, 520);
            this.Name          = "CompressForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text          = "Comprimir Archivos Grandes — HandBrakeCLI";
            this.WindowState   = System.Windows.Forms.FormWindowState.Maximized;
            this._topPanel.ResumeLayout(false);
            this._topPanel.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel                       _topPanel;
        private System.Windows.Forms.Label                       _lblHandBrakePath;
        private System.Windows.Forms.TextBox                     _txtHandBrakePath;
        private System.Windows.Forms.Button                      _btnBrowseHandBrake;
        private System.Windows.Forms.Button                      _btnTestHandBrake;
        private System.Windows.Forms.Label                       _lblHandBrakeStatus;
        private System.Windows.Forms.Label                       _lblFolder;
        private System.Windows.Forms.TextBox                     _txtFolder;
        private System.Windows.Forms.Button                      _btnBrowseFolder;
        private System.Windows.Forms.Label                       _lblThreshold;
        private System.Windows.Forms.TextBox                     _txtThreshold;
        private System.Windows.Forms.Label                       _lblGB;
        private System.Windows.Forms.Button                      _btnScan;
        private System.Windows.Forms.Button                      _btnCompressSelected;
        private System.Windows.Forms.Button                      _btnCheckRecommended;
        private System.Windows.Forms.Button                      _btnUncheckAll;
        private System.Windows.Forms.Button                      _btnCancel;
        private System.Windows.Forms.ProgressBar                 _progressBarFile;
        private System.Windows.Forms.Label                       _lblStatus;
        private System.Windows.Forms.CheckBox                    _chkVerboseLogging;
        private System.Windows.Forms.SplitContainer              _splitContainer;
        private System.Windows.Forms.DataGridView                _grid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn  _gridColSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn   _gridColFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn   _gridColFullPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn   _gridColSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn   _gridColRecommended;
        private System.Windows.Forms.DataGridViewTextBoxColumn   _gridColStatus;
        private System.Windows.Forms.TextBox                     _txtLog;
        private System.Windows.Forms.StatusStrip                 _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel        _stripLabel;
        private System.Windows.Forms.Panel                       _sep;
    }
}
```

- [ ] **Step 2: Create CompressForm.resx**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
</root>
```

---

### Task 6: CompressForm logic

**Files:**
- Create: `AnimeFixer/AnimeFixer/CompressForm.cs`

The form logic handles: settings load/save, HandBrakeCLI path browsing + test, folder browsing, scan, check/uncheck helpers, and the async compress loop.

- [ ] **Step 1: Create CompressForm.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnimeFixer.Models;
using AnimeFixer.Services;

namespace AnimeFixer
{
    public partial class CompressForm : Form
    {
        private readonly SettingsService      _settings = new SettingsService();
        private readonly CompressScannerService _scanner = new CompressScannerService();
        private readonly HandBrakeService     _handBrake = new HandBrakeService();
        private          List<CompressFile>   _files     = new List<CompressFile>();
        private          CancellationTokenSource _cts;

        public CompressForm()
        {
            InitializeComponent();
            LoadSettings();
            WireEvents();
        }

        // ── Settings ─────────────────────────────────────────────────────────

        private void LoadSettings()
        {
            _txtHandBrakePath.Text = _settings.HandBrakeCLIPath;
            _txtFolder.Text        = _settings.LastFolder;
        }

        private void SaveSettings()
        {
            _settings.HandBrakeCLIPath = _txtHandBrakePath.Text.Trim();
            _settings.LastFolder       = _txtFolder.Text.Trim();
            _settings.Save();
        }

        // ── Event wiring ─────────────────────────────────────────────────────

        private void WireEvents()
        {
            _btnBrowseHandBrake.Click   += BtnBrowseHandBrake_Click;
            _btnTestHandBrake.Click     += BtnTestHandBrake_Click;
            _txtHandBrakePath.Leave     += (s, e) => SaveSettings();
            _btnBrowseFolder.Click      += BtnBrowseFolder_Click;
            _txtFolder.Leave            += (s, e) => SaveSettings();
            _btnScan.Click              += BtnScan_Click;
            _btnCompressSelected.Click  += BtnCompressSelected_Click;
            _btnCheckRecommended.Click  += BtnCheckRecommended_Click;
            _btnUncheckAll.Click        += BtnUncheckAll_Click;
            _btnCancel.Click            += BtnCancel_Click;
        }

        // ── Browse / Test HandBrakeCLI ────────────────────────────────────

        private void BtnBrowseHandBrake_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Title  = "Seleccionar HandBrakeCLI.exe",
                Filter = "HandBrakeCLI.exe|HandBrakeCLI.exe|Todos los ejecutables|*.exe"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _txtHandBrakePath.Text = dlg.FileName;
                    SaveSettings();
                }
            }
        }

        private async void BtnTestHandBrake_Click(object sender, EventArgs e)
        {
            _btnTestHandBrake.Enabled    = false;
            _lblHandBrakeStatus.Text     = "Verificando...";
            _lblHandBrakeStatus.ForeColor = Color.Gray;

            string path = _txtHandBrakePath.Text.Trim();
            string version = null, error = null;
            bool   ok      = false;

            await Task.Run(() =>
            {
                _handBrake.Resolve(path);
                ok = _handBrake.TestVersion(out version, out error);
            });

            if (ok)
            {
                _lblHandBrakeStatus.Text      = "OK: " + version;
                _lblHandBrakeStatus.ForeColor = Color.FromArgb(30, 130, 60);
            }
            else
            {
                _lblHandBrakeStatus.Text      = error;
                _lblHandBrakeStatus.ForeColor = Color.FromArgb(180, 0, 0);
            }

            _btnTestHandBrake.Enabled = true;
        }

        // ── Browse folder ────────────────────────────────────────────────────

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog
            {
                Description  = "Seleccionar carpeta de medios",
                SelectedPath = _txtFolder.Text.Trim()
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _txtFolder.Text = dlg.SelectedPath;
                    SaveSettings();
                }
            }
        }

        // ── Scan ─────────────────────────────────────────────────────────────

        private void BtnScan_Click(object sender, EventArgs e)
        {
            string folder = _txtFolder.Text.Trim();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show("Selecciona una carpeta válida.", "AnimeFixer",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateHandBrake()) return;

            long thresholdBytes = ParseThresholdBytes();

            _grid.Rows.Clear();
            _files.Clear();
            _btnScan.Enabled              = false;
            _btnCompressSelected.Enabled  = false;
            _btnCheckRecommended.Enabled  = false;
            _btnUncheckAll.Enabled        = false;
            SetStatus("Escaneando...");

            Task.Run(() =>
            {
                var found = _scanner.Scan(folder, thresholdBytes, path =>
                    SetStatus("Encontrado: " + Path.GetFileName(path)));

                this.Invoke(new Action(() =>
                {
                    _files = found;
                    PopulateGrid();
                    _btnScan.Enabled             = true;
                    _btnCompressSelected.Enabled = _files.Count > 0;
                    _btnCheckRecommended.Enabled = _files.Count > 0;
                    _btnUncheckAll.Enabled       = _files.Count > 0;
                    SetStatus(string.Format(
                        "Encontrados {0} archivos ({1} recomendados para comprimir).",
                        _files.Count,
                        CountRecommended()));
                    _stripLabel.Text = _files.Count + " archivos encontrados.";
                }));
            });
        }

        private void PopulateGrid()
        {
            _grid.Rows.Clear();
            foreach (var cf in _files)
            {
                var idx = _grid.Rows.Add(
                    cf.IsSelected,
                    cf.FileName,
                    cf.FilePath,
                    ScannerService.FormatSize(cf.FileSizeBytes),
                    cf.IsRecommended ? "Comprimir" : "",
                    "Pendiente");
                _grid.Rows[idx].Tag = cf;
            }
        }

        // ── Check / Uncheck helpers ──────────────────────────────────────────

        private void BtnCheckRecommended_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                var cf = row.Tag as CompressFile;
                if (cf != null && cf.IsRecommended)
                {
                    row.Cells[0].Value = true;
                    cf.IsSelected      = true;
                }
            }
        }

        private void BtnUncheckAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                row.Cells[0].Value = false;
                var cf = row.Tag as CompressFile;
                if (cf != null) cf.IsSelected = false;
            }
        }

        // ── Compress ─────────────────────────────────────────────────────────

        private async void BtnCompressSelected_Click(object sender, EventArgs e)
        {
            if (!ValidateHandBrake()) return;

            // Sync checkbox state back from grid before collecting
            SyncCheckboxesFromGrid();

            var toProcess = new List<CompressFile>();
            foreach (var cf in _files)
            {
                if (cf.IsSelected && cf.Status != CompressStatus.Done)
                    toProcess.Add(cf);
            }

            if (toProcess.Count == 0)
            {
                MessageBox.Show("No hay archivos seleccionados.", "AnimeFixer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Check for pre-existing output files
            var existing = new List<CompressFile>();
            foreach (var cf in toProcess)
            {
                if (File.Exists(cf.OutputPath))
                    existing.Add(cf);
            }

            if (existing.Count > 0)
            {
                var answer = MessageBox.Show(
                    string.Format("{0} archivo(s) ya tienen versión comprimida. ¿Sobreescribir?",
                        existing.Count),
                    "Archivo existente",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer == DialogResult.Cancel) return;

                if (answer == DialogResult.No)
                {
                    foreach (var cf in existing)
                        toProcess.Remove(cf);
                }
                // DialogResult.Yes → let HandBrake overwrite (it will overwrite automatically)
            }

            if (toProcess.Count == 0) return;

            SetBusy(true);
            _cts = new CancellationTokenSource();

            _handBrake.Resolve(_txtHandBrakePath.Text.Trim());

            await Task.Run(() =>
            {
                for (int i = 0; i < toProcess.Count; i++)
                {
                    if (_cts.Token.IsCancellationRequested) break;

                    var cf = toProcess[i];

                    int fileNum   = i + 1;
                    int fileTotal = toProcess.Count;

                    SetFileStatus(cf, CompressStatus.Processing, "Procesando...", Color.Blue);
                    SetStatus(string.Format("Procesando {0} de {1}: {2}",
                        fileNum, fileTotal, cf.FileName));
                    SetProgress(0);

                    try
                    {
                        bool ok = _handBrake.Compress(
                            cf,
                            line => AppendLog(line),
                            pct  => SetProgress((int)pct),
                            _cts.Token,
                            out string errMsg);

                        if (ok)
                        {
                            long saved   = cf.FileSizeBytes - cf.OutputSizeBytes;
                            double savePct = cf.FileSizeBytes > 0
                                ? saved * 100.0 / cf.FileSizeBytes : 0;

                            string doneText = string.Format(
                                "OK: {0} -> {1} (-{2:F0}%)",
                                ScannerService.FormatSize(cf.FileSizeBytes),
                                ScannerService.FormatSize(cf.OutputSizeBytes),
                                savePct);

                            SetFileStatus(cf, CompressStatus.Done, doneText,
                                Color.FromArgb(30, 130, 60));
                            AppendLog(string.Format("[OK] {0}: {1}", cf.FileName, doneText));
                        }
                        else
                        {
                            cf.ErrorMessage = errMsg;
                            SetFileStatus(cf, CompressStatus.Failed,
                                "Error: " + errMsg, Color.FromArgb(180, 0, 0));
                            AppendLog(string.Format("[FAIL] {0}: {1}", cf.FileName, errMsg));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        SetFileStatus(cf, CompressStatus.Skipped,
                            "Cancelado", Color.Gray);
                        break;
                    }
                }
            });

            SetBusy(false);
            SetProgress(0);
            SetStatus("Completado.");
            _stripLabel.Text = "Compresión finalizada.";

            if (_cts != null) { _cts.Dispose(); _cts = null; }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool ValidateHandBrake()
        {
            string path = _txtHandBrakePath.Text.Trim();
            if (!string.IsNullOrWhiteSpace(path) && !File.Exists(path))
            {
                _lblHandBrakeStatus.Text      =
                    "HandBrakeCLI.exe no encontrado. Configure la ruta arriba.";
                _lblHandBrakeStatus.ForeColor = Color.FromArgb(180, 0, 0);
                _btnScan.Enabled              = false;
                _btnCompressSelected.Enabled  = false;
                return false;
            }
            return true;
        }

        private long ParseThresholdBytes()
        {
            const long GB = 1024L * 1024L * 1024L;
            if (long.TryParse(_txtThreshold.Text.Trim(), out long gb) && gb > 0)
                return gb * GB;
            return 4L * GB;
        }

        private int CountRecommended()
        {
            int n = 0;
            foreach (var cf in _files)
                if (cf.IsRecommended) n++;
            return n;
        }

        private void SyncCheckboxesFromGrid()
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                var cf = row.Tag as CompressFile;
                if (cf == null) continue;
                var cell = row.Cells[0] as DataGridViewCheckBoxCell;
                if (cell != null)
                    cf.IsSelected = cell.Value is bool b && b;
            }
        }

        // ── Thread-safe UI updaters ──────────────────────────────────────────

        private void SetBusy(bool busy)
        {
            if (InvokeRequired) { Invoke(new Action<bool>(SetBusy), busy); return; }

            _btnScan.Enabled             = !busy;
            _btnCompressSelected.Enabled = !busy;
            _btnCheckRecommended.Enabled = !busy;
            _btnUncheckAll.Enabled       = !busy;
            _btnCancel.Enabled           = busy;
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired) { Invoke(new Action<string>(SetStatus), text); return; }
            _lblStatus.Text = text;
        }

        private void SetProgress(int value)
        {
            if (InvokeRequired) { Invoke(new Action<int>(SetProgress), value); return; }
            _progressBarFile.Value = Math.Max(0, Math.Min(100, value));
        }

        private void SetFileStatus(
            CompressFile cf, CompressStatus status, string displayText, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<CompressFile, CompressStatus, string, Color>(SetFileStatus),
                    cf, status, displayText, color);
                return;
            }

            cf.Status = status;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag != cf) continue;
                row.Cells[5].Value             = displayText;
                row.DefaultCellStyle.ForeColor = color;
                break;
            }
        }

        private void AppendLog(string line)
        {
            if (InvokeRequired) { Invoke(new Action<string>(AppendLog), line); return; }
            if (!_chkVerboseLogging.Checked &&
                line.StartsWith("Encoding:")) return;

            _txtLog.AppendText(line + Environment.NewLine);
            _txtLog.ScrollToCaret();
        }
    }
}
```

---

### Task 7: Update Form1 launcher — add third panel

**Files:**
- Modify: `AnimeFixer/AnimeFixer/Form1.cs`
- Modify: `AnimeFixer/AnimeFixer/Form1.Designer.cs`

Form1 needs a third panel for the Compress tool. Widen the form from 700px to 1030px. Keep existing two panels, shift them slightly, add the third at x=690.

- [ ] **Step 1: Add handler to Form1.cs**

In `Form1.cs`, after `BtnMkvFixer_Click`, add:

```csharp
private void BtnCompressFixer_Click(object sender, EventArgs e)
{
    new CompressForm().Show();
}
```

- [ ] **Step 2: Replace Form1.Designer.cs completely**

The new designer adds `_pnlCompress` and its three child controls, changes the form `ClientSize` to `(1030, 310)`, and widens `_lblTitle`.

```csharp
namespace AnimeFixer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblTitle          = new System.Windows.Forms.Label();
            this._pnlMp4            = new System.Windows.Forms.Panel();
            this._lblMp4Title       = new System.Windows.Forms.Label();
            this._lblMp4Desc        = new System.Windows.Forms.Label();
            this._btnMp4Fixer       = new System.Windows.Forms.Button();
            this._pnlMkv            = new System.Windows.Forms.Panel();
            this._lblMkvTitle       = new System.Windows.Forms.Label();
            this._lblMkvDesc        = new System.Windows.Forms.Label();
            this._btnMkvFixer       = new System.Windows.Forms.Button();
            this._pnlCompress       = new System.Windows.Forms.Panel();
            this._lblCompressTitle  = new System.Windows.Forms.Label();
            this._lblCompressDesc   = new System.Windows.Forms.Label();
            this._btnCompressFixer  = new System.Windows.Forms.Button();
            this._pnlMp4.SuspendLayout();
            this._pnlMkv.SuspendLayout();
            this._pnlCompress.SuspendLayout();
            this.SuspendLayout();
            //
            // _lblTitle
            //
            this._lblTitle.Dock      = System.Windows.Forms.DockStyle.Top;
            this._lblTitle.Font      = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this._lblTitle.ForeColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this._lblTitle.Location  = new System.Drawing.Point(0, 0);
            this._lblTitle.Name      = "_lblTitle";
            this._lblTitle.Padding   = new System.Windows.Forms.Padding(20, 20, 20, 10);
            this._lblTitle.Size      = new System.Drawing.Size(1030, 70);
            this._lblTitle.TabIndex  = 0;
            this._lblTitle.Text      = "AnimeFixer — Seleccionar modo";
            this._lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _pnlMp4
            //
            this._pnlMp4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pnlMp4.Controls.Add(this._lblMp4Title);
            this._pnlMp4.Controls.Add(this._lblMp4Desc);
            this._pnlMp4.Controls.Add(this._btnMp4Fixer);
            this._pnlMp4.Location    = new System.Drawing.Point(20, 90);
            this._pnlMp4.Name        = "_pnlMp4";
            this._pnlMp4.Size        = new System.Drawing.Size(300, 185);
            this._pnlMp4.TabIndex    = 1;
            //
            // _lblMp4Title
            //
            this._lblMp4Title.Font     = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._lblMp4Title.Location = new System.Drawing.Point(15, 15);
            this._lblMp4Title.Name     = "_lblMp4Title";
            this._lblMp4Title.Size     = new System.Drawing.Size(270, 25);
            this._lblMp4Title.TabIndex = 0;
            this._lblMp4Title.Text     = "MP4 / GPAC Fixer";
            //
            // _lblMp4Desc
            //
            this._lblMp4Desc.Location = new System.Drawing.Point(15, 45);
            this._lblMp4Desc.Name     = "_lblMp4Desc";
            this._lblMp4Desc.Size     = new System.Drawing.Size(270, 90);
            this._lblMp4Desc.TabIndex = 1;
            this._lblMp4Desc.Text     = "Detecta y corrige archivos MP4 con streams GPAC inválidos que causan crashes en Roku TV.";
            //
            // _btnMp4Fixer
            //
            this._btnMp4Fixer.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this._btnMp4Fixer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnMp4Fixer.ForeColor = System.Drawing.Color.White;
            this._btnMp4Fixer.Location  = new System.Drawing.Point(15, 148);
            this._btnMp4Fixer.Name      = "_btnMp4Fixer";
            this._btnMp4Fixer.Size      = new System.Drawing.Size(270, 30);
            this._btnMp4Fixer.TabIndex  = 2;
            this._btnMp4Fixer.Text      = "Abrir MP4 Fixer ->>";
            this._btnMp4Fixer.UseVisualStyleBackColor = false;
            this._btnMp4Fixer.Click    += new System.EventHandler(this.BtnMp4Fixer_Click);
            //
            // _pnlMkv
            //
            this._pnlMkv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pnlMkv.Controls.Add(this._lblMkvTitle);
            this._pnlMkv.Controls.Add(this._lblMkvDesc);
            this._pnlMkv.Controls.Add(this._btnMkvFixer);
            this._pnlMkv.Location    = new System.Drawing.Point(355, 90);
            this._pnlMkv.Name        = "_pnlMkv";
            this._pnlMkv.Size        = new System.Drawing.Size(300, 185);
            this._pnlMkv.TabIndex    = 2;
            //
            // _lblMkvTitle
            //
            this._lblMkvTitle.Font     = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._lblMkvTitle.Location = new System.Drawing.Point(15, 15);
            this._lblMkvTitle.Name     = "_lblMkvTitle";
            this._lblMkvTitle.Size     = new System.Drawing.Size(270, 25);
            this._lblMkvTitle.TabIndex = 0;
            this._lblMkvTitle.Text     = "MKV / Roku Fixer";
            //
            // _lblMkvDesc
            //
            this._lblMkvDesc.Location = new System.Drawing.Point(15, 45);
            this._lblMkvDesc.Name     = "_lblMkvDesc";
            this._lblMkvDesc.Size     = new System.Drawing.Size(270, 90);
            this._lblMkvDesc.TabIndex = 1;
            this._lblMkvDesc.Text     = "Detecta HEVC 10-bit en MKV y re-encodea a H.264. Extrae subtítulos ASS embebidos a archivos SRT para Roku.";
            //
            // _btnMkvFixer
            //
            this._btnMkvFixer.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this._btnMkvFixer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnMkvFixer.ForeColor = System.Drawing.Color.White;
            this._btnMkvFixer.Location  = new System.Drawing.Point(15, 148);
            this._btnMkvFixer.Name      = "_btnMkvFixer";
            this._btnMkvFixer.Size      = new System.Drawing.Size(270, 30);
            this._btnMkvFixer.TabIndex  = 2;
            this._btnMkvFixer.Text      = "Abrir MKV / Roku Fixer ->>";
            this._btnMkvFixer.UseVisualStyleBackColor = false;
            this._btnMkvFixer.Click    += new System.EventHandler(this.BtnMkvFixer_Click);
            //
            // _pnlCompress
            //
            this._pnlCompress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pnlCompress.Controls.Add(this._lblCompressTitle);
            this._pnlCompress.Controls.Add(this._lblCompressDesc);
            this._pnlCompress.Controls.Add(this._btnCompressFixer);
            this._pnlCompress.Location    = new System.Drawing.Point(690, 90);
            this._pnlCompress.Name        = "_pnlCompress";
            this._pnlCompress.Size        = new System.Drawing.Size(300, 185);
            this._pnlCompress.TabIndex    = 3;
            //
            // _lblCompressTitle
            //
            this._lblCompressTitle.Font     = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._lblCompressTitle.Location = new System.Drawing.Point(15, 15);
            this._lblCompressTitle.Name     = "_lblCompressTitle";
            this._lblCompressTitle.Size     = new System.Drawing.Size(270, 25);
            this._lblCompressTitle.TabIndex = 0;
            this._lblCompressTitle.Text     = "Comprimir Archivos";
            //
            // _lblCompressDesc
            //
            this._lblCompressDesc.Location = new System.Drawing.Point(15, 45);
            this._lblCompressDesc.Name     = "_lblCompressDesc";
            this._lblCompressDesc.Size     = new System.Drawing.Size(270, 90);
            this._lblCompressDesc.TabIndex = 1;
            this._lblCompressDesc.Text     = "Re-encodea archivos grandes (.mkv/.mp4) a H.264 via HandBrakeCLI para reducir tamaño preservando audio y subtítulos.";
            //
            // _btnCompressFixer
            //
            this._btnCompressFixer.BackColor = System.Drawing.Color.FromArgb(142, 68, 173);
            this._btnCompressFixer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCompressFixer.ForeColor = System.Drawing.Color.White;
            this._btnCompressFixer.Location  = new System.Drawing.Point(15, 148);
            this._btnCompressFixer.Name      = "_btnCompressFixer";
            this._btnCompressFixer.Size      = new System.Drawing.Size(270, 30);
            this._btnCompressFixer.TabIndex  = 2;
            this._btnCompressFixer.Text      = "Abrir Compress Fixer ->>";
            this._btnCompressFixer.UseVisualStyleBackColor = false;
            this._btnCompressFixer.Click    += new System.EventHandler(this.BtnCompressFixer_Click);
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(245, 245, 248);
            this.ClientSize          = new System.Drawing.Size(1030, 310);
            this.Controls.Add(this._lblTitle);
            this.Controls.Add(this._pnlMp4);
            this.Controls.Add(this._pnlMkv);
            this.Controls.Add(this._pnlCompress);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "Form1";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "AnimeFixer";
            this._pnlMp4.ResumeLayout(false);
            this._pnlMkv.ResumeLayout(false);
            this._pnlCompress.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label  _lblTitle;
        private System.Windows.Forms.Panel  _pnlMp4;
        private System.Windows.Forms.Label  _lblMp4Title;
        private System.Windows.Forms.Label  _lblMp4Desc;
        private System.Windows.Forms.Button _btnMp4Fixer;
        private System.Windows.Forms.Panel  _pnlMkv;
        private System.Windows.Forms.Label  _lblMkvTitle;
        private System.Windows.Forms.Label  _lblMkvDesc;
        private System.Windows.Forms.Button _btnMkvFixer;
        private System.Windows.Forms.Panel  _pnlCompress;
        private System.Windows.Forms.Label  _lblCompressTitle;
        private System.Windows.Forms.Label  _lblCompressDesc;
        private System.Windows.Forms.Button _btnCompressFixer;
    }
}
```

---

### Task 8: Register new files in AnimeFixer.csproj

**Files:**
- Modify: `AnimeFixer/AnimeFixer/AnimeFixer.csproj`

- [ ] **Step 1: Add the new Compile entries**

Inside the `<ItemGroup>` that contains the `<Compile>` entries, add after `<Compile Include="Models\SubtitleTrack.cs" />`:

```xml
<Compile Include="Models\CompressFile.cs" />
```

After `<Compile Include="Services\MkvScannerService.cs" />`:

```xml
<Compile Include="Services\HandBrakeService.cs" />
<Compile Include="Services\CompressScannerService.cs" />
```

After `<Compile Include="SubtitleExtractionDialog.Designer.cs">...</Compile>`:

```xml
<Compile Include="CompressForm.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="CompressForm.Designer.cs">
  <DependentUpon>CompressForm.cs</DependentUpon>
</Compile>
```

After `<EmbeddedResource Include="SubtitleExtractionDialog.resx">...</EmbeddedResource>`:

```xml
<EmbeddedResource Include="CompressForm.resx">
  <DependentUpon>CompressForm.cs</DependentUpon>
</EmbeddedResource>
```

---

### Task 9: Build and verify

**Files:** none (verification only)

- [ ] **Step 1: Run MSBuild**

```
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "D:\Just For Fun\CidAnimeFixer\AnimeFixer\AnimeFixer\AnimeFixer.csproj" /p:Configuration=Debug /t:Build /v:minimal
```

Expected output ends with:
```
Build succeeded.
    0 Error(s)
```

- [ ] **Step 2: Verify the output binary exists**

```
ls "D:\Just For Fun\CidAnimeFixer\AnimeFixer\AnimeFixer\bin\Debug\AnimeFixer.exe"
```

Expected: the file exists and its timestamp is the current time.

- [ ] **Step 3: Manual smoke test checklist**

Launch `AnimeFixer.exe` and verify:
1. Form1 now shows **three** panels side by side
2. "Abrir Compress Fixer" button opens `CompressForm`
3. HandBrakeCLI path row has textbox + `...` + Test button + status label
4. Test button with no path shows `HandBrakeCLI.exe not found` (unless HandBrakeCLI is in PATH)
5. Test button with a valid HandBrakeCLI.exe path shows the version string in green
6. Folder browse works
7. Threshold field defaults to `4`
8. Scan without a valid folder shows a warning dialog
9. Scan with a folder containing video files populates the DataGridView sorted by size descending
10. Files ≥ threshold are pre-checked; smaller files are unchecked
11. "Marcar Recomendados" checks all recommended rows; "Desmarcar Todo" clears all
12. "Comprimir Sel." with nothing selected shows "No hay archivos seleccionados"
13. With HandBrakeCLI available: "Comprimir Sel." starts processing, progress bar fills, log shows live stdout
14. Cancel button aborts mid-file, partial output file is cleaned up

---

## Self-Review

### Spec coverage

| Spec requirement | Covered by |
|---|---|
| Scan .mkv/.mp4/.m4v/.mov | Task 4 `CompressScannerService` |
| Size threshold (default 4 GB) | Task 4 + Task 5 `_txtThreshold` default "4" |
| Pre-check recommended files | Task 4 `IsSelected = recommended`, Task 5 `_gridColSelected` |
| Sort by size descending | Task 4 `files.Sort()` |
| DataGridView with checkbox, filename, path, size, recommended, status | Task 5 designer, 6 columns |
| HandBrakeCLI path config (textbox + browse + test + inline status) | Task 5 + Task 6 |
| Save path on focus leave or browse select | Task 6 `_txtHandBrakePath.Leave` + `BtnBrowseHandBrake_Click` |
| Test button → --version → async → inline result | Task 6 `BtnTestHandBrake_Click` async |
| Path resolution: configured → PATH fallback | Task 3 `HandBrakeService.Resolve()` |
| HB command matches spec exactly | Task 3 `HandBrakeService.Compress()` args string |
| Output naming `_compressed` + original extension | Task 1 `CompressFile.OutputPath` |
| Original never modified or deleted | Task 3 only creates output file; never touches input |
| Real-time progress from stdout `N.NN %` regex | Task 3 `_progressRx.Match` in `OutputDataReceived` |
| Per-file progress bar | Task 5 `_progressBarFile` + Task 6 `SetProgress()` |
| Overall status label `Processing N of M` | Task 6 `SetStatus()` inside loop |
| Log panel with live stdout | Task 5 `_txtLog` + Task 6 `AppendLog()` |
| Size reduction on completion | Task 6 Done branch computes `saved` / `savePct` |
| Failed row marked red, continue to next | Task 6 `SetFileStatus(...Failed...)` + loop continues |
| Output file already exists → prompt | Task 6 pre-flight check before `Task.Run` |
| HandBrakeCLI not found → disable buttons + inline error | Task 6 `ValidateHandBrake()` |
| Cancel button kills process + deletes partial output | Task 3 `p.Kill()` + `File.Delete(outputPath)` |
| Form1 third panel | Task 7 |
| SettingsService HandBrakeCLIPath key | Task 2 |
| .csproj registration | Task 8 |

### Type consistency check

- `CompressFile.OutputPath` used in Task 3 (`file.OutputPath`) — defined in Task 1 ✓
- `CompressFile.FileSizeBytes` / `OutputSizeBytes` used in Task 6 — defined in Task 1 ✓
- `CompressStatus.Done/.Failed/.Skipped/.Processing` used in Task 6 — defined in Task 1 ✓
- `HandBrakeService.Resolve(string)` called in Task 6 — defined in Task 3 ✓
- `HandBrakeService.TestVersion(out string, out string)` called in Task 6 — defined in Task 3 ✓
- `HandBrakeService.Compress(CompressFile, Action<string>, Action<float>, CancellationToken, out string)` called in Task 6 — defined in Task 3 ✓
- `CompressScannerService.Scan(string, long, Action<string>)` called in Task 6 — defined in Task 4 ✓
- `ScannerService.FormatSize(long)` called in Task 6 — existing static method in `ScannerService.cs:65` ✓
- `_settings.HandBrakeCLIPath` used in Task 6 — added to `SettingsService` in Task 2 ✓
- `DataGridViewRow.Tag` used to store `CompressFile` and retrieve via `row.Tag as CompressFile` — consistent across Task 6 ✓
- `_grid.Rows[idx].Tag` set in `PopulateGrid()` and read in `SetFileStatus()`, `BtnCheckRecommended_Click()`, `BtnUncheckAll_Click()`, `SyncCheckboxesFromGrid()` — consistent ✓
- Grid column index 0 = checkbox, index 5 = status — consistent between designer (Task 5) and form logic (Task 6) ✓
