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
        private readonly SettingsService _settings = new SettingsService();
        private FFmpegService            _ffmpeg;
        private CancellationTokenSource  _cts;
        private List<MkvMediaFile>       _files        = new List<MkvMediaFile>();
        private bool                     _verboseLogging;
        private DateTime                 _elapsedStart;

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
            _tmrElapsed.Tick         += TmrElapsed_Tick;

            _chkVerboseLogging.CheckedChanged += (s, e) =>
            {
                _verboseLogging  = _chkVerboseLogging.Checked;
                _stripLabel.Text = _verboseLogging ? "Modo Verbose ACTIVADO" : "Modo Simple ACTIVADO";
            };

            _chkNvenc.CheckedChanged += (s, e) =>
            {
                bool nvenc = _chkNvenc.Checked;
                _lblPreset.Enabled  = !nvenc;
                _cmbPreset.Enabled  = !nvenc;
            };

            _toolTip.SetToolTip(_chkNvenc,
                "h264_nvenc — Codificación por hardware NVIDIA NVENC\r\n" +
                "Usa el chip de video dedicado de la GPU en lugar del CPU.\r\n" +
                "\r\n" +
                "Ventajas:\r\n" +
                "  • 5-15x mas rapido que libx264 (minutos en vez de horas)\r\n" +
                "  • No carga la CPU — puedes seguir usando el PC con normalidad\r\n" +
                "  • Ideal para archivos grandes (peliculas, temporadas completas)\r\n" +
                "\r\n" +
                "Desventajas:\r\n" +
                "  • Calidad ligeramente inferior a libx264 al mismo tamano de archivo\r\n" +
                "    (imperceptible en anime y contenido de streaming)\r\n" +
                "  • Requiere drivers NVIDIA actualizados y ffmpeg con soporte NVENC\r\n" +
                "  • El preset CPU queda sin efecto (NVENC usa su propio preset interno)\r\n" +
                "\r\n" +
                "Compatible con: GTX 600 en adelante (Pascal GTX 1080 = excelente soporte)");

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

        private string GetSelectedPreset()
        {
            switch (_cmbPreset.SelectedIndex)
            {
                case 1:  return "fast";
                case 2:  return "medium";
                case 3:  return "slow";
                default: return "veryfast";
            }
        }

        private void TmrElapsed_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _elapsedStart;
            _lblElapsed.Text = string.Format("Transcurrido: {0:hh\\:mm\\:ss}", elapsed);
        }

        private void SetEncodeProgress(int frame, int totalFrames, double fps, double speed)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetEncodeProgress(frame, totalFrames, fps, speed)));
                return;
            }

            string fpsStr   = fps   > 0 ? string.Format(" | {0:F0} fps", fps)  : string.Empty;
            string speedStr = speed > 0 ? string.Format(" | {0:F2}x", speed)   : string.Empty;

            if (totalFrames > 0)
            {
                int pct = (int)Math.Min(100, frame * 100.0 / totalFrames);
                _progressBar.Maximum = 100;
                _progressBar.Value   = pct;

                string etaStr = string.Empty;
                if (fps > 0)
                {
                    double etaSec = (totalFrames - frame) / fps;
                    etaStr = string.Format(" | ETA {0}",
                        TimeSpan.FromSeconds(etaSec).ToString(@"hh\:mm\:ss"));
                }

                _lblStatus.Text = string.Format("Codificando: {0}%{1}{2}{3}",
                    pct, fpsStr, speedStr, etaStr);
            }
            else
            {
                // No total frames available — still show live stats
                _lblStatus.Text = string.Format("Codificando: frame {0}{1}{2}",
                    frame, fpsStr, speedStr);
            }
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
                        LogVerbose(string.Format("  ✓ Encontrado: {0} ({1})",
                            Path.GetFileName(fp),
                            ScannerService.FormatSize(new FileInfo(fp).Length))));

                    if (_files.Count == 0)
                    {
                        SetStatus("No se encontraron archivos MKV.");
                        SetBusy(false);
                        return;
                    }

                    LogSimple(string.Format("Se encontraron {0} archivo(s) MKV", _files.Count));
                    PopulateList();
                    SetProgress(0, _files.Count);

                    int analyzed  = 0;
                    int hevcCount = 0;

                    foreach (var mf in _files)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        analyzed++;
                        UpdateFileStatus(mf, MkvFileStatus.Analyzing);
                        SetStatus(string.Format("Analizando {0}/{1}: {2}", analyzed, _files.Count, mf.FileName));

                        try
                        {
                            var probe       = _ffmpeg.ProbeMkv(mf.FilePath, line => LogVerbose(line));
                            mf.IsHevc10bit  = probe.IsHevc10bit;
                            mf.SubtitleTracks = probe.SubtitleTracks;

                            if (probe.IsHevc10bit)
                            {
                                mf.Status = MkvFileStatus.NeedsFixing;
                                mf.Action = MkvAction.Both;
                                hevcCount++;
                                LogSimple(string.Format("{0} - ⚠ HEVC 10-bit, {1} sub(s)",
                                    mf.FileName, probe.SubtitleTracks.Count));
                            }
                            else
                            {
                                mf.Status = MkvFileStatus.Clean;
                                if (probe.SubtitleTracks.Count > 0)
                                    mf.Action = MkvAction.ExtractSubtitles;
                                LogSimple(string.Format("{0} - ✔ Sin HEVC 10-bit, {1} sub(s)",
                                    mf.FileName, probe.SubtitleTracks.Count));
                            }
                        }
                        catch (Exception ex)
                        {
                            mf.Status       = MkvFileStatus.Failed;
                            mf.ErrorMessage = ex.Message;
                            LogSimple(string.Format("{0} - ✘ Error: {1}", mf.FileName, ex.Message));
                        }

                        UpdateFileStatus(mf, mf.Status);
                        SetProgress(analyzed, _files.Count);
                    }

                    LogSimple(string.Empty);
                    string summary = string.Format(
                        "Escaneo MKV completo: {0} archivo(s) — {1} con HEVC 10-bit.",
                        _files.Count, hevcCount);
                    SetStatus(summary);
                    LogSimple(summary);
                    _stripLabel.Text           = summary;
                    int anySubsCount = 0;
                    foreach (var f in _files)
                        if (f.SubtitleTracks.Count > 0) anySubsCount++;

                    _btnReencodeAll.Enabled    = hevcCount > 0;
                    _btnExtractSubsAll.Enabled = anySubsCount > 0;
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
                if (f.Action == MkvAction.None)
                    continue;
                if (action == MkvAction.ExtractSubtitles && f.SubtitleTracks.Count == 0)
                    continue;
                if (action == MkvAction.ReencodeVideo && !f.IsHevc10bit)
                    continue;
                if (action == MkvAction.ExtractSubtitles &&
                    f.Action != MkvAction.ExtractSubtitles && f.Action != MkvAction.Both)
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
                case MkvAction.ReencodeVideo:    actionText = "re-encodear";                    break;
                case MkvAction.ExtractSubtitles: actionText = "extraer subtítulos de";          break;
                default:                          actionText = "procesar (re-encode + subs) de"; break;
            }

            var confirm = MessageBox.Show(
                string.Format("Se van a {0} {1} archivo(s).\n\n¿Continuar?", actionText, toProcess.Count),
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            // Capture encoder settings on UI thread before entering background task
            string selectedPreset = GetSelectedPreset();
            bool   useNvenc       = _chkNvenc.Checked;

            SetBusy(true);
            _btnReencodeAll.Enabled    = false;
            _btnExtractSubsAll.Enabled = false;
            _btnBothAll.Enabled        = false;
            _cts = new CancellationTokenSource();

            _elapsedStart    = DateTime.Now;
            _lblElapsed.Text = "Transcurrido: 00:00:00";
            _tmrElapsed.Start();

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
                        SetStatus(string.Format("Procesando {0}/{1}: {2}", i + 1, toProcess.Count, mf.FileName));
                        LogSimple(string.Format("Procesando {0}...", mf.FileName));

                        bool   ok  = true;
                        string err = null;

                        if ((action == MkvAction.ReencodeVideo || action == MkvAction.Both)
                            && mf.IsHevc10bit)
                        {
                            int totalFrames = _ffmpeg.GetTotalFrames(mf.FilePath);
                            if (totalFrames > 0)
                                LogVerbose(string.Format("  Total frames: {0}", totalFrames));

                            ok = _ffmpeg.ReencodeToH264(mf, selectedPreset, totalFrames, useNvenc,
                                line => LogVerbose("  " + line),
                                (frame, fps, speed) => SetEncodeProgress(frame, totalFrames, fps, speed),
                                _cts.Token, out err);

                            if (ok)
                                LogSimple(string.Format("  ✔ Re-encode completado ({0}) → {1}.roku.mp4",
                                    useNvenc ? "NVENC" : selectedPreset,
                                    Path.GetFileNameWithoutExtension(mf.FileName)));
                            else
                                LogSimple(string.Format("  ✘ Re-encode falló: {0}", err));
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
                            LogSimple(string.Format("{0} - ✔ Completado", mf.FileName));
                        }
                        else
                        {
                            mf.Status       = MkvFileStatus.Failed;
                            mf.ErrorMessage = err;
                            failed++;
                            LogSimple(string.Format("{0} - ✘ Error: {1}", mf.FileName, err));
                        }

                        UpdateFileStatus(mf, mf.Status);
                        SetProgress(i + 1, toProcess.Count);
                    }

                    LogSimple(string.Empty);
                    string summary = string.Format("Proceso completo: ✔ {0}  ✘ {1}.", done, failed);
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
                _tmrElapsed.Stop();
                _lblElapsed.Text = string.Empty;
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

                var outputPath = Path.Combine(dir, string.Format("{0}.{1}.srt", nameNoExt, track.SuggestedFileSuffix));
                LogVerbose(string.Format("  Extrayendo sub#{0} ({1}) → {2}",
                    track.Index, track.DisplayTitle, Path.GetFileName(outputPath)));

                if (!_ffmpeg.ExtractSubtitle(mf.FilePath, track, outputPath,
                    line => LogVerbose("    " + line), out string err))
                {
                    errorMessage = string.Format("Sub#{0}: {1}", track.Index, err);
                    LogSimple(string.Format("  ✘ Falló sub#{0}: {1}", track.Index, err));
                }
                else
                {
                    LogSimple(string.Format("  ✔ Extraído: {0}", Path.GetFileName(outputPath)));
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
                    string.Format("Archivo: {0}\nTamaño: {1}\nHEVC 10-bit: {2}\nSubtítulos: ninguno",
                        mf.FilePath,
                        ScannerService.FormatSize(mf.FileSizeBytes),
                        mf.IsHevc10bit ? "Sí" : "No"),
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
                        ? string.Format("{0} pista(s)", mf.SubtitleTracks.Count)
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
            _txtLog.AppendText(string.Format("{0}{1}", line, Environment.NewLine));
        }

        private void LogVerbose(string line)
        {
            if (!_verboseLogging) return;
            if (InvokeRequired) { Invoke(new Action(() => LogVerbose(line))); return; }
            _txtLog.AppendText(string.Format("[{0:HH:mm:ss}] {1}{2}",
                DateTime.Now, line, Environment.NewLine));
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
