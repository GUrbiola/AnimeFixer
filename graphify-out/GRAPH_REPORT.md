# Graph Report - .  (2026-06-17)

## Corpus Check
- Corpus is ~25,465 words - fits in a single context window. You may not need a graph.

## Summary
- 377 nodes · 517 edges · 26 communities (19 shown, 7 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 31 edges (avg confidence: 0.89)
- Token cost: 5,200 input · 1,450 output

## Community Hubs (Navigation)
- [[_COMMUNITY_CompressForm Designer|CompressForm Designer]]
- [[_COMMUNITY_MkvRokuForm Logic|MkvRokuForm Logic]]
- [[_COMMUNITY_CompressForm Logic|CompressForm Logic]]
- [[_COMMUNITY_MP4FixerForm Logic|MP4FixerForm Logic]]
- [[_COMMUNITY_Compress & HandBrake Service|Compress & HandBrake Service]]
- [[_COMMUNITY_FFmpeg Media Service|FFmpeg Media Service]]
- [[_COMMUNITY_MkvRokuForm Designer|MkvRokuForm Designer]]
- [[_COMMUNITY_MKV Scanner & Roku Encoding|MKV Scanner & Roku Encoding]]
- [[_COMMUNITY_Media Models & HEVC Detection|Media Models & HEVC Detection]]
- [[_COMMUNITY_Subtitle Extraction Dialog|Subtitle Extraction Dialog]]
- [[_COMMUNITY_Main Form & Program Entry|Main Form & Program Entry]]
- [[_COMMUNITY_FFmpeg Parsing Pipeline|FFmpeg Parsing Pipeline]]
- [[_COMMUNITY_Subtitle Dialog Designer|Subtitle Dialog Designer]]
- [[_COMMUNITY_Form1 Designer|Form1 Designer]]
- [[_COMMUNITY_Scanner Service|Scanner Service]]
- [[_COMMUNITY_Resources & Culture Settings|Resources & Culture Settings]]
- [[_COMMUNITY_App Settings|App Settings]]
- [[_COMMUNITY_Project & Solution Files|Project & Solution Files]]
- [[_COMMUNITY_MIT License|MIT License]]
- [[_COMMUNITY_GitHub Copilot Instructions|GitHub Copilot Instructions]]
- [[_COMMUNITY_Assembly Info B|Assembly Info B]]
- [[_COMMUNITY_FFmpeg Validate|FFmpeg Validate]]
- [[_COMMUNITY_Scanner Format Size|Scanner Format Size]]
- [[_COMMUNITY_Scanner Scan|Scanner Scan]]

## God Nodes (most connected - your core abstractions)
1. `MkvRokuForm` - 49 edges
2. `MP4FixerForm` - 38 edges
3. `CompressForm` - 36 edges
4. `CompressForm` - 23 edges
5. `FFmpegService` - 22 edges
6. `MP4FixerForm` - 21 edges
7. `MkvRokuForm` - 21 edges
8. `SubtitleExtractionDialog` - 14 edges
9. `HandBrakeService` - 11 edges
10. `SettingsService` - 11 edges

## Surprising Connections (you probably didn't know these)
- `MKV Roku Compatibility Implementation Plan` --references--> `FFmpegService`  [EXTRACTED]
  D:/Just For Fun/CidAnimeFixer/AnimeFixer/docs/superpowers/plans/2026-06-17-mkv-roku-compatibility.md → AnimeFixer/Services/FFmpegService.cs
- `MKV Roku Compatibility Implementation Plan` --references--> `SubtitleExtractionDialog`  [EXTRACTED]
  D:/Just For Fun/CidAnimeFixer/AnimeFixer/docs/superpowers/plans/2026-06-17-mkv-roku-compatibility.md → AnimeFixer/SubtitleExtractionDialog.cs
- `MP4FixerForm` --calls--> `FFmpegService (external dependency reference)`  [EXTRACTED]
  AnimeFixer/MP4FixerForm.cs → D:/Just For Fun/CidAnimeFixer/AnimeFixer/AnimeFixer/MP4FixerForm.cs
- `MP4FixerForm` --calls--> `ScannerService (external dependency reference)`  [EXTRACTED]
  AnimeFixer/MP4FixerForm.cs → D:/Just For Fun/CidAnimeFixer/AnimeFixer/AnimeFixer/MP4FixerForm.cs
- `MkvRokuForm` --references--> `NVIDIA NVENC Hardware Encoding`  [EXTRACTED]
  AnimeFixer/MkvRokuForm.cs → D:/Just For Fun/CidAnimeFixer/AnimeFixer/AnimeFixer/MkvRokuForm.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Compress Pipeline: CompressForm + HandBrakeService + CompressFile** — animefixer_compressform_compressform, concept_handbrakeservice, models_compressfile_compressfile [EXTRACTED 1.00]
- **MP4 GPAC Fix Pipeline: MP4FixerForm + FFmpegService + MediaFile** — animefixer_mp4fixerform_mp4fixerform, concept_ffmpegservice, models_mediafile_mediafile [EXTRACTED 1.00]
- **MKV Roku Fix Pipeline: MkvRokuForm + FFmpegService + MkvMediaFile** — animefixer_mkvrokufrom_mkvrokufrom, concept_ffmpegservice, models_mkvmediafile_mkvmediafile [EXTRACTED 1.00]
- **FFmpeg MKV Probe + Subtitle Extraction Pipeline** — services_ffmpegservice_probemkv, services_ffmpegservice_extractsubtitle, services_ffmpegservice_cleansrtfile [INFERRED 0.95]
- **Large File Compression Workflow** — services_compressscannerservice_compressscannerservice, services_handbrakeservice_handbrakeservice, services_settingsservice_settingsservice [INFERRED 0.95]
- **MKV Roku Fix Workflow (scan, re-encode, extract subs)** — services_mkvscannerservice_mkvscannerservice, services_ffmpegservice_reencodetoh264, subtitleextractiondialog_subtitleextractiondialog [INFERRED 0.95]
- **Shared Top-Panel + SplitContainer + StatusStrip Form Layout** — animefixer_compressform_designer_compressform, animefixer_mp4fixerform_designer_mp4fixerform, animefixer_mkvrokufrom_designer_mkvrokufrom [INFERRED 0.95]
- **Shared Verbose Logging Checkbox + Dark Log TextBox Pattern** — concept_verbose_logging_toggle, concept_splitcontainer_log_pattern, animefixer_compressform_designer_compressform [INFERRED 0.85]
- **Shared FFmpeg Folder Path Browser Pattern** — animefixer_mp4fixerform_designer_mp4fixerform, animefixer_mkvrokufrom_designer_mkvrokufrom, concept_media_folder_browser_pattern [INFERRED 0.90]
- **FFmpeg MKV Probe + Subtitle Extraction Pipeline** — services_ffmpegservice_probemkv, services_ffmpegservice_extractsubtitle, services_ffmpegservice_cleansrtfile [INFERRED 0.95]
- **Large File Compression Workflow** — services_compressscannerservice_compressscannerservice, services_handbrakeservice_handbrakeservice, services_settingsservice_settingsservice [INFERRED 0.95]
- **MKV Roku Fix Workflow (scan, re-encode, extract subs)** — services_mkvscannerservice_mkvscannerservice, services_ffmpegservice_reencodetoh264, subtitleextractiondialog_subtitleextractiondialog [INFERRED 0.95]

## Communities (26 total, 7 thin omitted)

### Community 0 - "CompressForm Designer"
Cohesion: 0.05
Nodes (42): AnimeFixer, CompressForm, Button, CheckBox, IContainer, Label, Panel, ProgressBar (+34 more)

### Community 1 - "MkvRokuForm Logic"
Cohesion: 0.08
Nodes (18): AnimeFixer, bool, CancellationTokenSource, EventArgs, FFmpegService, List, MkvMediaFile, SettingsService (+10 more)

### Community 2 - "CompressForm Logic"
Cohesion: 0.07
Nodes (19): AnimeFixer, CompressForm, CancellationTokenSource, CompressFile, EventArgs, List, SettingsService, Color (+11 more)

### Community 3 - "MP4FixerForm Logic"
Cohesion: 0.10
Nodes (11): AnimeFixer, bool, CancellationTokenSource, EventArgs, FFmpegService, List, MediaFile, SettingsService (+3 more)

### Community 4 - "Compress & HandBrake Service"
Cohesion: 0.06
Nodes (24): Action, CompressFile, List, string, Action, CancellationToken, CompressFile, Regex (+16 more)

### Community 5 - "FFmpeg Media Service"
Cohesion: 0.14
Nodes (12): Action, CancellationToken, List, MediaFile, MkvMediaFile, Regex, string, ProbeResult (+4 more)

### Community 6 - "MkvRokuForm Designer"
Cohesion: 0.09
Nodes (20): AnimeFixer, Button, CheckBox, ColumnHeader, IContainer, Label, ListView, Panel (+12 more)

### Community 7 - "MKV Scanner & Roku Encoding"
Cohesion: 0.14
Nodes (13): Action, List, MkvMediaFile, Cancellable Long-Running Encoding Pattern, Roku TV Media Compatibility, MKV Roku Compatibility Implementation Plan, FFmpegService.ReencodeToH264, Roku MP4 faststart rationale (+5 more)

### Community 8 - "Media Models & HEVC Detection"
Cohesion: 0.14
Nodes (12): GPAC Invalid Stream Detection, HEVC 10-bit Detection and Re-encode, NVIDIA NVENC Hardware Encoding, Roku TV Compatibility Fixing, AnimeFixer.Models, MediaFile, ProbeResult, StreamInfo (+4 more)

### Community 9 - "Subtitle Extraction Dialog"
Cohesion: 0.18
Nodes (8): AnimeFixer, EventArgs, List, string, SubtitleExtractionDialog, SubtitleExtractionDialog.BtnOK_Click, SubtitleExtractionDialog (Designer), SubtitleExtractionDialog.PopulateList

### Community 10 - "Main Form & Program Entry"
Cohesion: 0.18
Nodes (7): AnimeFixer, EventArgs, Form1, AnimeFixer, Program, Form, STAThread

### Community 11 - "FFmpeg Parsing Pipeline"
Cohesion: 0.21
Nodes (13): Subtitle Extraction Pipeline (ASS to SRT), FFmpegService.CleanSrtFile, FFmpegService.ExtractFFmpegError, FFmpegService.ExtractSubtitle, FFmpegService.GetTotalFrames, FFmpegService.ParseProbeOutput, FFmpegService.SplitStreamBlocks, FFmpegService.ParseStringField (+5 more)

### Community 12 - "Subtitle Dialog Designer"
Cohesion: 0.18
Nodes (8): AnimeFixer, Button, ColumnHeader, IContainer, Label, ListView, Panel, SubtitleExtractionDialog

### Community 13 - "Form1 Designer"
Cohesion: 0.22
Nodes (6): AnimeFixer, Button, IContainer, Label, Panel, Form1

### Community 14 - "Scanner Service"
Cohesion: 0.22
Nodes (6): Action, List, MediaFile, string, AnimeFixer.Services, ScannerService

### Community 15 - "Resources & Culture Settings"
Cohesion: 0.40
Nodes (4): CultureInfo, AnimeFixer.Properties, Resources, ResourceManager

### Community 16 - "App Settings"
Cohesion: 0.67
Nodes (3): ApplicationSettingsBase, AnimeFixer.Properties, Settings

## Knowledge Gaps
- **154 isolated node(s):** `AnimeFixer`, `IContainer`, `Panel`, `Label`, `TextBox` (+149 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SubtitleExtractionDialog` connect `Subtitle Extraction Dialog` to `Main Form & Program Entry`, `FFmpeg Parsing Pipeline`, `MKV Scanner & Roku Encoding`?**
  _High betweenness centrality (0.274) - this node is a cross-community bridge._
- **Why does `MKV Roku Compatibility Implementation Plan` connect `MKV Scanner & Roku Encoding` to `Subtitle Extraction Dialog`, `FFmpeg Media Service`?**
  _High betweenness centrality (0.226) - this node is a cross-community bridge._
- **Why does `MkvRokuForm` connect `MkvRokuForm Logic` to `Media Models & HEVC Detection`, `MP4FixerForm Logic`, `Main Form & Program Entry`, `CompressForm Logic`?**
  _High betweenness centrality (0.181) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `MkvRokuForm` (e.g. with `MP4FixerForm` and `Verbose/Simple Logging Mode Toggle`) actually correct?**
  _`MkvRokuForm` has 2 INFERRED edges - model-reasoned connections that need verification._
- **Are the 3 inferred relationships involving `CompressForm` (e.g. with `MkvRokuForm (Designer)` and `MP4FixerForm`) actually correct?**
  _`CompressForm` has 3 INFERRED edges - model-reasoned connections that need verification._
- **What connects `AnimeFixer`, `IContainer`, `Panel` to the rest of the system?**
  _160 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `CompressForm Designer` be split into smaller, more focused modules?**
  _Cohesion score 0.047872340425531915 - nodes in this community are weakly interconnected._