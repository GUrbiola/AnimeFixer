# MKV Roku Compatibility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add two new capabilities to AnimeFixer: HEVC 10-bit re-encoding to H.264 and embedded ASS subtitle extraction to SRT, both triggered from a new MkvRokuForm opened via a repurposed Form1 launcher.

**Architecture:** Form1 becomes a mode-selection launcher that opens either MP4FixerForm (existing) or the new MkvRokuForm. FFmpegService gains three new methods (ProbeMkv, ReencodeToH264, ExtractSubtitle). New models MkvMediaFile and SubtitleTrack carry MKV-specific state. Double-clicking a row in MkvRokuForm opens SubtitleExtractionDialog for per-track selection.

**Tech Stack:** WinForms .NET Framework 4.8 / C# 7.3, ffmpeg/ffprobe via Process, no new NuGet packages.

---

## File Map

**Create:**
- `AnimeFixer/Models/SubtitleTrack.cs` — subtitle stream data (index, language, title, codec, selected flag, naming helpers)
- `AnimeFixer/Models/MkvMediaFile.cs` — MKV file state: HEVC flag, subtitle list, chosen action, status
- `AnimeFixer/Services/MkvScannerService.cs` — folder scan restricted to `.mkv` files
- `AnimeFixer/MkvRokuForm.cs` — main MKV form: scan, re-encode, extract subs logic
- `AnimeFixer/MkvRokuForm.Designer.cs` — all controls defined for VS designer
- `AnimeFixer/MkvRokuForm.resx` — form resources (minimal)
- `AnimeFixer/SubtitleExtractionDialog.cs` — subtitle track selection dialog logic
- `AnimeFixer/SubtitleExtractionDialog.Designer.cs` — all controls defined for VS designer
- `AnimeFixer/SubtitleExtractionDialog.resx` — dialog resources (minimal)

**Modify:**
- `AnimeFixer/Services/FFmpegService.cs` — add `MkvProbeResult` class, `ProbeMkv()`, `RunProcessCancellable()`, `ReencodeToH264()`, `ExtractSubtitle()`
- `AnimeFixer/Form1.cs` — replace empty shell with launcher button handlers
- `AnimeFixer/Form1.Designer.cs` — replace empty designer with two-panel launcher layout
- `AnimeFixer/Program.cs` — change `Application.Run(new MP4FixerForm())` → `Application.Run(new Form1())`
- `AnimeFixer/AnimeFixer.csproj` — register all new `.cs` and `.resx` files

---

## Task 1: Add MKV Models

**Files:**
- Create: `AnimeFixer\Models\SubtitleTrack.cs`
- Create: `AnimeFixer\Models\MkvMediaFile.cs`

- [ ] **Step 1: Create SubtitleTrack.cs**

```csharp
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
```

- [ ] **Step 2: Create MkvMediaFile.cs**

```csharp
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
```

---

## Task 2: Extend FFmpegService

**Files:**
- Modify: `AnimeFixer\Services\FFmpegService.cs`

- [ ] **Step 1: Verify usings at top of FFmpegService.cs — add `System.Threading` if missing**

The file currently has:
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AnimeFixer.Models;
```

Add `using System.Threading;` after `using System.Text.RegularExpressions;`.

- [ ] **Step 2: Add MkvProbeResult class inside FFmpegService.cs (inside the namespace, outside the class)**

Add this class before `public class FFmpegService`:

```csharp
public class MkvProbeResult
{
    public bool                IsHevc10bit    { get; set; }
    public List<SubtitleTrack> SubtitleTracks { get; set; } = new List<SubtitleTrack>();
}
```

- [ ] **Step 3: Add ProbeMkv method inside FFmpegService class**

Add after the existing `Probe` method:

```csharp
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
        var blocks = SplitStreamBlocks(subJson);
        foreach (var block in blocks)
        {
            int    index   = ParseIntField(block, "index");
            string codec   = ParseStringField(block, "codec_name");
            string lang    = string.Empty;
            string title   = string.Empty;

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
```

- [ ] **Step 4: Add RunProcessCancellable method inside FFmpegService class**

Add after the existing `RunProcess` method:

```csharp
private string RunProcessCancellable(string exe, string args, CancellationToken ct,
    Action<string> stderrLine, out int exitCode)
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
                outputBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                stderrLine?.Invoke(e.Data);
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
```

- [ ] **Step 5: Add ReencodeToH264 method inside FFmpegService class**

Add after `RunProcessCancellable`:

```csharp
public bool ReencodeToH264(MkvMediaFile file, Action<string> logLine,
    CancellationToken ct, out string errorMessage)
{
    errorMessage = null;

    try
    {
        var dir       = Path.GetDirectoryName(file.FilePath);
        var nameNoExt = Path.GetFileNameWithoutExtension(file.FilePath);
        var output    = Path.Combine(dir, nameNoExt + "_roku.mkv");

        var args = $"-y -i \"{file.FilePath}\" " +
                   "-c:v libx264 -crf 18 -preset slow -profile:v high -level 4.1 " +
                   $"-pix_fmt yuv420p -c:a copy -c:s copy \"{output}\"";

        logLine?.Invoke($"ffmpeg {args}");

        string result = RunProcessCancellable(_ffmpegPath, args, ct,
            line => logLine?.Invoke("  " + line), out int exitCode);

        if (exitCode == 0 && File.Exists(output) && new FileInfo(output).Length > 0)
            return true;

        CleanupTemp(output);
        errorMessage = ExtractFFmpegError(result);
        return false;
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        errorMessage = ex.Message;
        return false;
    }
}
```

- [ ] **Step 6: Add ExtractSubtitle method inside FFmpegService class**

```csharp
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
            return true;

        errorMessage = ExtractFFmpegError(result);
        return false;
    }
    catch (Exception ex)
    {
        errorMessage = ex.Message;
        return false;
    }
}
```

---

## Task 3: MkvScannerService

**Files:**
- Create: `AnimeFixer\Services\MkvScannerService.cs`

- [ ] **Step 1: Create MkvScannerService.cs**

```csharp
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
```

---

## Task 4: SubtitleExtractionDialog

**Files:**
- Create: `AnimeFixer\SubtitleExtractionDialog.cs`
- Create: `AnimeFixer\SubtitleExtractionDialog.Designer.cs`
- Create: `AnimeFixer\SubtitleExtractionDialog.resx`

- [ ] **Step 1: Create SubtitleExtractionDialog.resx**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:element name="root" msdata:IsDataSet="true" />
  </xsd:schema>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
</root>
```

- [ ] **Step 2: Create SubtitleExtractionDialog.cs**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AnimeFixer.Models;

namespace AnimeFixer
{
    public partial class SubtitleExtractionDialog : Form
    {
        private readonly string            _videoFilePath;
        private readonly List<SubtitleTrack> _tracks;

        public List<SubtitleTrack> SelectedTracks { get; private set; }

        public SubtitleExtractionDialog(string videoFilePath, List<SubtitleTrack> tracks)
        {
            InitializeComponent();
            _videoFilePath = videoFilePath;
            _tracks        = tracks;
            _lblFile.Text  = Path.GetFileName(videoFilePath);
            PopulateList();
        }

