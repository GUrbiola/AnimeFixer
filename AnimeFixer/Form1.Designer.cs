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
            this._lblTitle         = new System.Windows.Forms.Label();
            this._pnlMp4           = new System.Windows.Forms.Panel();
            this._lblMp4Title      = new System.Windows.Forms.Label();
            this._lblMp4Desc       = new System.Windows.Forms.Label();
            this._btnMp4Fixer      = new System.Windows.Forms.Button();
            this._pnlMkv           = new System.Windows.Forms.Panel();
            this._lblMkvTitle      = new System.Windows.Forms.Label();
            this._lblMkvDesc       = new System.Windows.Forms.Label();
            this._btnMkvFixer      = new System.Windows.Forms.Button();
            this._pnlCompress      = new System.Windows.Forms.Panel();
            this._lblCompressTitle = new System.Windows.Forms.Label();
            this._lblCompressDesc  = new System.Windows.Forms.Label();
            this._btnCompressFixer = new System.Windows.Forms.Button();
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
            this._lblTitle.Text      = "AnimeFixer - Seleccionar modo";
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
            this._lblMp4Desc.Text     = "Detecta y corrige archivos MP4 con streams GPAC invalidos que causan crashes en Roku TV.";
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
            this._lblMkvDesc.Text     = "Detecta HEVC 10-bit en MKV y re-encodea a H.264. Extrae subtitulos ASS embebidos a archivos SRT para Roku.";
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
            this._lblCompressDesc.Text     = "Re-encodea archivos grandes (.mkv/.mp4) a H.264 via HandBrakeCLI para reducir tamano preservando audio y subtitulos.";
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
