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
            this._lblThreshold.Text      = "Tamano minimo:";
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
            this._splitContainer.Dock        = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location    = new System.Drawing.Point(0, 159);
            this._splitContainer.Name        = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this._splitContainer.Panel1.Controls.Add(this._grid);
            this._splitContainer.Panel1MinSize  = 100;
            this._splitContainer.Panel2.Controls.Add(this._txtLog);
            this._splitContainer.Panel2MinSize  = 80;
            this._splitContainer.Size           = new System.Drawing.Size(1495, 500);
            this._splitContainer.SplitterDistance = 300;
            this._splitContainer.TabIndex       = 0;
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
            this._gridColSelected.HeaderText = "V";
            this._gridColSelected.Name       = "_gridColSelected";
            this._gridColSelected.ReadOnly   = false;
            this._gridColSelected.Width      = 30;
            //
            // _gridColFileName
            //
            this._gridColFileName.HeaderText = "Archivo";
            this._gridColFileName.Name       = "_gridColFileName";
            this._gridColFileName.ReadOnly   = true;
            this._gridColFileName.Width      = 380;
            //
            // _gridColFullPath
            //
            this._gridColFullPath.HeaderText = "Ruta";
            this._gridColFullPath.Name       = "_gridColFullPath";
            this._gridColFullPath.ReadOnly   = true;
            this._gridColFullPath.Width      = 430;
            //
            // _gridColSize
            //
            this._gridColSize.HeaderText = "Tamano";
            this._gridColSize.Name       = "_gridColSize";
            this._gridColSize.ReadOnly   = true;
            this._gridColSize.Width      = 90;
            //
            // _gridColRecommended
            //
            this._gridColRecommended.HeaderText = "Recomendado";
            this._gridColRecommended.Name       = "_gridColRecommended";
            this._gridColRecommended.ReadOnly   = true;
            this._gridColRecommended.Width      = 110;
            //
            // _gridColStatus
            //
            this._gridColStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this._gridColStatus.HeaderText   = "Estado";
            this._gridColStatus.Name         = "_gridColStatus";
            this._gridColStatus.ReadOnly     = true;
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
            this.Text          = "Comprimir Archivos Grandes - HandBrakeCLI";
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
