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

        private void BtnCompressFixer_Click(object sender, EventArgs e)
        {
            new CompressForm().Show();
        }
    }
}
