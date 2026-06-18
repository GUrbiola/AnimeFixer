using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AnimeFixer.Models;

namespace AnimeFixer
{
    public partial class SubtitleExtractionDialog : Form
    {
        private readonly string              _videoFilePath;
        private readonly List<SubtitleTrack> _tracks;

        public List<SubtitleTrack> SelectedTracks { get; private set; }

        public SubtitleExtractionDialog(string videoFilePath, List<SubtitleTrack> tracks)
        {
            InitializeComponent();
            _videoFilePath = videoFilePath;
            _tracks        = tracks;
            _lblFile.Text  = Path.GetFileName(videoFilePath);
            PopulateList();
        }

        private void PopulateList()
        {
            var nameNoExt = Path.GetFileNameWithoutExtension(_videoFilePath);
            _listView.Items.Clear();

            foreach (var track in _tracks)
            {
                var item = new ListViewItem(track.Index.ToString());
                item.SubItems.Add(track.Language);
                item.SubItems.Add(track.Title);
                item.SubItems.Add(track.Codec);
                item.SubItems.Add($"{nameNoExt}.{track.SuggestedFileSuffix}.srt");
                item.Checked = track.Selected;
                item.Tag     = track;
                _listView.Items.Add(item);
            }
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _listView.Items)
                item.Checked = true;
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _listView.Items)
                item.Checked = false;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            SelectedTracks = new List<SubtitleTrack>();
            foreach (ListViewItem item in _listView.Items)
            {
                if (item.Checked)
                {
                    var track = (SubtitleTrack)item.Tag;
                    track.Selected = true;
                    SelectedTracks.Add(track);
                }
            }

            if (SelectedTracks.Count == 0)
            {
                MessageBox.Show("Selecciona al menos una pista de subtítulos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
