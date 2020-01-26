using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksForm : Form
    {
        private const int LblHeight = 16;
        private const int FatButtonHeight = 25;
        protected internal Button BtnPreviewAll = new Button();
        protected internal Button BtnStopPreview = new Button();
        protected internal Button BtnPreviewStart = new Button();
        protected internal Button BtnPreviewEnd = new Button();
        protected internal Button BtnPrevious = new Button();
        protected internal Button BtnNext = new Button();
        protected internal Button BtnDelete = new Button();
        protected internal Button BtnAddTrackBefore = new Button();
        protected internal Button BtnAddTrackAfter = new Button();
        protected internal Label LblTrack = new Label();

        protected internal Button BtnMoveStartMinus = new Button();
        protected internal Label LblMoveStart = new Label();
        protected internal Button BtnMoveStartPlus = new Button();

        protected internal Button BtnMoveEndMinus = new Button();
        protected internal Label LblMoveEnd = new Label();
        protected internal Button BtnMoveEndPlus = new Button();

        protected internal Button BtnMoveFadeInMinus = new Button();
        protected internal Label LblMoveFadeIn = new Label();
        protected internal Button BtnMoveFadeInPlus = new Button();

        protected internal Button BtnMoveFadeOutMinus = new Button();
        protected internal Label LblMoveFadeOut = new Label();
        protected internal Button BtnMoveFadeOutPlus = new Button();

        private readonly Color LabelBgColor = Color.FromArgb(17, Color.Black);
        private ToolTip _toolTip;
        private Point _pt = new Point(10, 10);
        private Size _sOff = new Size(10, 10);
        private Size _sSpacer = new Size(10, 15);
        private Size _sForm = new Size(520, 360);

        public EditTracksForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = _sForm;
            AutoScroll = false;
            KeyPreview = true;

            _toolTip = new ToolTip();

            Label lblPrompt = new Label();
            lblPrompt.Text = "Adjust track region definitions - click OK to apply changes.";
            lblPrompt.Width = _sForm.Width - _pt.X - _sOff.Width;
            lblPrompt.Height = LblHeight;
            lblPrompt.Location = _pt;
            Controls.Add(lblPrompt);

            _pt.Y += lblPrompt.Height + _sSpacer.Height;

            const string previewIcon = "SoundForgeScriptsLib.Resources.ionicons_android-volume-up_17.png";

            BtnPreviewAll.TabStop = true;
            BtnPreviewAll.Width = (lblPrompt.Width - _sSpacer.Width) / 2;
            BtnPreviewAll.Height = FatButtonHeight;
            BtnPreviewAll.Text = "Preview All";
            BtnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            BtnPreviewAll.Location = _pt;
            BtnPreviewAll.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewAll.TextAlign = ContentAlignment.MiddleCenter;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewAll.Image = Image.FromStream(stream); });
            Controls.Add(BtnPreviewAll);

            _pt.X += _sSpacer.Width + BtnPreviewAll.Width;

            BtnStopPreview.TabStop = true;
            BtnStopPreview.Height = FatButtonHeight;
            BtnStopPreview.Width = BtnPreviewAll.Width;
            BtnStopPreview.Text = "Stop Preview";
            _toolTip.SetToolTip(BtnStopPreview, "Keyboard: Space");
            BtnStopPreview.TextAlign = ContentAlignment.MiddleCenter;
            BtnStopPreview.Location = _pt;
            Controls.Add(BtnStopPreview);

            // NEW ROW
            _pt.Y += BtnStopPreview.Height + _sSpacer.Height;
            _pt.X = _sOff.Width;

            BtnPreviewStart.TabStop = true;
            BtnPreviewStart.Text = "Start";
            _toolTip.SetToolTip(BtnPreviewStart, "Keyboard: Home");
            BtnPreviewStart.Height = FatButtonHeight;
            BtnPreviewStart.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewStart.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewStart.Image = Image.FromStream(stream); });
            BtnPreviewStart.Location = _pt;
            Controls.Add(BtnPreviewStart);

            _pt.X += BtnPreviewStart.Width + _sSpacer.Width;

            LblTrack.Text = string.Format("Track {0}", 1);
            LblTrack.Width = _sForm.Width - 2 * (_sOff.Width + BtnPreviewStart.Width + _sSpacer.Width);
            LblTrack.Height = FatButtonHeight;
            LblTrack.TextAlign = ContentAlignment.MiddleCenter;
            LblTrack.Location = _pt;
            LblTrack.BackColor = LabelBgColor;
            LblTrack.BorderStyle = BorderStyle.FixedSingle;
            LblTrack.BorderStyle = BorderStyle.None;
            Controls.Add(LblTrack);

            _pt.X += _sSpacer.Width + LblTrack.Width;

            BtnPreviewEnd.Width = BtnPreviewStart.Width;
            BtnPreviewEnd.Height = FatButtonHeight;
            BtnPreviewEnd.TabStop = true;
            BtnPreviewEnd.Text = "End";
            _toolTip.SetToolTip(BtnPreviewEnd, "Keyboard: End");
            BtnPreviewEnd.ImageAlign = ContentAlignment.MiddleLeft;
            BtnPreviewEnd.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate (Stream stream) { BtnPreviewEnd.Image = Image.FromStream(stream); });
            BtnPreviewEnd.Location = _pt;
            Controls.Add(BtnPreviewEnd);

            // NEW ROW
            _pt.X = _sOff.Width;
            _pt.Y += BtnPreviewEnd.Height + _sSpacer.Height;

            BtnPrevious.TabStop = true;
            BtnPrevious.Width = 120;
            BtnPrevious.Text = "<< Previous Track";
            _toolTip.SetToolTip(BtnPrevious, "Keyboard: Ctrl + Left");
            BtnPrevious.TextAlign = ContentAlignment.MiddleCenter;
            BtnPrevious.Location = _pt;
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
            _pt.X += bottomRowGapWidth + BtnPrevious.Width;

            BtnDelete.Location = _pt;

            _pt.X += bottomRowGapWidth + BtnDelete.Width;

            BtnNext.Width = BtnPrevious.Width;
            BtnNext.TabStop = true;
            BtnNext.Text = "Next Track >>";
            _toolTip.SetToolTip(BtnNext, "Keyboard: Ctrl + Right");
            BtnNext.TextAlign = ContentAlignment.MiddleCenter;
            BtnNext.Location = _pt;
            Controls.Add(BtnNext);

            // NEW ROW
            _pt.Y += BtnNext.Height + _sSpacer.Height;
            _pt.X = _sOff.Width;

            BtnAddTrackBefore.TabStop = true;
            BtnAddTrackBefore.Height = FatButtonHeight;
            BtnAddTrackBefore.Width = BtnPreviewAll.Width;
            BtnAddTrackBefore.Text = "Insert Track Before";
            _toolTip.SetToolTip(BtnNext, "Keyboard: B");
            BtnAddTrackBefore.TextAlign = ContentAlignment.MiddleCenter;
            BtnAddTrackBefore.Location = _pt;
            Controls.Add(BtnAddTrackBefore);

            _pt.X += BtnAddTrackBefore.Width + _sOff.Width;

            BtnAddTrackAfter.TabStop = true;
            BtnAddTrackAfter.Height = FatButtonHeight;
            BtnAddTrackAfter.Width = BtnPreviewAll.Width;
            BtnAddTrackAfter.Text = "Insert Track After";
            _toolTip.SetToolTip(BtnNext, "Keyboard: A");
            BtnAddTrackAfter.TextAlign = ContentAlignment.MiddleCenter;
            BtnAddTrackAfter.Location = _pt;
            Controls.Add(BtnAddTrackAfter);

            // NEW ROW
            //TODO: add ++/-- buttons, click handlers, key bindings
            _pt.Y += BtnAddTrackAfter.Height + _sSpacer.Height;
            _pt.X = _sOff.Width;

            DoPlusMinusControlLayout(BtnMoveStartMinus, BtnMoveStartPlus, LblMoveStart, BtnPreviewAll.Width);
            LblMoveStart.Text = "Start";
            _toolTip.SetToolTip(BtnMoveStartMinusMinus, "Keyboard: J");
            _toolTip.SetToolTip(BtnMoveStartPlusPlus, "Keyboard: K");
            _toolTip.SetToolTip(BtnMoveStartMinus, "Keyboard: j");
            _toolTip.SetToolTip(BtnMoveStartPlus, "Keyboard: k");

            DoPlusMinusControlLayout(BtnMoveEndMinus, BtnMoveEndPlus, LblMoveEnd, BtnPreviewAll.Width);
            LblMoveEnd.Text = "End";
            _toolTip.SetToolTip(BtnMoveEndMinusMinus, "Keyboard: H");
            _toolTip.SetToolTip(BtnMoveEndPlusPlus, "Keyboard: L");
            _toolTip.SetToolTip(BtnMoveEndMinus, "Keyboard: h");
            _toolTip.SetToolTip(BtnMoveEndPlus, "Keyboard: l");

            // NEW ROW
            _pt.Y += BtnMoveEndMinus.Height + _sSpacer.Height;
            _pt.X = _sOff.Width;

            DoPlusMinusControlLayout(BtnMoveFadeInMinus, BtnMoveFadeInPlus, LblMoveFadeIn, BtnPreviewAll.Width);
            LblMoveFadeIn.Text = "FadeIn";
            _toolTip.SetToolTip(BtnMoveFadeInMinusMinus, "Keyboard: U");
            _toolTip.SetToolTip(BtnMoveFadeInPlusPlus, "Keyboard: I");
            _toolTip.SetToolTip(BtnMoveFadeInMinus, "Keyboard: u");
            _toolTip.SetToolTip(BtnMoveFadeInPlus, "Keyboard: i");

            DoPlusMinusControlLayout(BtnMoveFadeOutMinus, BtnMoveFadeOutPlus, LblMoveFadeOut, BtnPreviewAll.Width);
            LblMoveFadeOut.Text = "FadeOut";
            _toolTip.SetToolTip(BtnMoveFadeOutMinusMinus, "Keyboard: Y");
            _toolTip.SetToolTip(BtnMoveFadeOutPlusPlus, "Keyboard: O");
            _toolTip.SetToolTip(BtnMoveFadeOutMinus, "Keyboard: y");
            _toolTip.SetToolTip(BtnMoveFadeOutPlus, "Keyboard: o");
        }

        private void DoPlusMinusControlLayout(Button minusButton, Button plusButton, Label label, int groupWidth)
        {
            //TODO: add ++/-- buttons, click handlers, key bindings
            int groupSpacerWidth = _sSpacer.Width / 2;
            int controlsMinusSpacingWidth = groupWidth - (2 * groupSpacerWidth);
            int btnWidth = (int)Math.Floor(controlsMinusSpacingWidth * 0.15f);
            int lblWidth = controlsMinusSpacingWidth - (2 * btnWidth);

            foreach (Button btn in new Button[] { minusButton, plusButton })
            {
                btn.TabStop = true;
                btn.Height = FatButtonHeight;
                btn.Width = btnWidth;
                btn.TextAlign = ContentAlignment.MiddleCenter;
                Controls.Add(btn);
            }

            minusButton.Location = _pt;
            minusButton.Text = "-";
            _pt.X += minusButton.Width + groupSpacerWidth;

            label.Location = _pt;
            label.Width = lblWidth;
            label.Height = FatButtonHeight;
            label.BackColor = LabelBgColor;
            label.BorderStyle = BorderStyle.None;
            label.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(label);
            _pt.X += label.Width + groupSpacerWidth;

            plusButton.Location = _pt;
            plusButton.Text = "+";
            _pt.X += plusButton.Width + _sSpacer.Width;
        }
    }
}
