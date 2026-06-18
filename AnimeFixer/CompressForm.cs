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
        private readonly SettingsService        _settings  = new SettingsService();
        private readonly CompressScannerService _scanner   = new CompressScannerService();
        private readonly HandBrakeService       _handBrake = new HandBrakeService();
        private          List<CompressFile>     _files     = new List<CompressFile>();
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
            _btnBrowseHandBrake.Click  += BtnBrowseHandBrake_Click;
            _btnTestHandBrake.Click    += BtnTestHandBrake_Click;
            _txtHandBrakePath.Leave    += (s, e) => SaveSettings();
            _btnBrowseFolder.Click     += BtnBrowseFolder_Click;
            _txtFolder.Leave           += (s, e) => SaveSettings();
            _btnScan.Click             += BtnScan_Click;
            _btnCompressSelected.Click += BtnCompressSelected_Click;
            _btnCheckRecommended.Click += BtnCheckRecommended_Click;
            _btnUncheckAll.Click       += BtnUncheckAll_Click;
            _btnCancel.Click           += BtnCancel_Click;
        }

        // ── Browse / Test HandBrakeCLI ────────────────────────────────────────

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
            _btnTestHandBrake.Enabled     = false;
            _lblHandBrakeStatus.Text      = "Verificando...";
            _lblHandBrakeStatus.ForeColor = Color.Gray;

            string path    = _txtHandBrakePath.Text.Trim();
            string version = null;
            string error   = null;
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

        // ── Browse folder ─────────────────────────────────────────────────────

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

        // ── Scan ──────────────────────────────────────────────────────────────

        private void BtnScan_Click(object sender, EventArgs e)
        {
            string folder = _txtFolder.Text.Trim();
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show("Selecciona una carpeta valida.", "AnimeFixer",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateHandBrake()) return;

            long thresholdBytes = ParseThresholdBytes();

            _grid.Rows.Clear();
            _files.Clear();
            _btnScan.Enabled             = false;
            _btnCompressSelected.Enabled = false;
            _btnCheckRecommended.Enabled = false;
            _btnUncheckAll.Enabled       = false;
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
                        _files.Count, CountRecommended()));
                    _stripLabel.Text = _files.Count + " archivos encontrados.";
                }));
            });
        }

        private void PopulateGrid()
        {
            _grid.Rows.Clear();
            foreach (var cf in _files)
            {
                int idx = _grid.Rows.Add(
                    cf.IsSelected,
                    cf.FileName,
                    cf.FilePath,
                    ScannerService.FormatSize(cf.FileSizeBytes),
                    cf.IsRecommended ? "Comprimir" : "",
                    "Pendiente");
                _grid.Rows[idx].Tag = cf;
            }
        }

        // ── Check / Uncheck helpers ───────────────────────────────────────────

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

        // ── Compress ──────────────────────────────────────────────────────────

        private async void BtnCompressSelected_Click(object sender, EventArgs e)
        {
            if (!ValidateHandBrake()) return;

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
                    string.Format("{0} archivo(s) ya tienen version comprimida. Sobreescribir?",
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

                    var cf        = toProcess[i];
                    int fileNum   = i + 1;
                    int fileTotal = toProcess.Count;

                    SetFileStatus(cf, CompressStatus.Processing, "Procesando...", Color.Blue);
                    SetStatus(string.Format("Procesando {0} de {1}: {2}",
                        fileNum, fileTotal, cf.FileName));
                    SetProgress(0);

                    try
                    {
                        string errMsg;
                        bool ok = _handBrake.Compress(
                            cf,
                            line => AppendLog(line),
                            pct  => SetProgress((int)pct),
                            _cts.Token,
                            out errMsg);

                        if (ok)
                        {
                            long   saved   = cf.FileSizeBytes - cf.OutputSizeBytes;
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
                        SetFileStatus(cf, CompressStatus.Skipped, "Cancelado", Color.Gray);
                        break;
                    }
                }
            });

            SetBusy(false);
            SetProgress(0);
            SetStatus("Completado.");
            _stripLabel.Text = "Compresion finalizada.";

            if (_cts != null) { _cts.Dispose(); _cts = null; }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool ValidateHandBrake()
        {
            string path = _txtHandBrakePath.Text.Trim();
            if (!string.IsNullOrWhiteSpace(path) && !File.Exists(path))
            {
                _lblHandBrakeStatus.Text =
                    "HandBrakeCLI.exe no encontrado. Configure la ruta arriba.";
                _lblHandBrakeStatus.ForeColor = Color.FromArgb(180, 0, 0);
                _btnScan.Enabled             = false;
                _btnCompressSelected.Enabled = false;
                return false;
            }
            return true;
        }

        private long ParseThresholdBytes()
        {
            const long GB = 1024L * 1024L * 1024L;
            long gb;
            if (long.TryParse(_txtThreshold.Text.Trim(), out gb) && gb > 0)
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
                    cf.IsSelected = cell.Value is bool && (bool)cell.Value;
            }
        }

        // ── Thread-safe UI updaters ───────────────────────────────────────────

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
            CompressFile  cf,
            CompressStatus status,
            string         displayText,
            Color          color)
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
            if (!_chkVerboseLogging.Checked && line.StartsWith("Encoding:")) return;
            _txtLog.AppendText(line + Environment.NewLine);
            _txtLog.ScrollToCaret();
        }
    }
}