        private void PopulateList()
        {
            var nameNoExt = Path.GetFileNameWithoutExtension(_videoFilePath);
            _listView.Items.Clear();

            foreach (var track in _tracks)
            {
                var item = new ListViewItem(track.Index.ToString());
                item.SubItems.Add(track.Language);
                item.SubItems.Add(track.Title);
                item.SubItems.Add(track.Codec);
                item.SubItems.Add($"{nameNoExt}.{track.SuggestedFileSuffix}.srt");
                item.Checked = track.Selected;
                item.Tag     = track;
                _listView.Items.Add(item);
            }
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _listView.Items)
                item.Checked = true;
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _listView.Items)
                item.Checked = false;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SelectedTracks = new List<SubtitleTrack>();
            foreach (ListViewItem item in _listView.Items)
            {
                if (item.Checked)
                {
                    var track = (SubtitleTrack)item.Tag;
                    track.Selected = true;
                    SelectedTracks.Add(track);
                }
            }

            if (SelectedTracks.Count == 0)
            {
                MessageBox.Show("Selecciona al menos una pista de subtítulos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
```

- [ ] **Step 3: Create SubtitleExtractionDialog.Designer.cs**

```csharp
namespace AnimeFixer
{
    partial class SubtitleExtractionDialog
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
            this._lblDescription = new System.Windows.Forms.Label();
            this._lblFile        = new System.Windows.Forms.Label();
            this._listView       = new System.Windows.Forms.ListView();
            this._colIndex       = new System.Windows.Forms.ColumnHeader();
            this._colLanguage    = new System.Windows.Forms.ColumnHeader();
            this._colTitle       = new System.Windows.Forms.ColumnHeader();
            this._colCodec       = new System.Windows.Forms.ColumnHeader();
            this._colOutputFile  = new System.Windows.Forms.ColumnHeader();
            this._panelBottom    = new System.Windows.Forms.Panel();
            this._btnSelectAll   = new System.Windows.Forms.Button();
            this._btnDeselectAll = new System.Windows.Forms.Button();
            this._btnOK          = new System.Windows.Forms.Button();
            this._btnCancel      = new System.Windows.Forms.Button();
            this._panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblDescription
            // 
            this._lblDescription.Location  = new System.Drawing.Point(12, 12);
            this._lblDescription.Name      = "_lblDescription";
            this._lblDescription.Size      = new System.Drawing.Size(760, 20);
            this._lblDescription.TabIndex  = 0;
            this._lblDescription.Text      = "Selecciona las pistas de subtítulos a extraer como archivos .srt:";
            // 
            // _lblFile
            // 
            this._lblFile.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblFile.Location = new System.Drawing.Point(12, 35);
            this._lblFile.Name     = "_lblFile";
            this._lblFile.Size     = new System.Drawing.Size(760, 20);
            this._lblFile.TabIndex = 1;
            this._lblFile.Text     = "(filename)";
            // 
            // _listView
            // 
            this._listView.Anchor = ((System.Windows.Forms.AnchorStyles)(
                ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right));
            this._listView.CheckBoxes = true;
            this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this._colIndex, this._colLanguage, this._colTitle,
                this._colCodec, this._colOutputFile });
            this._listView.FullRowSelect = true;
            this._listView.GridLines     = true;
            this._listView.Location      = new System.Drawing.Point(12, 60);
            this._listView.Name          = "_listView";
            this._listView.Size          = new System.Drawing.Size(760, 280);
            this._listView.TabIndex      = 2;
            this._listView.View          = System.Windows.Forms.View.Details;
            // 
            // _colIndex
            // 
            this._colIndex.Text  = "#";
            this._colIndex.Width = 40;
            // 
            // _colLanguage
            // 
            this._colLanguage.Text  = "Idioma";
            this._colLanguage.Width = 70;
            // 
            // _colTitle
            // 
            this._colTitle.Text  = "Título";
            this._colTitle.Width = 200;
            // 
            // _colCodec
            // 
            this._colCodec.Text  = "Codec";
            this._colCodec.Width = 60;
            // 
            // _colOutputFile
            // 
            this._colOutputFile.Text  = "Archivo de salida";
            this._colOutputFile.Width = 380;
            // 
            // _panelBottom
            // 
            this._panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(
                (System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right));
            this._panelBottom.Controls.Add(this._btnSelectAll);
            this._panelBottom.Controls.Add(this._btnDeselectAll);
            this._panelBottom.Controls.Add(this._btnOK);
            this._panelBottom.Controls.Add(this._btnCancel);
            this._panelBottom.Location = new System.Drawing.Point(0, 352);
            this._panelBottom.Name     = "_panelBottom";
            this._panelBottom.Size     = new System.Drawing.Size(784, 45);
            this._panelBottom.TabIndex = 3;
            // 
            // _btnSelectAll
            // 
            this._btnSelectAll.Location = new System.Drawing.Point(12, 10);
            this._btnSelectAll.Name     = "_btnSelectAll";
            this._btnSelectAll.Size     = new System.Drawing.Size(115, 26);
            this._btnSelectAll.TabIndex = 0;
            this._btnSelectAll.Text     = "Seleccionar todo";
            this._btnSelectAll.Click   += new System.EventHandler(this.BtnSelectAll_Click);
            // 
            // _btnDeselectAll
            // 
            this._btnDeselectAll.Location = new System.Drawing.Point(135, 10);
            this._btnDeselectAll.Name     = "_btnDeselectAll";
            this._btnDeselectAll.Size     = new System.Drawing.Size(125, 26);
            this._btnDeselectAll.TabIndex = 1;
            this._btnDeselectAll.Text     = "Deseleccionar todo";
            this._btnDeselectAll.Click   += new System.EventHandler(this.BtnDeselectAll_Click);
            // 
            // _btnOK
            // 
            this._btnOK.Anchor    = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
            this._btnOK.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this._btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnOK.ForeColor = System.Drawing.Color.White;
            this._btnOK.Location  = new System.Drawing.Point(556, 10);
            this._btnOK.Name      = "_btnOK";
            this._btnOK.Size      = new System.Drawing.Size(100, 26);
            this._btnOK.TabIndex  = 2;
            this._btnOK.Text      = "✔ Extraer";
            this._btnOK.UseVisualStyleBackColor = false;
            this._btnOK.Click    += new System.EventHandler(this.BtnOK_Click);
            // 
            // _btnCancel
            // 
            this._btnCancel.Anchor   = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
            this._btnCancel.Location = new System.Drawing.Point(664, 10);
            this._btnCancel.Name     = "_btnCancel";
            this._btnCancel.Size     = new System.Drawing.Size(100, 26);
            this._btnCancel.TabIndex = 3;
            this._btnCancel.Text     = "Cancelar";
            this._btnCancel.Click   += new System.EventHandler(this.BtnCancel_Click);
            // 
            // SubtitleExtractionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(784, 397);
            this.Controls.Add(this._lblDescription);
            this.Controls.Add(this._lblFile);
            this.Controls.Add(this._listView);
            this.Controls.Add(this._panelBottom);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.Name            = "SubtitleExtractionDialog";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text            = "Extracción de subtítulos";
            this._panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label        _lblDescription;
        private System.Windows.Forms.Label        _lblFile;
        private System.Windows.Forms.ListView     _listView;
        private System.Windows.Forms.ColumnHeader _colIndex;
        private System.Windows.Forms.ColumnHeader _colLanguage;
        private System.Windows.Forms.ColumnHeader _colTitle;
        private System.Windows.Forms.ColumnHeader _colCodec;
        private System.Windows.Forms.ColumnHeader _colOutputFile;
        private System.Windows.Forms.Panel        _panelBottom;
        private System.Windows.Forms.Button       _btnSelectAll;
        private System.Windows.Forms.Button       _btnDeselectAll;
        private System.Windows.Forms.Button       _btnOK;
        private System.Windows.Forms.Button       _btnCancel;
    }
}
```

---

## Task 5: MkvRokuForm Designer

**Files:**
- Create: `AnimeFixer\MkvRokuForm.resx`
- Create: `AnimeFixer\MkvRokuForm.Designer.cs`

- [ ] **Step 1: Create MkvRokuForm.resx** (same minimal XML as SubtitleExtractionDialog.resx above, just the filename differs — copy the content exactly)

- [ ] **Step 2: Create MkvRokuForm.Designer.cs**

```csharp
namespace AnimeFixer
{
    partial class MkvRokuForm
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
            this._topPanel          = new System.Windows.Forms.Panel();
            this._lblFFmpegPath     = new System.Windows.Forms.Label();
            this._txtFFmpegFolder   = new System.Windows.Forms.TextBox();
            this._btnBrowseFFmpeg   = new System.Windows.Forms.Button();
            this._lblFolder         = new System.Windows.Forms.Label();
            this._txtFolder         = new System.Windows.Forms.TextBox();
            this._btnBrowseFolder   = new System.Windows.Forms.Button();
            this._btnScan           = new System.Windows.Forms.Button();
            this._btnReencodeAll    = new System.Windows.Forms.Button();
            this._btnExtractSubsAll = new System.Windows.Forms.Button();
            this._btnBothAll        = new System.Windows.Forms.Button();
            this._btnCancel         = new System.Windows.Forms.Button();
            this._progressBar       = new System.Windows.Forms.ProgressBar();
            this._lblStatus         = new System.Windows.Forms.Label();
            this._chkVerboseLogging = new System.Windows.Forms.CheckBox();
            this._splitContainer    = new System.Windows.Forms.SplitContainer();
            this._listView          = new System.Windows.Forms.ListView();
            this._colFileName       = new System.Windows.Forms.ColumnHeader();
            this._colSize           = new System.Windows.Forms.ColumnHeader();
            this._colHevc           = new System.Windows.Forms.ColumnHeader();
            this._colSubs           = new System.Windows.Forms.ColumnHeader();
            this._colStatus         = new System.Windows.Forms.ColumnHeader();
            this._colAction         = new System.Windows.Forms.ColumnHeader();
            this._ctxMenu           = new System.Windows.Forms.ContextMenuStrip();
            this._ctxReencode       = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxExtractSubs    = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxBoth           = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxSeparator      = new System.Windows.Forms.ToolStripSeparator();
            this._ctxSkip           = new System.Windows.Forms.ToolStripMenuItem();
            this._txtLog            = new System.Windows.Forms.TextBox();
            this._statusStrip       = new System.Windows.Forms.StatusStrip();
            this._stripLabel        = new System.Windows.Forms.ToolStripStatusLabel();
            this._sep               = new System.Windows.Forms.Panel();
            this._topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this._ctxMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // _topPanel
            // 
            this._topPanel.BackColor = System.Drawing.Color.White;
            this._topPanel.Controls.Add(this._lblFFmpegPath);
            this._topPanel.Controls.Add(this._txtFFmpegFolder);
            this._topPanel.Controls.Add(this._btnBrowseFFmpeg);
            this._topPanel.Controls.Add(this._lblFolder);
            this._topPanel.Controls.Add(this._txtFolder);
            this._topPanel.Controls.Add(this._btnBrowseFolder);
            this._topPanel.Controls.Add(this._btnScan);
            this._topPanel.Controls.Add(this._btnReencodeAll);
            this._topPanel.Controls.Add(this._btnExtractSubsAll);
            this._topPanel.Controls.Add(this._btnBothAll);
            this._topPanel.Controls.Add(this._btnCancel);
            this._topPanel.Controls.Add(this._progressBar);
            this._topPanel.Controls.Add(this._lblStatus);
            this._topPanel.Controls.Add(this._chkVerboseLogging);
            this._topPanel.Dock     = System.Windows.Forms.DockStyle.Top;
            this._topPanel.Location = new System.Drawing.Point(0, 0);
            this._topPanel.Name     = "_topPanel";
            this._topPanel.Padding  = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this._topPanel.Size     = new System.Drawing.Size(1495, 130);
            this._topPanel.TabIndex = 2;
            // 
            // _lblFFmpegPath
            // 
            this._lblFFmpegPath.Location  = new System.Drawing.Point(12, 9);
            this._lblFFmpegPath.Name      = "_lblFFmpegPath";
            this._lblFFmpegPath.Size      = new System.Drawing.Size(180, 20);
            this._lblFFmpegPath.TabIndex  = 0;
            this._lblFFmpegPath.Text      = "Carpeta de ffmpeg / ffprobe:";
            this._lblFFmpegPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _txtFFmpegFolder
            // 
            this._txtFFmpegFolder.Anchor   = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._txtFFmpegFolder.Location = new System.Drawing.Point(198, 6);
            this._txtFFmpegFolder.Name     = "_txtFFmpegFolder";
            this._txtFFmpegFolder.Size     = new System.Drawing.Size(1247, 23);
            this._txtFFmpegFolder.TabIndex = 1;
            // 
            // _btnBrowseFFmpeg
            // 
            this._btnBrowseFFmpeg.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnBrowseFFmpeg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseFFmpeg.Location  = new System.Drawing.Point(1451, 4);
            this._btnBrowseFFmpeg.Name      = "_btnBrowseFFmpeg";
            this._btnBrowseFFmpeg.Size      = new System.Drawing.Size(32, 23);
            this._btnBrowseFFmpeg.TabIndex  = 2;
            this._btnBrowseFFmpeg.Text      = "...";
            // 
            // _lblFolder
            // 
            this._lblFolder.Location  = new System.Drawing.Point(12, 42);
            this._lblFolder.Name      = "_lblFolder";
            this._lblFolder.Size      = new System.Drawing.Size(180, 20);
            this._lblFolder.TabIndex  = 3;
            this._lblFolder.Text      = "Carpeta de medios:";
            this._lblFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _txtFolder
            // 
            this._txtFolder.Anchor   = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._txtFolder.Location = new System.Drawing.Point(198, 41);
            this._txtFolder.Name     = "_txtFolder";
            this._txtFolder.Size     = new System.Drawing.Size(1247, 23);
            this._txtFolder.TabIndex = 4;
            // 
            // _btnBrowseFolder
            // 
            this._btnBrowseFolder.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnBrowseFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseFolder.Location  = new System.Drawing.Point(1451, 40);
            this._btnBrowseFolder.Name      = "_btnBrowseFolder";
            this._btnBrowseFolder.Size      = new System.Drawing.Size(32, 24);
            this._btnBrowseFolder.TabIndex  = 5;
            this._btnBrowseFolder.Text      = "...";
            // 
            // _btnScan
            // 
            this._btnScan.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this._btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnScan.ForeColor = System.Drawing.Color.White;
            this._btnScan.Location  = new System.Drawing.Point(0, 80);
            this._btnScan.Name      = "_btnScan";
            this._btnScan.Size      = new System.Drawing.Size(130, 28);
            this._btnScan.TabIndex  = 6;
            this._btnScan.Text      = "🔍 Escanear MKV";
            this._btnScan.UseVisualStyleBackColor = false;
            // 
            // _btnReencodeAll
            // 
            this._btnReencodeAll.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this._btnReencodeAll.Enabled   = false;
            this._btnReencodeAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnReencodeAll.ForeColor = System.Drawing.Color.White;
            this._btnReencodeAll.Location  = new System.Drawing.Point(140, 80);
            this._btnReencodeAll.Name      = "_btnReencodeAll";
            this._btnReencodeAll.Size      = new System.Drawing.Size(145, 28);
            this._btnReencodeAll.TabIndex  = 7;
            this._btnReencodeAll.Text      = "🎬 Re-encode Todo";
            this._btnReencodeAll.UseVisualStyleBackColor = false;
            // 
            // _btnExtractSubsAll
            // 
            this._btnExtractSubsAll.BackColor = System.Drawing.Color.FromArgb(142, 68, 173);
            this._btnExtractSubsAll.Enabled   = false;
            this._btnExtractSubsAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnExtractSubsAll.ForeColor = System.Drawing.Color.White;
            this._btnExtractSubsAll.Location  = new System.Drawing.Point(295, 80);
            this._btnExtractSubsAll.Name      = "_btnExtractSubsAll";
            this._btnExtractSubsAll.Size      = new System.Drawing.Size(150, 28);
            this._btnExtractSubsAll.TabIndex  = 8;
            this._btnExtractSubsAll.Text      = "📝 Extraer Subtítulos";
            this._btnExtractSubsAll.UseVisualStyleBackColor = false;
            // 
            // _btnBothAll
            // 
            this._btnBothAll.BackColor = System.Drawing.Color.FromArgb(211, 84, 0);
            this._btnBothAll.Enabled   = false;
            this._btnBothAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBothAll.ForeColor = System.Drawing.Color.White;
            this._btnBothAll.Location  = new System.Drawing.Point(455, 80);
            this._btnBothAll.Name      = "_btnBothAll";
            this._btnBothAll.Size      = new System.Drawing.Size(100, 28);
            this._btnBothAll.TabIndex  = 9;
            this._btnBothAll.Text      = "⚡ Ambos";
            this._btnBothAll.UseVisualStyleBackColor = false;
            // 
            // _btnCancel
            // 
            this._btnCancel.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this._btnCancel.Enabled   = false;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.ForeColor = System.Drawing.Color.White;
            this._btnCancel.Location  = new System.Drawing.Point(565, 80);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(100, 28);
            this._btnCancel.TabIndex  = 10;
            this._btnCancel.Text      = "✖ Cancelar";
            this._btnCancel.UseVisualStyleBackColor = false;
            // 
            // _progressBar
            // 
            this._progressBar.Anchor   = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(675, 81);
            this._progressBar.Name     = "_progressBar";
            this._progressBar.Size     = new System.Drawing.Size(808, 27);
            this._progressBar.Style    = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 11;
            // 
            // _lblStatus
            // 
            this._lblStatus.Anchor    = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._lblStatus.ForeColor = System.Drawing.Color.Gray;
            this._lblStatus.Location  = new System.Drawing.Point(12, 110);
            this._lblStatus.Name      = "_lblStatus";
            this._lblStatus.Size      = new System.Drawing.Size(1314, 18);
            this._lblStatus.TabIndex  = 12;
            // 
            // _chkVerboseLogging
            // 
            this._chkVerboseLogging.Anchor   = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._chkVerboseLogging.AutoSize = true;
            this._chkVerboseLogging.Location = new System.Drawing.Point(1332, 110);
            this._chkVerboseLogging.Name     = "_chkVerboseLogging";
            this._chkVerboseLogging.Size     = new System.Drawing.Size(151, 19);
            this._chkVerboseLogging.TabIndex = 13;
            this._chkVerboseLogging.Text     = "Logging Mode: Verbose";
            this._chkVerboseLogging.UseVisualStyleBackColor = true;
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 131);
            this._splitContainer.Name     = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this._splitContainer.Panel1.Controls.Add(this._listView);
            this._splitContainer.Panel1MinSize = 100;
            this._splitContainer.Panel2.Controls.Add(this._txtLog);
            this._splitContainer.Panel2MinSize = 80;
            this._splitContainer.Size            = new System.Drawing.Size(1495, 528);
            this._splitContainer.SplitterDistance = 250;
            this._splitContainer.TabIndex        = 0;
            // 
            // _listView
            // 
            this._listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this._colFileName, this._colSize, this._colHevc,
                this._colSubs, this._colStatus, this._colAction });
            this._listView.ContextMenuStrip  = this._ctxMenu;
            this._listView.Dock              = System.Windows.Forms.DockStyle.Fill;
            this._listView.Font              = new System.Drawing.Font("Segoe UI", 8.5F);
            this._listView.FullRowSelect     = true;
            this._listView.GridLines         = true;
            this._listView.HideSelection     = false;
            this._listView.Location          = new System.Drawing.Point(0, 0);
            this._listView.MultiSelect       = false;
            this._listView.Name              = "_listView";
            this._listView.Size              = new System.Drawing.Size(1495, 250);
            this._listView.TabIndex          = 0;
            this._listView.UseCompatibleStateImageBehavior = false;
            this._listView.View              = System.Windows.Forms.View.Details;
            // 
            // _colFileName
            // 
            this._colFileName.Text  = "Archivo";
            this._colFileName.Width = 350;
            // 
            // _colSize
            // 
            this._colSize.Text  = "Tamaño";
            this._colSize.Width = 80;
            // 
            // _colHevc
            // 
            this._colHevc.Text  = "HEVC 10-bit";
            this._colHevc.Width = 95;
            // 
            // _colSubs
            // 
            this._colSubs.Text  = "Subtítulos";
            this._colSubs.Width = 85;
            // 
            // _colStatus
            // 
            this._colStatus.Text  = "Estado";
            this._colStatus.Width = 220;
            // 
            // _colAction
            // 
            this._colAction.Text  = "Acción";
            this._colAction.Width = 160;
            // 
            // _ctxMenu
            // 
            this._ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this._ctxReencode, this._ctxExtractSubs, this._ctxBoth,
                this._ctxSeparator, this._ctxSkip });
            this._ctxMenu.Name = "_ctxMenu";
            this._ctxMenu.Size = new System.Drawing.Size(225, 120);
            // 
            // _ctxReencode
            // 
            this._ctxReencode.Name = "_ctxReencode";
            this._ctxReencode.Size = new System.Drawing.Size(224, 22);
            this._ctxReencode.Text = "🎬 Re-encode video (H.264)";
            // 
            // _ctxExtractSubs
            // 
            this._ctxExtractSubs.Name = "_ctxExtractSubs";
            this._ctxExtractSubs.Size = new System.Drawing.Size(224, 22);
            this._ctxExtractSubs.Text = "📝 Extraer subtítulos";
            // 
            // _ctxBoth
            // 
            this._ctxBoth.Name = "_ctxBoth";
            this._ctxBoth.Size = new System.Drawing.Size(224, 22);
            this._ctxBoth.Text = "⚡ Ambos";
            // 
            // _ctxSeparator
            // 
            this._ctxSeparator.Name = "_ctxSeparator";
            this._ctxSeparator.Size = new System.Drawing.Size(221, 6);
            // 
            // _ctxSkip
            // 
            this._ctxSkip.Name = "_ctxSkip";
            this._ctxSkip.Size = new System.Drawing.Size(224, 22);
            this._ctxSkip.Text = "— Omitir";
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
            this._txtLog.Size        = new System.Drawing.Size(1495, 274);
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
            this._sep.Location  = new System.Drawing.Point(0, 130);
            this._sep.Name      = "_sep";
            this._sep.Size      = new System.Drawing.Size(1495, 1);
            this._sep.TabIndex  = 1;
            // 
            // MkvRokuForm
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
            this.Name          = "MkvRokuForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text          = "MKV / Roku Fixer — HEVC + Subtítulos";
            this.WindowState   = System.Windows.Forms.FormWindowState.Maximized;
            this._topPanel.ResumeLayout(false);
            this._topPanel.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this._ctxMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel              _topPanel;
        private System.Windows.Forms.Label              _lblFFmpegPath;
        private System.Windows.Forms.TextBox            _txtFFmpegFolder;
        private System.Windows.Forms.Button             _btnBrowseFFmpeg;
        private System.Windows.Forms.Label              _lblFolder;
        private System.Windows.Forms.TextBox            _txtFolder;
        private System.Windows.Forms.Button             _btnBrowseFolder;
        private System.Windows.Forms.Button             _btnScan;
        private System.Windows.Forms.Button             _btnReencodeAll;
        private System.Windows.Forms.Button             _btnExtractSubsAll;
        private System.Windows.Forms.Button             _btnBothAll;
        private System.Windows.Forms.Button             _btnCancel;
        private System.Windows.Forms.ProgressBar        _progressBar;
        private System.Windows.Forms.Label              _lblStatus;
        private System.Windows.Forms.CheckBox           _chkVerboseLogging;
        private System.Windows.Forms.SplitContainer     _splitContainer;
        private System.Windows.Forms.ListView           _listView;
        private System.Windows.Forms.ColumnHeader       _colFileName;
        private System.Windows.Forms.ColumnHeader       _colSize;
        private System.Windows.Forms.ColumnHeader       _colHevc;
        private System.Windows.Forms.ColumnHeader       _colSubs;
        private System.Windows.Forms.ColumnHeader       _colStatus;
        private System.Windows.Forms.ColumnHeader       _colAction;
        private System.Windows.Forms.ContextMenuStrip   _ctxMenu;
        private System.Windows.Forms.ToolStripMenuItem  _ctxReencode;
        private System.Windows.Forms.ToolStripMenuItem  _ctxExtractSubs;
        private System.Windows.Forms.ToolStripMenuItem  _ctxBoth;
        private System.Windows.Forms.ToolStripSeparator _ctxSeparator;
        private System.Windows.Forms.ToolStripMenuItem  _ctxSkip;
        private System.Windows.Forms.TextBox            _txtLog;
        private System.Windows.Forms.StatusStrip        _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _stripLabel;
        private System.Windows.Forms.Panel              _sep;
    }
}
```

---

## Task 6: MkvRokuForm Logic

**Files:**
- Create: `AnimeFixer\MkvRokuForm.cs`

- [ ] **Step 1: Create MkvRokuForm.cs**

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
    public partial class MkvRokuForm : Form
    {
        private readonly SettingsService  _settings = new SettingsService();
        private FFmpegService             _ffmpeg;
        private CancellationTokenSource  _cts;
        private List<MkvMediaFile>       _files    = new List<MkvMediaFile>();
        private bool                     _verboseLogging;

        public MkvRokuForm()
        {
            InitializeComponent();
            ApplySettings();
            TryAutoDetectFFmpeg();
            WireEvents();
        }

        private void WireEvents()
        {
            _btnBrowseFFmpeg.Click   += BtnBrowseFFmpeg_Click;
            _btnBrowseFolder.Click   += BtnBrowseFolder_Click;
            _btnScan.Click           += BtnScan_Click;
            _btnReencodeAll.Click    += BtnReencodeAll_Click;
            _btnExtractSubsAll.Click += BtnExtractSubsAll_Click;
            _btnBothAll.Click        += BtnBothAll_Click;
            _btnCancel.Click         += BtnCancel_Click;
            _listView.DoubleClick    += ListView_DoubleClick;
            _ctxReencode.Click       += (s, e) => SetActionForSelected(MkvAction.ReencodeVideo);
            _ctxExtractSubs.Click    += (s, e) => SetActionForSelected(MkvAction.ExtractSubtitles);
            _ctxBoth.Click           += (s, e) => SetActionForSelected(MkvAction.Both);
            _ctxSkip.Click           += (s, e) => SetActionForSelected(MkvAction.None);

            _chkVerboseLogging.CheckedChanged += (s, e) =>
            {
                _verboseLogging  = _chkVerboseLogging.Checked;
                _stripLabel.Text = _verboseLogging ? "Modo Verbose ACTIVADO" : "Modo Simple ACTIVADO";
            };

            var logMenu = new ContextMenuStrip();
            logMenu.Items.Add("Limpiar Log", null, (s, e) => _txtLog.Clear());
            _txtLog.ContextMenuStrip = logMenu;

            this.FormClosing += (s, e) => SaveSettings();
        }

        private void ApplySettings()
        {
            _txtFFmpegFolder.Text = _settings.FFmpegFolder;
            _txtFolder.Text       = _settings.LastFolder;
        }

        private void SaveSettings()
        {
            _settings.FFmpegFolder = _txtFFmpegFolder.Text.Trim();
            _settings.LastFolder   = _txtFolder.Text.Trim();
            _settings.Save();
        }

        private void TryAutoDetectFFmpeg()
        {
            if (!string.IsNullOrEmpty(_settings.FFmpegFolder) &&
                File.Exists(_settings.FFprobePath) && File.Exists(_settings.FFmpegPath))
                return;

            var detected = SettingsService.TryAutoDetectFFmpegFolder();
            if (!string.IsNullOrEmpty(detected))
            {
                _txtFFmpegFolder.Text  = detected;
                _settings.FFmpegFolder = detected;
            }
        }

        private bool BuildFFmpegService()
        {
            _settings.FFmpegFolder = _txtFFmpegFolder.Text.Trim();
            _ffmpeg = new FFmpegService(_settings.FFprobePath, _settings.FFmpegPath);
            if (!_ffmpeg.Validate(out string error))
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void BtnBrowseFFmpeg_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog
            {
                Description  = "Selecciona la carpeta que contiene ffmpeg.exe y ffprobe.exe",
                SelectedPath = _txtFFmpegFolder.Text
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    _txtFFmpegFolder.Text = dlg.SelectedPath;
            }
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog
            {
                Description  = "Selecciona la carpeta raíz de medios a escanear",
                SelectedPath = _txtFolder.Text
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    _txtFolder.Text = dlg.SelectedPath;
            }
        }

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            var folder = _txtFolder.Text.Trim();
            if (string.IsNullOrEmpty(folder))
            {
                MessageBox.Show("Por favor, selecciona una carpeta de medios.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("La carpeta especificada no existe.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!BuildFFmpegService()) return;

            SetBusy(true);
            ClearList();
            Log("── Iniciando escaneo MKV ─────────────────────────────────────");
            Log(folder);
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Run(() =>
                {
                    SetStatus("Buscando archivos MKV...");
                    var scanner = new MkvScannerService();
                    _files = scanner.Scan(folder, fp =>
                        LogVerbose($"  ✓ Encontrado: {Path.GetFileName(fp)} ({ScannerService.FormatSize(new FileInfo(fp).Length)})"));

                    if (_files.Count == 0)
                    {
                        SetStatus("No se encontraron archivos MKV.");
                        SetBusy(false);
                        return;
                    }

                    LogSimple($"Se encontraron {_files.Count} archivo(s) MKV");
                    PopulateList();
                    SetProgress(0, _files.Count);

                    int analyzed = 0;
                    int hevcCount = 0;

                    foreach (var mf in _files)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        analyzed++;
                        UpdateFileStatus(mf, MkvFileStatus.Analyzing);
                        SetStatus($"Analizando {analyzed}/{_files.Count}: {mf.FileName}");

                        try
                        {
                            var probe          = _ffmpeg.ProbeMkv(mf.FilePath, line => LogVerbose(line));
                            mf.IsHevc10bit    = probe.IsHevc10bit;
                            mf.SubtitleTracks = probe.SubtitleTracks;

                            if (probe.IsHevc10bit)
                            {
                                mf.Status = MkvFileStatus.NeedsFixing;
                                mf.Action = MkvAction.Both;
                                hevcCount++;
                                LogSimple($"{mf.FileName} - ⚠ HEVC 10-bit, {probe.SubtitleTracks.Count} sub(s)");
                            }
                            else
                            {
                                mf.Status = MkvFileStatus.Clean;
                                LogSimple($"{mf.FileName} - ✔ Sin HEVC 10-bit, {probe.SubtitleTracks.Count} sub(s)");
                            }
                        }
                        catch (Exception ex)
                        {
                            mf.Status       = MkvFileStatus.Failed;
                            mf.ErrorMessage = ex.Message;
                            LogSimple($"{mf.FileName} - ✘ Error: {ex.Message}");
                        }

                        UpdateFileStatus(mf, mf.Status);
                        SetProgress(analyzed, _files.Count);
                    }

                    LogSimple(string.Empty);
                    string summary = $"Escaneo MKV completo: {_files.Count} archivo(s) — {hevcCount} con HEVC 10-bit.";
                    SetStatus(summary);
                    LogSimple(summary);
                    _stripLabel.Text           = summary;
                    _btnReencodeAll.Enabled    = hevcCount > 0;
                    _btnExtractSubsAll.Enabled = hevcCount > 0;
                    _btnBothAll.Enabled        = hevcCount > 0;
                }, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log("Escaneo cancelado.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void BtnReencodeAll_Click(object sender, EventArgs e)
        {
            await ProcessFiles(MkvAction.ReencodeVideo);
        }

        private async void BtnExtractSubsAll_Click(object sender, EventArgs e)
        {
            await ProcessFiles(MkvAction.ExtractSubtitles);
        }

        private async void BtnBothAll_Click(object sender, EventArgs e)
        {
            await ProcessFiles(MkvAction.Both);
        }

        private async Task ProcessFiles(MkvAction action)
        {
            var toProcess = new List<MkvMediaFile>();
            foreach (var f in _files)
            {
                if (f.Status != MkvFileStatus.NeedsFixing && f.Status != MkvFileStatus.Clean)
                    continue;
                if (action == MkvAction.ExtractSubtitles && f.SubtitleTracks.Count == 0)
                    continue;
                if (action == MkvAction.ReencodeVideo && !f.IsHevc10bit)
                    continue;
                toProcess.Add(f);
            }

            if (toProcess.Count == 0)
            {
                MessageBox.Show("No hay archivos para procesar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string actionText;
            switch (action)
            {
                case MkvAction.ReencodeVideo:    actionText = "re-encodear";               break;
                case MkvAction.ExtractSubtitles: actionText = "extraer subtítulos de";     break;
                default:                          actionText = "procesar (re-encode + subs) de"; break;
            }

            var confirm = MessageBox.Show(
                $"Se van a {actionText} {toProcess.Count} archivo(s).\n\n¿Continuar?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            SetBusy(true);
            _btnReencodeAll.Enabled    = false;
            _btnExtractSubsAll.Enabled = false;
            _btnBothAll.Enabled        = false;
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Run(() =>
                {
                    int done = 0, failed = 0;

                    for (int i = 0; i < toProcess.Count; i++)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        var mf = toProcess[i];
                        UpdateFileStatus(mf, MkvFileStatus.Processing);
                        SetStatus($"Procesando {i + 1}/{toProcess.Count}: {mf.FileName}");
                        LogSimple($"Procesando {mf.FileName}...");

                        bool ok  = true;
                        string err = null;

                        if ((action == MkvAction.ReencodeVideo || action == MkvAction.Both)
                            && mf.IsHevc10bit)
                        {
                            ok = _ffmpeg.ReencodeToH264(mf, line => LogVerbose("  " + line),
                                _cts.Token, out err);
                            if (ok) LogSimple($"  ✔ Re-encode completado → {Path.GetFileNameWithoutExtension(mf.FilePath)}_roku.mkv");
                            else    LogSimple($"  ✘ Re-encode falló: {err}");
                        }

                        if (ok && (action == MkvAction.ExtractSubtitles || action == MkvAction.Both)
                            && mf.SubtitleTracks.Count > 0)
                        {
                            ok = ExtractSubtitlesForFile(mf, out err);
                        }

                        if (ok)
                        {
                            mf.Status = MkvFileStatus.Done;
                            done++;
                            LogSimple($"{mf.FileName} - ✔ Completado");
                        }
                        else
                        {
                            mf.Status       = MkvFileStatus.Failed;
                            mf.ErrorMessage = err;
                            failed++;
                            LogSimple($"{mf.FileName} - ✘ Error: {err}");
                        }

                        UpdateFileStatus(mf, mf.Status);
                        SetProgress(i + 1, toProcess.Count);
                    }

                    LogSimple(string.Empty);
                    string summary = $"Proceso completo: ✔ {done}  ✘ {failed}.";
                    SetStatus(summary);
                    LogSimple(summary);
                    _stripLabel.Text = summary;
                }, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log("Proceso cancelado.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private bool ExtractSubtitlesForFile(MkvMediaFile mf, out string errorMessage)
        {
            errorMessage = null;
            var dir       = Path.GetDirectoryName(mf.FilePath);
            var nameNoExt = Path.GetFileNameWithoutExtension(mf.FilePath);

            foreach (var track in mf.SubtitleTracks)
            {
                if (!track.Selected) continue;

                var outputPath = Path.Combine(dir, $"{nameNoExt}.{track.SuggestedFileSuffix}.srt");
                LogVerbose($"  Extrayendo sub#{track.Index} ({track.DisplayTitle}) → {Path.GetFileName(outputPath)}");

                if (!_ffmpeg.ExtractSubtitle(mf.FilePath, track, outputPath,
                    line => LogVerbose("    " + line), out string err))
                {
                    errorMessage = $"Sub#{track.Index}: {err}";
                    LogSimple($"  ✘ Falló sub#{track.Index}: {err}");
                }
                else
                {
                    LogSimple($"  ✔ Extraído: {Path.GetFileName(outputPath)}");
                }
            }

            return errorMessage == null;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            _btnCancel.Enabled = false;
        }

        private void SetActionForSelected(MkvAction action)
        {
            if (_listView.SelectedItems.Count == 0) return;
            int index = (int)_listView.SelectedItems[0].Tag;
            var mf    = _files[index];
            mf.Action = action;
            UpdateFileStatus(mf, mf.Status);
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0) return;

            int index = (int)_listView.SelectedItems[0].Tag;
            var mf    = _files[index];

            if (mf.SubtitleTracks.Count == 0)
            {
                MessageBox.Show(
                    $"Archivo: {mf.FilePath}\nTamaño: {ScannerService.FormatSize(mf.FileSizeBytes)}\n" +
                    $"HEVC 10-bit: {(mf.IsHevc10bit ? "Sí" : "No")}\nSubtítulos: ninguno",
                    "Detalles", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SubtitleExtractionDialog(mf.FilePath, mf.SubtitleTracks))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    mf.SubtitleTracks = dlg.SelectedTracks;
                    UpdateFileStatus(mf, mf.Status);
                }
            }
        }

        private void PopulateList()
        {
            if (InvokeRequired) { Invoke(new Action(PopulateList)); return; }

            _listView.BeginUpdate();
            _listView.Items.Clear();

            for (int i = 0; i < _files.Count; i++)
            {
                var mf   = _files[i];
                var item = new ListViewItem(mf.FileName);
                item.SubItems.Add(ScannerService.FormatSize(mf.FileSizeBytes));
                item.SubItems.Add("...");
                item.SubItems.Add("...");
                item.SubItems.Add(mf.StatusDisplay);
                item.SubItems.Add(mf.ActionDisplay);
                item.Tag = i;
                _listView.Items.Add(item);
            }

            _listView.EndUpdate();
        }

        private void UpdateFileStatus(MkvMediaFile mf, MkvFileStatus status)
        {
            if (InvokeRequired) { Invoke(new Action(() => UpdateFileStatus(mf, status))); return; }

            mf.Status = status;

            foreach (ListViewItem item in _listView.Items)
            {
                if ((int)item.Tag != _files.IndexOf(mf)) continue;

                if (status != MkvFileStatus.Analyzing && status != MkvFileStatus.Pending)
                {
                    item.SubItems[2].Text = mf.IsHevc10bit ? "⚠ Sí" : "✔ No";
                    item.SubItems[3].Text = mf.SubtitleTracks.Count > 0
                        ? $"{mf.SubtitleTracks.Count} pista(s)"
                        : "—";
                }

                item.SubItems[4].Text = mf.StatusDisplay;
                item.SubItems[5].Text = mf.ActionDisplay;

                switch (status)
                {
                    case MkvFileStatus.NeedsFixing: item.ForeColor = Color.FromArgb(180, 80, 0);  break;
                    case MkvFileStatus.Done:        item.ForeColor = Color.FromArgb(30, 130, 60); break;
                    case MkvFileStatus.Failed:      item.ForeColor = Color.FromArgb(180, 0, 0);   break;
                    case MkvFileStatus.Clean:       item.ForeColor = Color.FromArgb(80, 80, 80);  break;
                    default:                        item.ForeColor = _listView.ForeColor;          break;
                }

                break;
            }
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetBusy(busy))); return; }
            _btnScan.Enabled         = !busy;
            _btnBrowseFolder.Enabled = !busy;
            _btnBrowseFFmpeg.Enabled = !busy;
            _btnCancel.Enabled       = busy;
            _progressBar.Visible     = busy;
            if (!busy) _progressBar.Value = 0;
        }

        private void SetProgress(int value, int max)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetProgress(value, max))); return; }
            _progressBar.Maximum = max;
            _progressBar.Value   = Math.Min(value, max);
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetStatus(text))); return; }
            _lblStatus.Text = text;
        }

        private void Log(string line)     => LogSimple(line);
        private void LogSimple(string line)
        {
            if (InvokeRequired) { Invoke(new Action(() => LogSimple(line))); return; }
            _txtLog.AppendText($"{line}{Environment.NewLine}");
        }

        private void LogVerbose(string line)
        {
            if (!_verboseLogging) return;
            if (InvokeRequired) { Invoke(new Action(() => LogVerbose(line))); return; }
            _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
        }

        private void ClearList()
        {
            if (InvokeRequired) { Invoke(new Action(ClearList)); return; }
            _listView.Items.Clear();
            _txtLog.Clear();
            _btnReencodeAll.Enabled    = false;
            _btnExtractSubsAll.Enabled = false;
            _btnBothAll.Enabled        = false;
            _stripLabel.Text           = "Listo.";
        }
    }
}
```

