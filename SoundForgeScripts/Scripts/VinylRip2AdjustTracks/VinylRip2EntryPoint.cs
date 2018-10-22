/* =======================================================================================================
 *	Script Name: Vinyl Rip 2 - Adjust Tracks
*	Description: Allows editing of track regions found in "Vinyl Rip 1 - Find Tracks"
 *
 *	Initial State: Run with a file open a selection containing at least 2 seconds of track noise
 *	
 *	Parameters (Args):
 *		None
 *
 *	Output:
 *	    None
 *
 * ==================================================================================================== */

using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SoundForge;
using SoundForgeScriptsLib;
using SoundForgeScriptsLib.EntryPoints;
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    [ScriptName("Vinyl Rip 2 - Adjust Tracks")]
    public class EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;

        private FindTracksOptions _findTracksOptions;
        private FileTasks _fileTasks;
        private SplitTrackList _splitTrackList;

        protected override void Execute()
        {
            _file = App.CurrentFile;
            ISfFileHost file = _file;

            _fileTasks = new FileTasks(file);
            _fileTasks.EnforceStereoFileOpen();
            _fileTasks.ZoomOutFull();

            _splitTrackList = new SplitTrackList(_file);

            //TODO: initial dialog to configure these:
            _findTracksOptions = new FindTracksOptions();
            _findTracksOptions.TrackAddFadeOutLengthInSeconds = 3;
            _findTracksOptions.TrackFadeInLengthInSamples = 20;

            SplitTrackList tracks = GetSplitTrackDefinitions();
            ConfirmTrackSplitsForm(App.Win32Window, tracks);
        }

        private SplitTrackList GetSplitTrackDefinitions()
        {
            long fadeOutLengthSamples = _file.SecondsToPosition(_findTracksOptions.TrackAddFadeOutLengthInSeconds);
            SplitTrackList tracks = _splitTrackList.InitTracks(_findTracksOptions.TrackFadeInLengthInSamples, fadeOutLengthSamples);
            Output.ToScriptWindow("Found {0} tracks:", tracks.Count);
            foreach (SplitTrackDefinition track in tracks)
            {
                Output.ToScriptWindow("{0}:\t{1}\t{2}\t(Start fade @ {3})", track.Number,
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.Selection.Start)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.Selection.Length)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.FadeOutStartPosition)));
                
            }
            Output.LineBreakToScriptWindow();
            return tracks;
        }
        
        #region Edit Form
        public void ConfirmTrackSplitsForm(IWin32Window hOwner, SplitTrackList tracks)
        {
            //TODO: I think it's ok, but check it's ok to interact with the file window while the script window is active...
            //TODO: and figure out the layout...!
            Form dlg = new Form();
            Size sForm = new Size(520, 450);

            dlg.Text = ScriptTitle;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = true;
            dlg.MinimizeBox = true;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ClientSize = sForm;
            dlg.AutoScroll = true;

            Point pt = new Point(10, 10);
            Size sOff = new Size(10, 10);
            Size sSpacer = new Size(10, 5);
            const int lblHeight = 16;
            //const int tbxHeight = 16;

            Label lblPrompt = new Label();
            lblPrompt.Text = "Adjust track region definitions - click OK to apply changes.";
            lblPrompt.Width = sForm.Width - pt.X - sOff.Width;
            lblPrompt.Height = lblHeight;
            lblPrompt.Location = pt;
            Output.ToScriptWindow("pt.y {0}", pt.Y);
            //lblPrompt.BackColor = Color.Aqua;
            dlg.Controls.Add(lblPrompt);

            pt.Y += lblPrompt.Height + sSpacer.Height;

            const string previewIcon = "SoundForgeScriptsLib.Resources.ionicons_android-volume-up_17.png";

            Button btnPrvwStart = new Button();
            btnPrvwStart.TabStop = true;
            //btnPrvwStart.TabIndex = 4;
            btnPrvwStart.Text = "Start";
            btnPrvwStart.Height = 25;
            btnPrvwStart.ImageAlign = ContentAlignment.MiddleLeft;
            btnPrvwStart.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate(Stream stream) { btnPrvwStart.Image = Image.FromStream(stream); });
            btnPrvwStart.Location = pt;
            dlg.Controls.Add(btnPrvwStart);
            //btnPrvwStart.Click += FormHelper.OnOK_Click;

            //pt.Y += btnPrvwStart.Height + sSpacer.Height;
            pt.X += btnPrvwStart.Width + sSpacer.Width;

            Label lblTrack = new Label();
            lblTrack.Text = string.Format("Track {0}", 1);
            lblTrack.Width = sForm.Width - 2 * (sOff.Width + btnPrvwStart.Width + sSpacer.Width);
            lblTrack.Height = btnPrvwStart.Height;
            lblTrack.TextAlign = ContentAlignment.MiddleCenter;
            lblTrack.Location = pt;
            Output.ToScriptWindow("pt.y {0}", pt.Y);
            lblTrack.BackColor = Color.LightGray;
            lblTrack.BorderStyle = BorderStyle.FixedSingle;
            dlg.Controls.Add(lblTrack);

            pt.X += sSpacer.Width + lblTrack.Width;

            Button btnPrvwEnd = new Button();
            btnPrvwEnd.Width = btnPrvwStart.Width;
            btnPrvwEnd.Height = btnPrvwStart.Height;
            btnPrvwEnd.TabStop = true;
            //btnPrvwEnd.TabIndex = 4;
            btnPrvwEnd.Text = "End";
            btnPrvwEnd.ImageAlign = ContentAlignment.MiddleLeft;
            btnPrvwEnd.TextAlign = ContentAlignment.MiddleRight;
            ResourceHelper.GetResourceStream(previewIcon, delegate(Stream stream) { btnPrvwEnd.Image = Image.FromStream(stream); });
            btnPrvwEnd.Location = pt;
            dlg.Controls.Add(btnPrvwEnd);
            //btnPrvwEnd.Click += FormHelper.OnOK_Click;

            pt.X = sOff.Width;
            pt.Y += btnPrvwEnd.Height + sSpacer.Height;

            Button btnPrevious = new Button();
            //btnNext.Width = btnPrvwStart.Width;
            btnPrevious.TabStop = true;
            //btnNext.TabIndex = 4;
            btnPrevious.Width = 120; 
            btnPrevious.Text = "<< Previous Track";
            btnPrevious.TextAlign = ContentAlignment.MiddleCenter;
            btnPrevious.Location = pt;
            dlg.Controls.Add(btnPrevious);

            pt.X += sSpacer.Width + btnPrevious.Width;

            Button btnNext = new Button();
            btnNext.Width = btnPrevious.Width;
            btnNext.TabStop = true;
            //btnNext.TabIndex = 4;
            btnNext.Text = "Next Track >>";
            btnNext.TextAlign = ContentAlignment.MiddleCenter;
            btnNext.Location = pt;
            dlg.Controls.Add(btnNext);
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
            dlg.Show(hOwner);
        }

        #endregion // Edit Form
    }
}
