using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksForm : Form
    {
        protected internal Button BtnPreviewAll = new Button();
        protected internal Button BtnStopPreview = new Button();
        protected internal Button BtnPreviewStart = new Button();
        protected internal Button BtnPreviewEnd = new Button();
        protected internal Button BtnPrevious = new Button();
        protected internal Button BtnNext = new Button();
        protected internal Button BtnAddTrack = new Button();
        protected internal Button BtnDelete = new Button();

        protected internal Label LblTrack = new Label();

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
            Controls.Add(lblPrompt);

            pt.Y += lblPrompt.Height + sSpacer.Height;

            const string previewIcon = "SoundForgeScriptsLib.Resources.ionicons_android-volume-up_17.png";

            BtnPreviewAll.TabStop = true;
            BtnPreviewAll.Width = (lblPrompt.Width - sSpacer.Width) / 2;
            BtnPreviewAll.Height = fatButtonHeight;
            BtnPreviewAll.Text = "Preview All";
            BtnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            BtnPreviewAll.Location = pt;
            BtnPreviewAll.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewAll.Image = Image.FromStream(stream); });
            Controls.Add(BtnPreviewAll);

            pt.X += sSpacer.Width + BtnPreviewAll.Width;

            BtnStopPreview.TabStop = true;
            BtnStopPreview.Height = fatButtonHeight;
            BtnStopPreview.Width = BtnPreviewAll.Width;
            BtnStopPreview.Text = "Stop Preview";
            _toolTip.SetToolTip(BtnStopPreview, "Keyboard: Space");
            BtnStopPreview.TextAlign = ContentAlignment.MiddleCenter;
            BtnStopPreview.Location = pt;
            Controls.Add(BtnStopPreview);

            // NEW ROW
            pt.Y += BtnStopPreview.Height + sSpacer.Height;
            pt.X = sOff.Width;

            BtnPreviewStart.TabStop = true;
            BtnPreviewStart.Text = "Start";
            _toolTip.SetToolTip(BtnPreviewStart, "Keyboard: Home");
            BtnPreviewStart.Height = fatButtonHeight;
            BtnPreviewStart.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewStart.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewStart.Image = Image.FromStream(stream); });
            BtnPreviewStart.Location = pt;
            Controls.Add(BtnPreviewStart);

            pt.X += BtnPreviewStart.Width + sSpacer.Width;

            LblTrack.Text = string.Format("Track {0}", 1);
            LblTrack.Width = sForm.Width - 2 * (sOff.Width + BtnPreviewStart.Width + sSpacer.Width);
            LblTrack.Height = fatButtonHeight;
            LblTrack.TextAlign = ContentAlignment.MiddleCenter;
            LblTrack.Location = pt;
            LblTrack.BackColor = Color.LightGray;
            LblTrack.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(LblTrack);

            pt.X += sSpacer.Width + LblTrack.Width;

            BtnPreviewEnd.Width = BtnPreviewStart.Width;
            BtnPreviewEnd.Height = fatButtonHeight;
            BtnPreviewEnd.TabStop = true;
            BtnPreviewEnd.Text = "End";
            _toolTip.SetToolTip(BtnPreviewEnd, "Keyboard: End");
            BtnPreviewEnd.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewEnd.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewEnd.Image = Image.FromStream(stream); });
            BtnPreviewEnd.Location = pt;
            Controls.Add(BtnPreviewEnd);

            // NEW ROW
            pt.X = sOff.Width;
            pt.Y += BtnPreviewEnd.Height + sSpacer.Height;

            BtnPrevious.TabStop = true;
            BtnPrevious.Width = 120;
            BtnPrevious.Text = "<< Previous Track";
            _toolTip.SetToolTip(BtnPrevious, "Keyboard: Ctrl + Left");
            BtnPrevious.TextAlign = ContentAlignment.MiddleCenter;
            BtnPrevious.Location = pt;
            Controls.Add(BtnPrevious);

            BtnDelete.TabStop = true;
            BtnDelete.Width = 80;
            BtnDelete.Text = "DELETE";
            _toolTip.SetToolTip(BtnDelete, "Keyboard: Del");
            BtnDelete.ForeColor = Color.Crimson;
            BtnDelete.Font = new Font(BtnDelete.Font.Name, BtnDelete.Font.Size, FontStyle.Bold);
            BtnDelete.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(BtnDelete);

            int bottomRowGapWidth = (lblPrompt.Width - (2 * BtnPrevious.Width) - BtnDelete.Width) / 2;
            pt.X += bottomRowGapWidth + BtnPrevious.Width;

            BtnDelete.Location = pt;

            pt.X += bottomRowGapWidth + BtnDelete.Width;

            BtnNext.Width = BtnPrevious.Width;
            BtnNext.TabStop = true;
            BtnNext.Text = "Next Track >>";
            _toolTip.SetToolTip(BtnNext, "Keyboard: Ctrl + Right");
            BtnNext.TextAlign = ContentAlignment.MiddleCenter;
            BtnNext.Location = pt;
            Controls.Add(BtnNext);

            // NEW ROW
            pt.Y += BtnNext.Height + sSpacer.Height;
            pt.X = sOff.Width;

            BtnAddTrack.TabStop = true;
            BtnAddTrack.Height = fatButtonHeight;
            BtnAddTrack.Width = BtnPreviewAll.Width;
            BtnAddTrack.Text = "Add Track From Selection";
            BtnAddTrack.TextAlign = ContentAlignment.MiddleCenter;
            BtnAddTrack.Location = pt;
            Controls.Add(BtnAddTrack);
        }
    }
}
