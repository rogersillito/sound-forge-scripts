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
            lblPrompt.BackColor = Color.Aqua;
            dlg.Controls.Add(lblPrompt);

            pt.Y += lblPrompt.Height + sSpacer.Height;

            foreach (SplitTrackDefinition track in tracks)
            {

                Button btn = new Button();
                btn.TabStop = true;
                //btn.TabIndex = 4;
                btn.Text = string.Format("Track {0}", track.Number);
                btn.Location = pt;
                dlg.Controls.Add(btn);
                //btn.Click += FormHelper.OnOK_Click;

                pt.Y += btn.Height + sSpacer.Height;

            }
            //dlg.AcceptButton = btn;
            dlg.Show(hOwner);
        }

        #endregion // Edit Form
    }
}
