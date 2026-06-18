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
            this.components             = new System.ComponentModel.Container();
            this._topPanel              = new System.Windows.Forms.Panel();
            this._lblFFmpegPath         = new System.Windows.Forms.Label();
            this._txtFFmpegFolder       = new System.Windows.Forms.TextBox();
            this._btnBrowseFFmpeg       = new System.Windows.Forms.Button();
            this._lblFolder             = new System.Windows.Forms.Label();
            this._txtFolder             = new System.Windows.Forms.TextBox();
            this._btnBrowseFolder       = new System.Windows.Forms.Button();
            this._lblPreset             = new System.Windows.Forms.Label();
            this._cmbPreset             = new System.Windows.Forms.ComboBox();
            this._btnScan               = new System.Windows.Forms.Button();
            this._btnReencodeAll        = new System.Windows.Forms.Button();
            this._btnExtractSubsAll     = new System.Windows.Forms.Button();
            this._btnBothAll            = new System.Windows.Forms.Button();
            this._btnCancel             = new System.Windows.Forms.Button();
            this._progressBar           = new System.Windows.Forms.ProgressBar();
            this._lblStatus             = new System.Windows.Forms.Label();
            this._lblElapsed            = new System.Windows.Forms.Label();
            this._chkVerboseLogging     = new System.Windows.Forms.CheckBox();
            this._tmrElapsed            = new System.Windows.Forms.Timer(this.components);
            this._chkNvenc              = new System.Windows.Forms.CheckBox();
            this._toolTip               = new System.Windows.Forms.ToolTip(this.components);
            this._splitContainer        = new System.Windows.Forms.SplitContainer();
            this._listView              = new System.Windows.Forms.ListView();
            this._colFileName           = new System.Windows.Forms.ColumnHeader();
            this._colSize               = new System.Windows.Forms.ColumnHeader();
            this._colHevc               = new System.Windows.Forms.ColumnHeader();
            this._colSubs               = new System.Windows.Forms.ColumnHeader();
            this._colStatus             = new System.Windows.Forms.ColumnHeader();
            this._colAction             = new System.Windows.Forms.ColumnHeader();
            this._ctxMenu               = new System.Windows.Forms.ContextMenuStrip();
            this._ctxReencode           = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxExtractSubs        = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxBoth               = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxSeparator          = new System.Windows.Forms.ToolStripSeparator();
            this._ctxSkip               = new System.Windows.Forms.ToolStripMenuItem();
            this._txtLog                = new System.Windows.Forms.TextBox();
            this._statusStrip           = new System.Windows.Forms.StatusStrip();
            this._stripLabel            = new System.Windows.Forms.ToolStripStatusLabel();
            this._sep                   = new System.Windows.Forms.Panel();
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
            this._topPanel.Controls.Add(this._lblPreset);
            this._topPanel.Controls.Add(this._cmbPreset);
            this._topPanel.Controls.Add(this._chkNvenc);
            this._topPanel.Controls.Add(this._btnScan);
            this._topPanel.Controls.Add(this._btnReencodeAll);
            this._topPanel.Controls.Add(this._btnExtractSubsAll);
            this._topPanel.Controls.Add(this._btnBothAll);
            this._topPanel.Controls.Add(this._btnCancel);
            this._topPanel.Controls.Add(this._progressBar);
            this._topPanel.Controls.Add(this._lblStatus);
            this._topPanel.Controls.Add(this._lblElapsed);
            this._topPanel.Controls.Add(this._chkVerboseLogging);
            this._topPanel.Dock     = System.Windows.Forms.DockStyle.Top;
            this._topPanel.Location = new System.Drawing.Point(0, 0);
            this._topPanel.Name     = "_topPanel";
            this._topPanel.Padding  = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this._topPanel.Size     = new System.Drawing.Size(1495, 158);
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
            // _lblPreset
            //
            this._lblPreset.Location  = new System.Drawing.Point(12, 69);
            this._lblPreset.Name      = "_lblPreset";
            this._lblPreset.Size      = new System.Drawing.Size(80, 20);
            this._lblPreset.TabIndex  = 6;
            this._lblPreset.Text      = "Preset x264:";
            this._lblPreset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _cmbPreset
            //
            this._cmbPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbPreset.Items.AddRange(new object[] {
                "Muy rapido (recomendado)",
                "Rapido",
                "Medio",
                "Lento (mejor compresion)" });
            this._cmbPreset.Location      = new System.Drawing.Point(97, 66);
            this._cmbPreset.Name          = "_cmbPreset";
            this._cmbPreset.SelectedIndex = 0;
            this._cmbPreset.Size          = new System.Drawing.Size(210, 23);
            this._cmbPreset.TabIndex      = 7;
            //
            // _chkNvenc
            //
            this._chkNvenc.AutoSize  = true;
            this._chkNvenc.Font      = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._chkNvenc.ForeColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this._chkNvenc.Location  = new System.Drawing.Point(320, 68);
            this._chkNvenc.Name      = "_chkNvenc";
            this._chkNvenc.Size      = new System.Drawing.Size(220, 19);
            this._chkNvenc.TabIndex  = 8;
            this._chkNvenc.Text      = "Usar GPU NVIDIA (NVENC)  ⚡";
            this._chkNvenc.UseVisualStyleBackColor = true;
            //
            // _toolTip
            //
            this._toolTip.AutoPopDelay  = 15000;
            this._toolTip.InitialDelay  = 400;
            this._toolTip.ReshowDelay   = 200;
            this._toolTip.ShowAlways    = true;
            //
            // _btnScan
            //
            this._btnScan.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this._btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnScan.ForeColor = System.Drawing.Color.White;
            this._btnScan.Location  = new System.Drawing.Point(0, 95);
            this._btnScan.Name      = "_btnScan";
            this._btnScan.Size      = new System.Drawing.Size(130, 28);
            this._btnScan.TabIndex  = 8;
            this._btnScan.Text      = "Escanear MKV";
            this._btnScan.UseVisualStyleBackColor = false;
            //
            // _btnReencodeAll
            //
            this._btnReencodeAll.BackColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this._btnReencodeAll.Enabled   = false;
            this._btnReencodeAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnReencodeAll.ForeColor = System.Drawing.Color.White;
            this._btnReencodeAll.Location  = new System.Drawing.Point(140, 95);
            this._btnReencodeAll.Name      = "_btnReencodeAll";
            this._btnReencodeAll.Size      = new System.Drawing.Size(145, 28);
            this._btnReencodeAll.TabIndex  = 9;
            this._btnReencodeAll.Text      = "Re-encode Todo";
            this._btnReencodeAll.UseVisualStyleBackColor = false;
            //
            // _btnExtractSubsAll
            //
            this._btnExtractSubsAll.BackColor = System.Drawing.Color.FromArgb(142, 68, 173);
            this._btnExtractSubsAll.Enabled   = false;
            this._btnExtractSubsAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnExtractSubsAll.ForeColor = System.Drawing.Color.White;
            this._btnExtractSubsAll.Location  = new System.Drawing.Point(295, 95);
            this._btnExtractSubsAll.Name      = "_btnExtractSubsAll";
            this._btnExtractSubsAll.Size      = new System.Drawing.Size(150, 28);
            this._btnExtractSubsAll.TabIndex  = 10;
            this._btnExtractSubsAll.Text      = "Extraer Subtitulos";
            this._btnExtractSubsAll.UseVisualStyleBackColor = false;
            //
            // _btnBothAll
            //
            this._btnBothAll.BackColor = System.Drawing.Color.FromArgb(211, 84, 0);
            this._btnBothAll.Enabled   = false;
            this._btnBothAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBothAll.ForeColor = System.Drawing.Color.White;
            this._btnBothAll.Location  = new System.Drawing.Point(455, 95);
            this._btnBothAll.Name      = "_btnBothAll";
            this._btnBothAll.Size      = new System.Drawing.Size(100, 28);
            this._btnBothAll.TabIndex  = 11;
            this._btnBothAll.Text      = "Ambos";
            this._btnBothAll.UseVisualStyleBackColor = false;
            //
            // _btnCancel
            //
            this._btnCancel.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            this._btnCancel.Enabled   = false;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.ForeColor = System.Drawing.Color.White;
            this._btnCancel.Location  = new System.Drawing.Point(565, 95);
            this._btnCancel.Name      = "_btnCancel";
            this._btnCancel.Size      = new System.Drawing.Size(100, 28);
            this._btnCancel.TabIndex  = 12;
            this._btnCancel.Text      = "Cancelar";
            this._btnCancel.UseVisualStyleBackColor = false;
            //
            // _progressBar
            //
            this._progressBar.Anchor   = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(675, 96);
            this._progressBar.Name     = "_progressBar";
            this._progressBar.Size     = new System.Drawing.Size(808, 27);
            this._progressBar.Style    = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 13;
            //
            // _lblStatus
            //
            this._lblStatus.Anchor    = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this._lblStatus.ForeColor = System.Drawing.Color.Gray;
            this._lblStatus.Location  = new System.Drawing.Point(12, 131);
            this._lblStatus.Name      = "_lblStatus";
            this._lblStatus.Size      = new System.Drawing.Size(1196, 18);
            this._lblStatus.TabIndex  = 14;
            //
            // _lblElapsed
            //
            this._lblElapsed.Anchor    = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._lblElapsed.ForeColor = System.Drawing.Color.Gray;
            this._lblElapsed.Location  = new System.Drawing.Point(1215, 131);
            this._lblElapsed.Name      = "_lblElapsed";
            this._lblElapsed.Size      = new System.Drawing.Size(110, 18);
            this._lblElapsed.TabIndex  = 15;
            this._lblElapsed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // _chkVerboseLogging
            //
            this._chkVerboseLogging.Anchor   = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._chkVerboseLogging.AutoSize = true;
            this._chkVerboseLogging.Location = new System.Drawing.Point(1332, 131);
            this._chkVerboseLogging.Name     = "_chkVerboseLogging";
            this._chkVerboseLogging.Size     = new System.Drawing.Size(151, 19);
            this._chkVerboseLogging.TabIndex = 16;
            this._chkVerboseLogging.Text     = "Logging Mode: Verbose";
            this._chkVerboseLogging.UseVisualStyleBackColor = true;
            //
            // _tmrElapsed
            //
            this._tmrElapsed.Interval = 1000;
            //
            // _splitContainer
            //
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 159);
            this._splitContainer.Name     = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this._splitContainer.Panel1.Controls.Add(this._listView);
            this._splitContainer.Panel1MinSize = 100;
            this._splitContainer.Panel2.Controls.Add(this._txtLog);
            this._splitContainer.Panel2MinSize = 80;
            this._splitContainer.Size            = new System.Drawing.Size(1495, 500);
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
            this._colSize.Text  = "Tamano";
            this._colSize.Width = 80;
            //
            // _colHevc
            //
            this._colHevc.Text  = "HEVC 10-bit";
            this._colHevc.Width = 95;
            //
            // _colSubs
            //
            this._colSubs.Text  = "Subtitulos";
            this._colSubs.Width = 85;
            //
            // _colStatus
            //
            this._colStatus.Text  = "Estado";
            this._colStatus.Width = 220;
            //
            // _colAction
            //
            this._colAction.Text  = "Accion";
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
            this._ctxReencode.Text = "Re-encode video (H.264)";
            //
            // _ctxExtractSubs
            //
            this._ctxExtractSubs.Name = "_ctxExtractSubs";
            this._ctxExtractSubs.Size = new System.Drawing.Size(224, 22);
            this._ctxExtractSubs.Text = "Extraer subtitulos";
            //
            // _ctxBoth
            //
            this._ctxBoth.Name = "_ctxBoth";
            this._ctxBoth.Size = new System.Drawing.Size(224, 22);
            this._ctxBoth.Text = "Ambos";
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
            this._ctxSkip.Text = "Omitir";
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
            this._sep.Location  = new System.Drawing.Point(0, 158);
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
            this.Text          = "MKV / Roku Fixer - HEVC + Subtitulos";
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
        private System.Windows.Forms.Label              _lblPreset;
        private System.Windows.Forms.ComboBox           _cmbPreset;
        private System.Windows.Forms.Button             _btnScan;
        private System.Windows.Forms.Button             _btnReencodeAll;
        private System.Windows.Forms.Button             _btnExtractSubsAll;
        private System.Windows.Forms.Button             _btnBothAll;
        private System.Windows.Forms.Button             _btnCancel;
        private System.Windows.Forms.ProgressBar        _progressBar;
        private System.Windows.Forms.Label              _lblStatus;
        private System.Windows.Forms.Label              _lblElapsed;
        private System.Windows.Forms.CheckBox           _chkVerboseLogging;
        private System.Windows.Forms.Timer              _tmrElapsed;
        private System.Windows.Forms.CheckBox           _chkNvenc;
        private System.Windows.Forms.ToolTip            _toolTip;
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