---

## Task 7: Form1 Launcher

**Files:**
- Modify: `AnimeFixer\Form1.cs`
- Modify: `AnimeFixer\Form1.Designer.cs`

- [ ] **Step 1: Replace Form1.cs with launcher logic**

```csharp
using System;
using System.Windows.Forms;

namespace AnimeFixer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnMp4Fixer_Click(object sender, EventArgs e)
        {
            new MP4FixerForm().Show();
        }

        private void BtnMkvFixer_Click(object sender, EventArgs e)
        {
            new MkvRokuForm().Show();
        }
    }
}
```

- [ ] **Step 2: Replace Form1.Designer.cs with two-panel launcher layout**

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
            this._lblTitle    = new System.Windows.Forms.Label();
            this._pnlMp4      = new System.Windows.Forms.Panel();
            this._lblMp4Title = new System.Windows.Forms.Label();
            this._lblMp4Desc  = new System.Windows.Forms.Label();
            this._btnMp4Fixer = new System.Windows.Forms.Button();
            this._pnlMkv      = new System.Windows.Forms.Panel();
            this._lblMkvTitle = new System.Windows.Forms.Label();
            this._lblMkvDesc  = new System.Windows.Forms.Label();
            this._btnMkvFixer = new System.Windows.Forms.Button();
            this._pnlMp4.SuspendLayout();
            this._pnlMkv.SuspendLayout();
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
            this._lblTitle.Size      = new System.Drawing.Size(700, 70);
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
            this._pnlMp4.Location    = new System.Drawing.Point(30, 90);
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
            this._btnMp4Fixer.Text      = "Abrir MP4 Fixer →";
            this._btnMp4Fixer.UseVisualStyleBackColor = false;
            this._btnMp4Fixer.Click    += new System.EventHandler(this.BtnMp4Fixer_Click);
            // 
            // _pnlMkv
            // 
            this._pnlMkv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pnlMkv.Controls.Add(this._lblMkvTitle);
            this._pnlMkv.Controls.Add(this._lblMkvDesc);
            this._pnlMkv.Controls.Add(this._btnMkvFixer);
            this._pnlMkv.Location    = new System.Drawing.Point(360, 90);
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
            this._btnMkvFixer.Text      = "Abrir MKV / Roku Fixer →";
            this._btnMkvFixer.UseVisualStyleBackColor = false;
            this._btnMkvFixer.Click    += new System.EventHandler(this.BtnMkvFixer_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.FromArgb(245, 245, 248);
            this.ClientSize          = new System.Drawing.Size(700, 310);
            this.Controls.Add(this._lblTitle);
            this.Controls.Add(this._pnlMp4);
            this.Controls.Add(this._pnlMkv);
            this.Font            = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Name            = "Form1";
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text            = "AnimeFixer";
            this._pnlMp4.ResumeLayout(false);
            this._pnlMkv.ResumeLayout(false);
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
    }
}
```

---

## Task 8: Update Program.cs and AnimeFixer.csproj

**Files:**
- Modify: `AnimeFixer\Program.cs`
- Modify: `AnimeFixer\AnimeFixer.csproj`

- [ ] **Step 1: Update Program.cs — change startup form to Form1**

Replace the entire file with:

```csharp
using System.Windows.Forms;

