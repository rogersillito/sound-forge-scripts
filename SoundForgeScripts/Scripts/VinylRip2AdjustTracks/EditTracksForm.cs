using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksForm : Form
    {
        protected internal EventHandler PreviewAllClicked;
        protected internal EventHandler PreviewStartClicked;
        protected internal EventHandler PreviewEndClicked;
        protected internal EventHandler NextClicked;
        protected internal EventHandler PreviousClicked;
        protected internal EventHandler DeleteClicked;
        protected internal EventHandler AddTrackClicked;
        protected internal EventHandler StopPreviewClicked;
        protected internal EventHandler LoopPlaybackClicked;
        protected internal Button BtnPreviewAll = new Button();
        protected internal Button BtnStopPreview = new Button();
        protected internal Button BtnPreviewStart = new Button();
        protected internal Button BtnPreviewEnd = new Button();
        protected internal Button BtnPrevious = new Button();
        protected internal Button BtnNext = new Button();
        protected internal Button BtnAddTrack = new Button();
        protected internal Label LblTrack = new Label();
        protected internal Button BtnDelete = new Button();

        private ToolTip _toolTip;

        public EditTracksForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Size sForm = new Size(520, 160);

            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = sForm;
            AutoScroll = false;
            KeyPreview = true;

            Point pt = new Point(10, 10);
            Size sOff = new Size(10, 10);
            Size sSpacer = new Size(10, 15);
            const int lblHeight = 16;
            const int fatButtonHeight = 25;
            //const int tbxHeight = 16;

            _toolTip = new ToolTip();

            Label lblPrompt = new Label();
            lblPrompt.Text = "Adjust track region definitions - click OK to apply changes.";
            lblPrompt.Width = sForm.Width - pt.X - sOff.Width;
            lblPrompt.Height = lblHeight;
            lblPrompt.Location = pt;
            //lblPrompt.BackColor = Color.Aqua;
            Controls.Add(lblPrompt);

            pt.Y += lblPrompt.Height + sSpacer.Height;

            const string previewIcon = "SoundForgeScriptsLib.Resources.ionicons_android-volume-up_17.png";

            BtnPreviewAll.TabStop = true;
            //btnNext.TabIndex = 4;
            BtnPreviewAll.Width = (lblPrompt.Width - sSpacer.Width) / 2;
            BtnPreviewAll.Height = fatButtonHeight;
            BtnPreviewAll.Text = "Preview All";
            BtnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            BtnPreviewAll.Location = pt;
            BtnPreviewAll.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            BtnPreviewAll.Click += PreviewAllClicked;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewAll.Image = Image.FromStream(stream); });
            Controls.Add(BtnPreviewAll);

            pt.X += sSpacer.Width + BtnPreviewAll.Width;

            BtnStopPreview.TabStop = true;
            BtnStopPreview.Height = fatButtonHeight;
            //btnNext.TabIndex = 4;
            BtnStopPreview.Width = BtnPreviewAll.Width;
            BtnStopPreview.Text = "Stop Preview";
            _toolTip.SetToolTip(BtnStopPreview, "Keyboard: Space");
            BtnStopPreview.TextAlign = ContentAlignment.MiddleCenter;
            BtnStopPreview.Location = pt;
            BtnStopPreview.Click += StopPreviewClicked;
            Controls.Add(BtnStopPreview);

            // NEW ROW
            pt.Y += BtnStopPreview.Height + sSpacer.Height;
            pt.X = sOff.Width;

            BtnPreviewStart.TabStop = true;
            //btnPreviewStart.TabIndex = 4;
            BtnPreviewStart.Text = "Start";
            _toolTip.SetToolTip(BtnPreviewStart, "Keyboard: Home");
            BtnPreviewStart.Height = fatButtonHeight;
            BtnPreviewStart.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewStart.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewStart.Image = Image.FromStream(stream); });
            BtnPreviewStart.Location = pt;
            BtnPreviewStart.Click += PreviewStartClicked;
            Controls.Add(BtnPreviewStart);
            //btnPrvwStart.Click += FormHelper.OnOK_Click;

            //pt.Y += btnPrvwStart.Height + sSpacer.Height;
            pt.X += BtnPreviewStart.Width + sSpacer.Width;

            LblTrack.Text = string.Format("Track {0}", 1);
            LblTrack.Width = sForm.Width - 2 * (sOff.Width + BtnPreviewStart.Width + sSpacer.Width);
            LblTrack.Height = fatButtonHeight;
            LblTrack.TextAlign = ContentAlignment.MiddleCenter;
            LblTrack.Location = pt;
            //Output.ToScriptWindow("pt.y {0}", pt.Y);
            LblTrack.BackColor = Color.LightGray;
            LblTrack.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(LblTrack);

            pt.X += sSpacer.Width + LblTrack.Width;

            BtnPreviewEnd.Width = BtnPreviewStart.Width;
            BtnPreviewEnd.Height = fatButtonHeight;
            BtnPreviewEnd.TabStop = true;
            //btnPrvwEnd.TabIndex = 4;
            BtnPreviewEnd.Text = "End";
            _toolTip.SetToolTip(BtnPreviewEnd, "Keyboard: End");
            BtnPreviewEnd.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewEnd.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewEnd.Image = Image.FromStream(stream); });
            BtnPreviewEnd.Location = pt;
            BtnPreviewEnd.Click += PreviewEndClicked;
            Controls.Add(BtnPreviewEnd);
            //btnPrvwEnd.Click += FormHelper.OnOK_Click;

            // NEW ROW
            pt.X = sOff.Width;
            pt.Y += BtnPreviewEnd.Height + sSpacer.Height;

            //btnNext.Width = btnPrvwStart.Width;
            BtnPrevious.TabStop = true;
            //btnNext.TabIndex = 4;
            BtnPrevious.Width = 120;
            BtnPrevious.Text = "<< Previous Track";
            _toolTip.SetToolTip(BtnPrevious, "Keyboard: Ctrl + Left");
            BtnPrevious.TextAlign = ContentAlignment.MiddleCenter;
            BtnPrevious.Location = pt;
            BtnPrevious.Click += PreviousClicked;
            Controls.Add(BtnPrevious);

            BtnDelete.TabStop = true;
            //btnDelete.TabIndex = 4;
            BtnDelete.Width = 80;
            BtnDelete.Text = "DELETE";
            _toolTip.SetToolTip(BtnDelete, "Keyboard: Del");
            BtnDelete.ForeColor = Color.Crimson;
            BtnDelete.Font = new Font(BtnDelete.Font.Name, BtnDelete.Font.Size, FontStyle.Bold);
            BtnDelete.TextAlign = ContentAlignment.MiddleCenter;
            BtnDelete.Click += DeleteClicked;
            Controls.Add(BtnDelete);

            int bottomRowGapWidth = (lblPrompt.Width - (2 * BtnPrevious.Width) - BtnDelete.Width) / 2;
            pt.X += bottomRowGapWidth + BtnPrevious.Width;

            BtnDelete.Location = pt;

            pt.X += bottomRowGapWidth + BtnDelete.Width;

            BtnNext.Width = BtnPrevious.Width;
            BtnNext.TabStop = true;
            //btnNext.TabIndex = 4;
            BtnNext.Text = "Next Track >>";
            _toolTip.SetToolTip(BtnNext, "Keyboard: Ctrl + Right");
            BtnNext.TextAlign = ContentAlignment.MiddleCenter;
            BtnNext.Location = pt;
            BtnNext.Click += NextClicked;
            Controls.Add(BtnNext);

            // NEW ROW
            pt.Y += BtnNext.Height + sSpacer.Height;
            pt.X = sOff.Width;

            BtnAddTrack.TabStop = true;
            BtnAddTrack.Height = fatButtonHeight;
            //btnNext.TabIndex = 4;
            BtnAddTrack.Width = BtnPreviewAll.Width;
            BtnAddTrack.Text = "Add Track From Selection";
            BtnAddTrack.TextAlign = ContentAlignment.MiddleCenter;
            BtnAddTrack.Location = pt;
            BtnAddTrack.Click += AddTrackClicked;
            Controls.Add(BtnAddTrack);

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
    }
}
