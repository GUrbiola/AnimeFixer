using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AnimeFixer.Models;
using AnimeFixer.Services;

namespace AnimeFixer
{
    public partial class MP4FixerForm : Form
    {
        private readonly SettingsService _settings = new SettingsService();
        private FFmpegService _ffmpeg;
        private CancellationTokenSource _cts;
        private List<MediaFile> _files = new List<MediaFile>();
        private bool _verboseLogging = false; // Set to false for simple mode by default

        public MP4FixerForm()
        {
            InitializeComponent();
            ApplySettings();
            TryAutoDetectFFmpeg();
            InitializeLoggingControls();

            // Wire up event handlers
            _btnBrowseFFmpeg.Click += BtnBrowseFFmpeg_Click;
            _btnBrowseFolder.Click += BtnBrowseFolder_Click;
            _btnScan.Click += BtnScan_Click;
            _btnFixAll.Click += BtnFixAll_Click;
            _btnCancel.Click += BtnCancel_Click;
            _listView.DoubleClick += ListView_DoubleClick;
            this.FormClosing += (s, e) => SaveSettings();
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.V)
                {
                    _chkVerboseLogging.Checked = !_chkVerboseLogging.Checked;
                    e.Handled = true;
                }
            };
        }

        private void InitializeLoggingControls()
        {
            // Wire up checkbox event
            _chkVerboseLogging.CheckedChanged += (s, e) =>
            {
                _verboseLogging = _chkVerboseLogging.Checked;
                _stripLabel.Text = _verboseLogging ? "Modo Verbose ACTIVADO (Ctrl+V)" : "Modo Simple ACTIVADO (Ctrl+V)";
            };

            // Add context menu to log textbox
            var logContextMenu = new ContextMenuStrip();
            logContextMenu.Items.Add("Modo Simple", null, (s, e) =>
            {
                _chkVerboseLogging.Checked = false;
            });
            logContextMenu.Items.Add("Modo Verbose", null, (s, e) =>
            {
                _chkVerboseLogging.Checked = true;
            });
            logContextMenu.Items.Add(new ToolStripSeparator());
            logContextMenu.Items.Add("Limpiar Log", null, (s, e) =>
            {
                _txtLog.Clear();
            });
            _txtLog.ContextMenuStrip = logContextMenu;
        }

        private void ApplySettings()
        {
            _txtFFmpegFolder.Text = _settings.FFmpegFolder;
            _txtFolder.Text = _settings.LastFolder;
        }

        private void SaveSettings()
        {
            _settings.FFmpegFolder = _txtFFmpegFolder.Text.Trim();
            _settings.LastFolder = _txtFolder.Text.Trim();
            _settings.Save();
        }

        private void TryAutoDetectFFmpeg()
        {
            if (!string.IsNullOrEmpty(_settings.FFmpegFolder) &&
                File.Exists(_settings.FFprobePath) &&
                File.Exists(_settings.FFmpegPath))
            {
                return;
            }

            var detected = SettingsService.TryAutoDetectFFmpegFolder();
            if (!string.IsNullOrEmpty(detected))
            {
                _txtFFmpegFolder.Text = detected;
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
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Selecciona la carpeta que contiene ffmpeg.exe y ffprobe.exe",
                SelectedPath = _txtFFmpegFolder.Text
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _txtFFmpegFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Selecciona la carpeta raíz de medios a escanear",
                SelectedPath = _txtFolder.Text
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _txtFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            var folder = _txtFolder.Text.Trim();

            if (string.IsNullOrEmpty(folder))
            {
                MessageBox.Show("Por favor, selecciona una carpeta de medios.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(folder))
            {
                MessageBox.Show("La carpeta especificada no existe.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!BuildFFmpegService())
                return;

            SetBusy(true);
            ClearList();

            Log("── Iniciando escaneo ──────────────────────────────────────────");
            Log(folder);

            _cts = new CancellationTokenSource();

            try
            {
                await Task.Run(() =>
                {
                    SetStatus("Buscando archivos...");
                    var scanner = new ScannerService();
                    _files = scanner.Scan(folder, filePath =>
                    {
                        LogVerbose($"  ✓ Encontrado: {Path.GetFileName(filePath)} ({ScannerService.FormatSize(new FileInfo(filePath).Length)})");
                    });

                    if (_files.Count == 0)
                    {
                        SetStatus("No se encontraron archivos de video.");
                        SetBusy(false);
                        return;
                    }

                    LogSimple($"Se encontraron {_files.Count} archivo(s)");
                    LogVerbose("");
                    LogVerbose("Iniciando análisis de streams...");
                    PopulateList();

                    SetProgress(0, _files.Count);

                    int analyzing = 0;
                    int filesWithIssues = 0;
                    foreach (var mf in _files)
                    {
                        if (_cts.Token.IsCancellationRequested)
                            break;

                        analyzing++;
                        UpdateFileStatus(mf, FileStatus.Analyzing);
                        SetStatus($"Analizando {analyzing}/{_files.Count}: {mf.FileName}");

                        try
                        {
                            mf.ProbeResult = _ffmpeg.Probe(mf.FilePath, debugLine => LogVerbose(debugLine));

                            LogVerbose($"  → {mf.FileName}");

                            if (mf.ProbeResult.Streams.Count == 0)
                            {
                                LogVerbose($"    ⚠ No streams found!");
                                mf.Status = FileStatus.Failed;
                                mf.ErrorMessage = "No streams detected";
                            }
                            else
                            {
                                LogVerbose($"    Streams encontrados: {mf.ProbeResult.Streams.Count}");
                                foreach (var stream in mf.ProbeResult.Streams)
                                {
                                    string streamInfo = $"      #{stream.Index}: {stream.CodecType}";
                                    if (!string.IsNullOrEmpty(stream.CodecName))
                                        streamInfo += $" codec={stream.CodecName}";
                                    if (!string.IsNullOrEmpty(stream.HandlerName))
                                        streamInfo += $" handler={stream.HandlerName}";

                                    if (stream.IsInvalid)
                                        streamInfo += " ⚠ INVÁLIDO";

                                    LogVerbose(streamInfo);
                                }

                                if (mf.ProbeResult.HasInvalidStreams)
                                {
                                    mf.Status = FileStatus.NeedsFixing;
                                    filesWithIssues++;
                                    var invalidList = new StringBuilder();
                                    foreach (var stream in mf.ProbeResult.InvalidStreams)
                                    {
                                        if (invalidList.Length > 0) invalidList.Append(", ");
                                        invalidList.Append($"#{stream.Index} {stream.CodecName}");
                                        if (!string.IsNullOrEmpty(stream.HandlerName))
                                            invalidList.Append($" ({stream.HandlerName})");
                                    }
                                    LogSimple($"{mf.FileName} - ⚠ Con problemas");
                                    LogVerbose($"    ⚠ NECESITA CORRECCIÓN: {invalidList.ToString()}");
                                }
                                else
                                {
                                    mf.Status = FileStatus.Clean;
                                    LogSimple($"{mf.FileName} - ✓ Sin problemas");
                                    LogVerbose($"    ✓ Sin problemas");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            mf.Status = FileStatus.Failed;
                            mf.ErrorMessage = ex.Message;
                            LogSimple($"{mf.FileName} - ✘ Error");
                            LogVerbose($"  ✘ {mf.FileName}: Error - {ex.Message}");
                        }

                        UpdateFileStatus(mf, mf.Status);
                        SetProgress(analyzing, _files.Count);
                    }

                    LogSimple("");
                    string summary = $"Escaneo completo: {_files.Count} archivo(s) — {filesWithIssues} con problemas.";
                    SetStatus(summary);
                    LogSimple(summary);
                    _stripLabel.Text = summary;
                    _btnFixAll.Enabled = filesWithIssues > 0;
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

        private async void BtnFixAll_Click(object sender, EventArgs e)
        {
            var toFix = new List<MediaFile>();
            foreach (var f in _files)
            {
                if (f.Status == FileStatus.NeedsFixing)
                    toFix.Add(f);
            }

            if (toFix.Count == 0)
            {
                MessageBox.Show("No hay archivos que corregir.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Se corregirán {toFix.Count} archivo(s).\n\n" +
                "El archivo original será reemplazado por la versión corregida.\n" +
                "Si la corrección falla, el original se conserva intacto.\n\n" +
                "¿Continuar?",
                "Confirmar corrección",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            SetBusy(true);
            _btnFixAll.Enabled = false;

            _cts = new CancellationTokenSource();

            try
            {
                await Task.Run(() =>
                {
                    LogVerbose("── Iniciando corrección ───────────────────────────────────────");
                    LogSimple("");

                    int fixed_ = 0;
                    int failed = 0;

                    for (int i = 0; i < toFix.Count; i++)
                    {
                        if (_cts.Token.IsCancellationRequested)
                            break;

                        var mf = toFix[i];
                        UpdateFileStatus(mf, FileStatus.Fixing);
                        SetStatus($"Corrigiendo {i + 1}/{toFix.Count}: {mf.FileName}");
                        LogSimple($"Corrigiendo {mf.FileName}...");

                        if (_ffmpeg.Remux(mf, line => LogVerbose("  " + line), out string err))
                        {
                            mf.Status = FileStatus.Fixed;
                            fixed_++;
                            LogSimple($"{mf.FileName} - ✔ Corregido!");
                            LogVerbose("  ✔ Corregido exitosamente.");
                        }
                        else
                        {
                            mf.Status = FileStatus.Failed;
                            mf.ErrorMessage = err;
                            failed++;
                            LogSimple($"{mf.FileName} - ✘ Falló");
                            LogVerbose($"  ✘ Falló: {err}");
                            LogVerbose("  El archivo original se ha conservado intacto.");
                        }

                        UpdateFileStatus(mf, mf.Status);
                        SetProgress(i + 1, toFix.Count);
                    }

                    LogSimple("");
                    string summary = $"Corrección completa: ✔ {fixed_} corregido(s)  ✘ {failed} fallido(s).";
                    SetStatus(summary);
                    LogSimple(summary);
                    _stripLabel.Text = summary;

                    if (failed > 0)
                    {
                        MessageBox.Show(
                            $"Se completó la corrección con {failed} error(es).\n\n" +
                            "Por favor, revisa el registro para más detalles.",
                            "Corrección completada con errores",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log("Corrección cancelada.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            _btnCancel.Enabled = false;
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            int index = (int)_listView.SelectedItems[0].Tag;
            var file = _files[index];

            var sb = new StringBuilder();
            sb.AppendLine($"Ruta: {file.FilePath}");
            sb.AppendLine($"Tamaño: {ScannerService.FormatSize(file.FileSizeBytes)}");
            sb.AppendLine($"Estado: {file.StatusDisplay}");
            sb.AppendLine();

            if (file.ProbeResult != null)
            {
                sb.AppendLine("Streams:");
                foreach (var stream in file.ProbeResult.Streams)
                {
                    sb.Append($"  #{stream.Index} [{stream.CodecType}] {stream.CodecName}");
                    if (!string.IsNullOrEmpty(stream.HandlerName))
                        sb.Append($" / {stream.HandlerName}");
                    if (stream.IsInvalid)
                        sb.Append(" ← INVÁLIDO");
                    sb.AppendLine();
                }
            }

            if (!string.IsNullOrEmpty(file.ErrorMessage))
            {
                sb.AppendLine();
                sb.AppendLine($"Error: {file.ErrorMessage}");
            }

            MessageBox.Show(sb.ToString(), "Detalles del archivo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PopulateList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(PopulateList));
                return;
            }

            _listView.BeginUpdate();
            _listView.Items.Clear();

            for (int i = 0; i < _files.Count; i++)
            {
                var file = _files[i];
                var item = new ListViewItem(file.FileName);
                item.SubItems.Add(ScannerService.FormatSize(file.FileSizeBytes));
                item.SubItems.Add(file.StatusDisplay);
                item.SubItems.Add(string.Empty);
                item.Tag = i;
                _listView.Items.Add(item);
            }

            _listView.EndUpdate();
        }

        private void UpdateFileStatus(MediaFile mf, FileStatus status)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateFileStatus(mf, status)));
                return;
            }

            mf.Status = status;

            foreach (ListViewItem item in _listView.Items)
            {
                if ((int)item.Tag == _files.IndexOf(mf))
                {
                    item.SubItems[2].Text = mf.StatusDisplay;

                    if (mf.ProbeResult != null && mf.ProbeResult.HasInvalidStreams)
                    {
                        var invalid = new StringBuilder();
                        foreach (var stream in mf.ProbeResult.InvalidStreams)
                        {
                            if (invalid.Length > 0) invalid.Append(", ");
                            invalid.Append($"#{stream.Index} {stream.CodecName}");
                        }
                        item.SubItems[3].Text = invalid.ToString();
                    }
                    else if (mf.ProbeResult != null)
                    {
                        item.SubItems[3].Text = "—";
                    }

                    switch (status)
                    {
                        case FileStatus.NeedsFixing:
                            item.ForeColor = Color.FromArgb(180, 80, 0);
                            break;
                        case FileStatus.Fixed:
                            item.ForeColor = Color.FromArgb(30, 130, 60);
                            break;
                        case FileStatus.Failed:
                            item.ForeColor = Color.FromArgb(180, 0, 0);
                            break;
                        case FileStatus.Clean:
                            item.ForeColor = Color.FromArgb(80, 80, 80);
                            break;
                        default:
                            item.ForeColor = _listView.ForeColor;
                            break;
                    }

                    break;
                }
            }
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetBusy(busy)));
                return;
            }

            _btnScan.Enabled = !busy;
            _btnBrowseFolder.Enabled = !busy;
            _btnBrowseFFmpeg.Enabled = !busy;
            _btnCancel.Enabled = busy;
            _progressBar.Visible = busy;

            if (!busy)
                _progressBar.Value = 0;
        }

        private void SetProgress(int value, int max)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetProgress(value, max)));
                return;
            }

            _progressBar.Maximum = max;
            _progressBar.Value = Math.Min(value, max);
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetStatus(text)));
                return;
            }

            _lblStatus.Text = text;
        }

        private void Log(string line)
        {
            LogSimple(line);
        }

        private void LogSimple(string line)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogSimple(line)));
                return;
            }

            _txtLog.AppendText($"{line}{Environment.NewLine}");
        }

        private void LogVerbose(string line)
        {
            if (!_verboseLogging)
                return;

            if (InvokeRequired)
            {
                Invoke(new Action(() => LogVerbose(line)));
                return;
            }

            _txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
        }

        private void ClearList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ClearList));
                return;
            }

            _listView.Items.Clear();
            _txtLog.Clear();
            _btnFixAll.Enabled = false;
            _stripLabel.Text = "Listo.";
        }

        private void MP4FixerForm_Load(object sender, EventArgs e)
        {

        }

        private void _progressBar_Click(object sender, EventArgs e)
        {

        }

        private void _btnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