namespace AnimeFixer
{
    internal static class Program
    {
        [System.STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
```

- [ ] **Step 2: Add new files to AnimeFixer.csproj**

In the `<ItemGroup>` containing `<Compile>` entries (after the existing `MP4FixerForm.Designer.cs` entry), add:

```xml
<Compile Include="MkvRokuForm.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="MkvRokuForm.Designer.cs">
  <DependentUpon>MkvRokuForm.cs</DependentUpon>
</Compile>
<Compile Include="SubtitleExtractionDialog.cs">
  <SubType>Form</SubType>
</Compile>
<Compile Include="SubtitleExtractionDialog.Designer.cs">
  <DependentUpon>SubtitleExtractionDialog.cs</DependentUpon>
</Compile>
<Compile Include="Models\MkvMediaFile.cs" />
<Compile Include="Models\SubtitleTrack.cs" />
<Compile Include="Services\MkvScannerService.cs" />
```

In the `<ItemGroup>` containing `<EmbeddedResource>` entries, add:

```xml
<EmbeddedResource Include="MkvRokuForm.resx">
  <DependentUpon>MkvRokuForm.cs</DependentUpon>
</EmbeddedResource>
<EmbeddedResource Include="SubtitleExtractionDialog.resx">
  <DependentUpon>SubtitleExtractionDialog.cs</DependentUpon>
</EmbeddedResource>
```

- [ ] **Step 3: Build the solution**

```
msbuild AnimeFixer\AnimeFixer.csproj /p:Configuration=Debug
```

Expected: 0 errors. Common errors to fix:
- `error CS8107: Feature 'switch expression' is not available in C# 7.3` — replace any `x switch { }` with `switch (x) { case: }` (none should be in the plan above, but double-check)
- `error CS0246: The type or namespace 'MkvProbeResult' could not be found` — ensure the class is inside `namespace AnimeFixer.Services` but accessible; move to `Models` if needed
- Missing `using System.Threading;` in FFmpegService.cs

- [ ] **Step 4: Smoke-test — launch AnimeFixer.exe**

Run `AnimeFixer\bin\Debug\AnimeFixer.exe`.

Verify:
1. Launcher window appears with two mode panels side-by-side
2. "Abrir MP4 Fixer →" opens the existing MP4FixerForm
3. "Abrir MKV / Roku Fixer →" opens MkvRokuForm
4. MkvRokuForm auto-detects ffmpeg folder (same as MP4FixerForm)

- [ ] **Step 5: End-to-end test with Made in Abyss test file**

In MkvRokuForm:
1. Set media folder to `X:\Media\Movies\Made in Abyss Dawn of the Deep Soul (2020) tt10068916\`
2. Click "🔍 Escanear MKV"
3. Verify the file appears as `⚠ HEVC 10-bit detectado` with `11 pista(s)` in the Subtítulos column
4. Double-click the row → subtitle dialog opens showing 11 ASS tracks
5. Uncheck all except `eng` (index 4) and `spa` (index 6), click "✔ Extraer"
6. Click "📝 Extraer Subtítulos" → two .srt files appear next to the .mkv:
   - `Made in Abyss Movie 3 - Fukaki Tamashii no Reimei.eng.english.srt`
   - `Made in Abyss Movie 3 - Fukaki Tamashii no Reimei.spa.spanish.srt`

---

## Spec Coverage Verification

| Spec requirement | Task covering it |
|---|---|
| Scan MKV for HEVC + yuv420p10le | Task 2 `ProbeMkv` + Task 3 `MkvScannerService` + Task 6 `BtnScan_Click` |
| Re-encode: `-c:v libx264 -crf 18 -preset slow -profile:v high -level 4.1 -pix_fmt yuv420p -c:a copy -c:s copy` | Task 2 `ReencodeToH264` |
| Output suffix `_roku.mkv` | Task 2 `nameNoExt + "_roku.mkv"` |
| Probe subtitle streams (index, codec, language, title) | Task 2 `ProbeMkv` subtitle section |
| Extract each ASS track to SRT via `ffmpeg -map 0:N -c:s srt` | Task 2 `ExtractSubtitle` |
| SRT naming: `{videoName}.{lang}.{title}.srt` (Jellyfin convention) | Task 1 `SuggestedFileSuffix`, Task 6 `ExtractSubtitlesForFile` |
| New scan mode UI alongside existing GPAC scanner | Task 7 Form1 launcher + Task 5/6 MkvRokuForm |
| Per-row action: Re-encode / Extract Subs / Both | Task 5 context menu, Task 6 `SetActionForSelected` |
| Subtitle extraction panel: show streams, check/uncheck, show output names | Task 4 `SubtitleExtractionDialog` |
| Progress bar + cancellation for re-encode | Task 2 `RunProcessCancellable` + Task 6 `SetProgress` + `BtnCancel_Click` |
| Reuse same ffmpeg path resolution | Task 6 uses `SettingsService` (same instance type as MP4FixerForm) |
| Log all ffmpeg commands and exit codes | Task 2: `logLine?.Invoke($"ffmpeg {args}")`, exit code logged via verbose |
