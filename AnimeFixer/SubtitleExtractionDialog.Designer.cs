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
