namespace AnimeFixer
{
    partial class MP4FixerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._topPanel = new System.Windows.Forms.Panel();
            this._lblFFmpegPath = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._txtFFmpegFolder = new System.Windows.Forms.TextBox();
            this._btnBrowseFFmpeg = new System.Windows.Forms.Button();
            this._lblFolder = new System.Windows.Forms.Label();
            this._txtFolder = new System.Windows.Forms.TextBox();
            this._btnBrowseFolder = new System.Windows.Forms.Button();
            this._btnScan = new System.Windows.Forms.Button();
            this._btnFixAll = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this._lblStatus = new System.Windows.Forms.Label();
            this._chkVerboseLogging = new System.Windows.Forms.CheckBox();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._listView = new System.Windows.Forms.ListView();
            this._txtLog = new System.Windows.Forms.TextBox();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._stripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.sep = new System.Windows.Forms.Panel();
            this._topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _topPanel
            // 
            this._topPanel.BackColor = System.Drawing.Color.White;
            this._topPanel.Controls.Add(this._lblFFmpegPath);
            this._topPanel.Controls.Add(this._progressBar);
            this._topPanel.Controls.Add(this._txtFFmpegFolder);
            this._topPanel.Controls.Add(this._btnBrowseFFmpeg);
            this._topPanel.Controls.Add(this._lblFolder);
            this._topPanel.Controls.Add(this._txtFolder);
            this._topPanel.Controls.Add(this._btnBrowseFolder);
            this._topPanel.Controls.Add(this._btnScan);
            this._topPanel.Controls.Add(this._btnFixAll);
            this._topPanel.Controls.Add(this._btnCancel);
            this._topPanel.Controls.Add(this._lblStatus);
            this._topPanel.Controls.Add(this._chkVerboseLogging);
            this._topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._topPanel.Location = new System.Drawing.Point(0, 0);
            this._topPanel.Name = "_topPanel";
            this._topPanel.Padding = new System.Windows.Forms.Padding(12, 10, 12, 10);
            this._topPanel.Size = new System.Drawing.Size(1495, 130);
            this._topPanel.TabIndex = 2;
            // 
            // _lblFFmpegPath
            // 
            this._lblFFmpegPath.Location = new System.Drawing.Point(12, 9);
            this._lblFFmpegPath.Name = "_lblFFmpegPath";
            this._lblFFmpegPath.Size = new System.Drawing.Size(180, 20);
            this._lblFFmpegPath.TabIndex = 0;
            this._lblFFmpegPath.Text = "Carpeta de ffmpeg / ffprobe:";
            this._lblFFmpegPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _progressBar
            // 
            this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(399, 81);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(1084, 27);
            this._progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this._progressBar.TabIndex = 9;
            this._progressBar.Click += new System.EventHandler(this._progressBar_Click);
            // 
            // _txtFFmpegFolder
            // 
            this._txtFFmpegFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtFFmpegFolder.Location = new System.Drawing.Point(198, 6);
            this._txtFFmpegFolder.Name = "_txtFFmpegFolder";
            this._txtFFmpegFolder.Size = new System.Drawing.Size(1247, 23);
            this._txtFFmpegFolder.TabIndex = 1;
            // 
            // _btnBrowseFFmpeg
            // 
            this._btnBrowseFFmpeg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnBrowseFFmpeg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseFFmpeg.Location = new System.Drawing.Point(1451, 4);
            this._btnBrowseFFmpeg.Name = "_btnBrowseFFmpeg";
            this._btnBrowseFFmpeg.Size = new System.Drawing.Size(32, 23);
            this._btnBrowseFFmpeg.TabIndex = 2;
            this._btnBrowseFFmpeg.Text = "...";
            // 
            // _lblFolder
            // 
            this._lblFolder.Location = new System.Drawing.Point(12, 42);
            this._lblFolder.Name = "_lblFolder";
            this._lblFolder.Size = new System.Drawing.Size(180, 20);
            this._lblFolder.TabIndex = 3;
            this._lblFolder.Text = "Carpeta de medios:";
            this._lblFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _txtFolder
            // 
            this._txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._txtFolder.Location = new System.Drawing.Point(198, 41);
            this._txtFolder.Name = "_txtFolder";
            this._txtFolder.Size = new System.Drawing.Size(1247, 23);
            this._txtFolder.TabIndex = 4;
            // 
            // _btnBrowseFolder
            // 
            this._btnBrowseFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnBrowseFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBrowseFolder.Location = new System.Drawing.Point(1451, 40);
            this._btnBrowseFolder.Name = "_btnBrowseFolder";
            this._btnBrowseFolder.Size = new System.Drawing.Size(32, 24);
            this._btnBrowseFolder.TabIndex = 5;
            this._btnBrowseFolder.Text = "...";
            // 
            // _btnScan
            // 
            this._btnScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this._btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnScan.ForeColor = System.Drawing.Color.White;
            this._btnScan.Location = new System.Drawing.Point(0, 80);
            this._btnScan.Name = "_btnScan";
            this._btnScan.Size = new System.Drawing.Size(130, 28);
            this._btnScan.TabIndex = 6;
            this._btnScan.Text = "🔍 Escanear";
            this._btnScan.UseVisualStyleBackColor = false;
            // 
            // _btnFixAll
            // 
            this._btnFixAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this._btnFixAll.Enabled = false;
            this._btnFixAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnFixAll.ForeColor = System.Drawing.Color.White;
            this._btnFixAll.Location = new System.Drawing.Point(140, 80);
            this._btnFixAll.Name = "_btnFixAll";
            this._btnFixAll.Size = new System.Drawing.Size(140, 28);
            this._btnFixAll.TabIndex = 7;
            this._btnFixAll.Text = "🔧 Corregir todo";
            this._btnFixAll.UseVisualStyleBackColor = false;
            // 
            // _btnCancel
            // 
            this._btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this._btnCancel.Enabled = false;
            this._btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCancel.ForeColor = System.Drawing.Color.White;
            this._btnCancel.Location = new System.Drawing.Point(290, 80);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(100, 28);
            this._btnCancel.TabIndex = 8;
            this._btnCancel.Text = "✖ Cancelar";
            this._btnCancel.UseVisualStyleBackColor = false;
            this._btnCancel.Click += new System.EventHandler(this._btnCancel_Click);
            // 
            // _lblStatus
            // 
            this._lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._lblStatus.ForeColor = System.Drawing.Color.Gray;
            this._lblStatus.Location = new System.Drawing.Point(12, 110);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new System.Drawing.Size(1314, 18);
            this._lblStatus.TabIndex = 10;
            // 
            // _chkVerboseLogging
            // 
            this._chkVerboseLogging.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._chkVerboseLogging.AutoSize = true;
            this._chkVerboseLogging.Location = new System.Drawing.Point(1332, 110);
            this._chkVerboseLogging.Name = "_chkVerboseLogging";
            this._chkVerboseLogging.Size = new System.Drawing.Size(151, 19);
            this._chkVerboseLogging.TabIndex = 11;
            this._chkVerboseLogging.Text = "Loggind Mode: Verbose";
            this._chkVerboseLogging.UseVisualStyleBackColor = true;
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 131);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._listView);
            this._splitContainer.Panel1MinSize = 100;
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._txtLog);
            this._splitContainer.Panel2MinSize = 80;
            this._splitContainer.Size = new System.Drawing.Size(1495, 528);
            this._splitContainer.SplitterDistance = 200;
            this._splitContainer.TabIndex = 0;
            // 
            // _listView
            // 
            this._listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listView.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this._listView.FullRowSelect = true;
            this._listView.GridLines = true;
            this._listView.HideSelection = false;
            this._listView.Location = new System.Drawing.Point(0, 0);
            this._listView.MultiSelect = false;
            this._listView.Name = "_listView";
            this._listView.Size = new System.Drawing.Size(1495, 200);
            this._listView.TabIndex = 0;
            this._listView.UseCompatibleStateImageBehavior = false;
            this._listView.View = System.Windows.Forms.View.Details;
            // 
            // _txtLog
            // 
            this._txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtLog.Font = new System.Drawing.Font("Consolas", 8F);
            this._txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this._txtLog.Location = new System.Drawing.Point(0, 0);
            this._txtLog.Multiline = true;
            this._txtLog.Name = "_txtLog";
            this._txtLog.ReadOnly = true;
            this._txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtLog.Size = new System.Drawing.Size(1495, 324);
            this._txtLog.TabIndex = 0;
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._stripLabel});
            this._statusStrip.Location = new System.Drawing.Point(0, 659);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(1495, 22);
            this._statusStrip.SizingGrip = false;
            this._statusStrip.TabIndex = 3;
            // 
            // _stripLabel
            // 
            this._stripLabel.Name = "_stripLabel";
            this._stripLabel.Size = new System.Drawing.Size(1480, 17);
            this._stripLabel.Spring = true;
            this._stripLabel.Text = "Listo.";
            this._stripLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sep
            // 
            this.sep.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(225)))));
            this.sep.Dock = System.Windows.Forms.DockStyle.Top;
            this.sep.Location = new System.Drawing.Point(0, 130);
            this.sep.Name = "sep";
            this.sep.Size = new System.Drawing.Size(1495, 1);
            this.sep.TabIndex = 1;
            // 
            // MP4FixerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1495, 681);
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this.sep);
            this.Controls.Add(this._topPanel);
            this.Controls.Add(this._statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(800, 520);
            this.Name = "MP4FixerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MP4 Stream Fixer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MP4FixerForm_Load);
            this._topPanel.ResumeLayout(false);
            this._topPanel.PerformLayout();
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel _topPanel;
        private System.Windows.Forms.Label _lblFFmpegPath;
        private System.Windows.Forms.TextBox _txtFFmpegFolder;
        private System.Windows.Forms.Button _btnBrowseFFmpeg;
        private System.Windows.Forms.Label _lblFolder;
        private System.Windows.Forms.TextBox _txtFolder;
        private System.Windows.Forms.Button _btnBrowseFolder;
        private System.Windows.Forms.Button _btnScan;
        private System.Windows.Forms.Button _btnFixAll;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.CheckBox _chkVerboseLogging;
        private System.Windows.Forms.ListView _listView;
        private System.Windows.Forms.TextBox _txtLog;
        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _stripLabel;
        private System.Windows.Forms.Panel sep;
    }
}
