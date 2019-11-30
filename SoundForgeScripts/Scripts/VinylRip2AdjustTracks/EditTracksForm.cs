using System;
using SoundForgeScriptsLib.Utils;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksForm
    {
        protected internal EventHandler PreviewAllClicked;
        protected internal EventHandler PreviewStartClicked;
        protected internal EventHandler PreviewEndClicked;
        protected internal EventHandler NextClicked;
        protected internal EventHandler PreviousClicked;
        protected internal EventHandler DeleteClicked;
        protected internal EventHandler AddTrackClicked;
        private Form _form;


        public void Create(EditTracksViewModel viewModel)
        {
            //TODO: I think it's ok, but check it's ok to interact with the file window while the script window is active...
            //TODO: and figure out the layout...!

            _form = new Form();
            Size sForm = new Size(520, 160);

            _form.Text = viewModel.FormTitle;
            _form.FormBorderStyle = FormBorderStyle.FixedDialog;
            _form.MaximizeBox = false;
            _form.MinimizeBox = false;
            _form.StartPosition = FormStartPosition.CenterScreen;
            _form.ClientSize = sForm;
            _form.AutoScroll = false;

            Point pt = new Point(10, 10);
            Size sOff = new Size(10, 10);
            Size sSpacer = new Size(10, 15);
            const int lblHeight = 16;
            const int fatButtonHeight = 25;
            //const int tbxHeight = 16;

            Label lblPrompt = new Label();
            lblPrompt.Text = "Adjust track region definitions - click OK to apply changes.";
            lblPrompt.Width = sForm.Width - pt.X - sOff.Width;
            lblPrompt.Height = lblHeight;
            lblPrompt.Location = pt;
            //lblPrompt.BackColor = Color.Aqua;
            _form.Controls.Add(lblPrompt);

            pt.Y += lblPrompt.Height + sSpacer.Height;

            const string previewIcon = "SoundForgeScriptsLib.Resources.ionicons_android-volume-up_17.png";

            Button btnPreviewAll = new Button();
            btnPreviewAll.TabStop = true;
            //btnNext.TabIndex = 4;
            btnPreviewAll.Width = (lblPrompt.Width - sSpacer.Width) / 2;
            btnPreviewAll.Height = fatButtonHeight;
            btnPreviewAll.Text = "Preview All";
            btnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            btnPreviewAll.Location = pt;
            btnPreviewAll.ImageAlign = ContentAlignment.MiddleLeft;
            btnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            btnPreviewAll.Click += PreviewAllClicked; 
            btnPreviewAll.DataBindings.Add("Enabled", viewModel, "HasTracks", false, DataSourceUpdateMode.OnPropertyChanged);
            ResourceHelper.GetResourceStream(previewIcon, delegate(Stream stream) { btnPreviewAll.Image = Image.FromStream(stream); });
            _form.Controls.Add(btnPreviewAll);

            pt.X += sSpacer.Width + btnPreviewAll.Width;

            Button btnAddTrack = new Button();
            btnAddTrack.TabStop = true;
            btnAddTrack.Height = fatButtonHeight;
            //btnNext.TabIndex = 4;
            btnAddTrack.Width = btnPreviewAll.Width;
            btnAddTrack.Text = "Add Track From Selection";
            btnAddTrack.TextAlign = ContentAlignment.MiddleCenter;
            btnAddTrack.Location = pt;
            btnAddTrack.Click += AddTrackClicked; 
            _form.Controls.Add(btnAddTrack);

            pt.Y += btnAddTrack.Height + sSpacer.Height;
            pt.X = sOff.Width;

            Button btnPreviewStart = new Button();
            btnPreviewStart.TabStop = true;
            //btnPreviewStart.TabIndex = 4;
            btnPreviewStart.Text = "Start";
            btnPreviewStart.Height = fatButtonHeight;
            btnPreviewStart.ImageAlign = ContentAlignment.MiddleLeft;
            btnPreviewStart.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate(Stream stream) { btnPreviewStart.Image = Image.FromStream(stream); });
            btnPreviewStart.Location = pt;
            btnPreviewStart.Click += PreviewStartClicked; 
            btnPreviewStart.DataBindings.Add("Enabled", viewModel, "HasTracks", false, DataSourceUpdateMode.OnPropertyChanged);
            _form.Controls.Add(btnPreviewStart);
            //btnPrvwStart.Click += FormHelper.OnOK_Click;

            //pt.Y += btnPrvwStart.Height + sSpacer.Height;
            pt.X += btnPreviewStart.Width + sSpacer.Width;

            Label lblTrack = new Label();
            lblTrack.Text = string.Format("Track {0}", 1);
            lblTrack.Width = sForm.Width - 2 * (sOff.Width + btnPreviewStart.Width + sSpacer.Width);
            lblTrack.Height = fatButtonHeight;
            lblTrack.TextAlign = ContentAlignment.MiddleCenter;
            lblTrack.Location = pt;
            lblTrack.DataBindings.Add("Text", viewModel, "TrackName", false, DataSourceUpdateMode.OnPropertyChanged);
            //Output.ToScriptWindow("pt.y {0}", pt.Y);
            lblTrack.BackColor = Color.LightGray;
            lblTrack.BorderStyle = BorderStyle.FixedSingle;
            _form.Controls.Add(lblTrack);

            pt.X += sSpacer.Width + lblTrack.Width;

            Button btnPreviewEnd = new Button();
            btnPreviewEnd.Width = btnPreviewStart.Width;
            btnPreviewEnd.Height = fatButtonHeight;
            btnPreviewEnd.TabStop = true;
            //btnPrvwEnd.TabIndex = 4;
            btnPreviewEnd.Text = "End";
            btnPreviewEnd.ImageAlign = ContentAlignment.MiddleLeft;
            btnPreviewEnd.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate(Stream stream) { btnPreviewEnd.Image = Image.FromStream(stream); });
            btnPreviewEnd.Location = pt;
            btnPreviewEnd.Click += PreviewEndClicked; 
            btnPreviewEnd.DataBindings.Add("Enabled", viewModel, "HasTracks", false, DataSourceUpdateMode.OnPropertyChanged);
            _form.Controls.Add(btnPreviewEnd);
            //btnPrvwEnd.Click += FormHelper.OnOK_Click;

            pt.X = sOff.Width;
            pt.Y += btnPreviewEnd.Height + sSpacer.Height;

            Button btnPrevious = new Button();
            //btnNext.Width = btnPrvwStart.Width;
            btnPrevious.TabStop = true;
            //btnNext.TabIndex = 4;
            btnPrevious.Width = 120; 
            btnPrevious.Text = "<< Previous Track";
            btnPrevious.TextAlign = ContentAlignment.MiddleCenter;
            btnPrevious.Location = pt;
            btnPrevious.Click += PreviousClicked;
            btnPrevious.DataBindings.Add("Enabled", viewModel, "CanNavigatePrevious", false, DataSourceUpdateMode.OnPropertyChanged);
            _form.Controls.Add(btnPrevious);

            Button btnDelete = new Button();
            btnDelete.TabStop = true;
            //btnDelete.TabIndex = 4;
            btnDelete.Width = 80; 
            btnDelete.Text = "DELETE";
            btnDelete.ForeColor = Color.Crimson;
            btnDelete.Font = new Font(btnDelete.Font.Name, btnDelete.Font.Size, FontStyle.Bold);
            btnDelete.TextAlign = ContentAlignment.MiddleCenter;
            btnDelete.Click += DeleteClicked; 
            btnDelete.DataBindings.Add("Enabled", viewModel, "HasTracks", false, DataSourceUpdateMode.OnPropertyChanged);
            _form.Controls.Add(btnDelete);

            int bottomRowGapWidth = (lblPrompt.Width - (2 * btnPrevious.Width) - btnDelete.Width) / 2;
            pt.X += bottomRowGapWidth + btnPrevious.Width;

            btnDelete.Location = pt;

            pt.X += bottomRowGapWidth + btnDelete.Width;

            Button btnNext = new Button();
            btnNext.Width = btnPrevious.Width;
            btnNext.TabStop = true;
            //btnNext.TabIndex = 4;
            btnNext.Text = "Next Track >>";
            btnNext.TextAlign = ContentAlignment.MiddleCenter;
            btnNext.Location = pt;
            btnNext.Click += NextClicked; 
            btnNext.DataBindings.Add("Enabled", viewModel, "CanNavigateNext", false, DataSourceUpdateMode.OnPropertyChanged);
            _form.Controls.Add(btnNext);

            //btnNext.Click += FormHelper.OnOK_Click;

            //btnNext.Click += FormHelper.OnOK_Click;

            //foreach (SplitTrackDefinition track in tracks)
            //{

            //    Button btn = new Button();
            //    btn.TabStop = true;
            //    //btn.TabIndex = 4;
            //    btn.Text = string.Format("Track {0}", track.Number);
            //    btn.ImageAlign = ContentAlignment.MiddleLeft;
            //    btn.TextAlign = ContentAlignment.MiddleRight;
            //    ResourceHelper.GetResourceStream(previewIcon, delegate(Stream stream) { btn.Image = Image.FromStream(stream); });
            //    btn.Location = pt;
            //    dlg.Controls.Add(btn);
            //    //btn.Click += FormHelper.OnOK_Click;

            //    pt.Y += btn.Height + sSpacer.Height;

            //}
            //dlg.AcceptButton = btn;
        }

        public void Show(IWin32Window hOwner)
        {
            _form.ShowDialog(hOwner);
        }

        public void Close()
        {
            _form.Hide();
            _form.Close();
            _form.Dispose();
        }
    }
}
