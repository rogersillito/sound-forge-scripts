/* =======================================================================================================
*	Script Name: Vinyl Rip 3 - Save Track Files
*	Description: Saves track regions found in "Vinyl Rip 1 - Find Tracks" to file
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

namespace SoundForgeScripts.Scripts.VinylRip3FinalTrackProcessing
{
    [ScriptName("Vinyl Rip 3 - Save Track Files")]
    public class EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;

        private TextBox _tbxAlbum;
        private TextBox _tbxArtist;
        private TextBox _tbxRootFolder;
        private VinylRipOptions _vinylRipOptions;
        private SfAudioSelection _noiseprintSelection;
        private string _outputDirectory;
        private FileTasks _fileTasks;
        private SplitTrackList _splitTrackList;
        private ICreateTrackMarkerNames _trackMarkerNameBuilder;

        private const string FinalCleaningPreset = "Vinyl Processing (Final)";
        private const string DefaultRootLibraryFolder = @"F:\My Music\From Vinyl\";

        //TODO: come up with a way of saving settings between script runs, and between the 2 scripts (save options to json?)
        protected override void Execute()
        {
            _file = App.CurrentFile;

            _fileTasks = new FileTasks(_file);
            _trackMarkerNameBuilder = new TrackMarkerNameBuilder();
            _fileTasks.EnforceStereoFileOpen();

            FileMarkersWrapper markers = new FileMarkersWrapper(_file);
            TrackMarkerFactory markerAndRegionFactory = new TrackMarkerFactory(markers);
            _splitTrackList = new SplitTrackList(markerAndRegionFactory, markerAndRegionFactory, _trackMarkerNameBuilder, markers, new TrackMarkerSpecifications(), Output);
            const int noiseprintLengthSeconds = 2;
            _noiseprintSelection = _fileTasks.EnforceNoisePrintSelection(App, noiseprintLengthSeconds);
            _vinylRipOptions = new VinylRipOptions();
            _vinylRipOptions.TrackAddFadeOutLengthInSeconds = 3;
            _vinylRipOptions.TrackFadeInLengthInSamples = 20;

            DialogResult result = ConfirmTrackSplitsForm(Script.Application.Win32Window);
            if (result == DialogResult.Cancel)
                return;

            DoFinalAudioClean();
            Directory.CreateDirectory(_outputDirectory);
            SplitTrackList tracks = GetSplitTrackDefinitions();
            DoTrackSplitting(tracks);
        }

        private void CleanVinylRecording(string presetName, int noiseReductionPasses, SfAudioSelection noiseprintSelection)
        {
            SfAudioSelection selection = _fileTasks.SelectAll();
            _fileTasks.ApplyEffectPreset(App, selection, EffectNames.ClickAndCrackleRemoval, presetName, EffectOptions.EffectOnly, Output.ToScriptWindow);
            for (int i = 1; i <= noiseReductionPasses; i++)
            {
                Output.ToScriptWindow("Noise Reduction (pass #{0})", i);
                EffectOptions noiseReductionOption = EffectOptions.EffectOnly;
                //if (i == 1)
                //    noiseReductionOption = EffectOptions.DialogFirst;
                _fileTasks.CopySelectionToStart(App, noiseprintSelection);
                _fileTasks.ApplyEffectPreset(App, selection, EffectNames.NoiseReduction, presetName, noiseReductionOption, Output.ToScriptWindow);
                _file.Window.SetSelectionAndScroll(0, _noiseprintSelection.Length, DataWndScrollTo.NoMove);
                App.DoMenuAndWait("Edit.Delete", false);
            }
            Output.LineBreakToScriptWindow();
        }

        #region Results Form
        public DialogResult ConfirmTrackSplitsForm(IWin32Window hOwner)
        {
            Form dlg = new Form();
            Size sForm = new Size(520, 450);

            dlg.Text = ScriptTitle;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ClientSize = sForm;

            Point pt = new Point(10, 10);
            Size sOff = new Size(10, 10);
            Size sSpacer = new Size(10, 5);
            const int lblHeight = 16;
            const int tbxHeight = 16;

            Label lblRootFolder = new Label();
            lblRootFolder.Text = "Root Library Folder Path:";
            lblRootFolder.Width = sForm.Width - pt.X - sOff.Width;
            lblRootFolder.Height = lblHeight;
            lblRootFolder.Location = pt;
            dlg.Controls.Add(lblRootFolder);
            pt.Y += lblRootFolder.Height;

            _tbxRootFolder = new TextBox();
            _tbxRootFolder.TabStop = true;
            _tbxRootFolder.TabIndex = 1;
            _tbxRootFolder.Width = sForm.Width - pt.X - sOff.Width;
            _tbxRootFolder.Height = tbxHeight;
            _tbxRootFolder.Location = pt;
            _tbxRootFolder.Text = DefaultRootLibraryFolder;
            dlg.Controls.Add(_tbxRootFolder);
            pt.Y += _tbxRootFolder.Height + sSpacer.Height;

            Label lblArtist = new Label();
            lblArtist.Text = "Artist Folder Name:";
            lblArtist.Width = sForm.Width - pt.X - sOff.Width;
            lblArtist.Height = lblHeight;
            lblArtist.Location = pt;
            dlg.Controls.Add(lblArtist);
            pt.Y += lblArtist.Height;

            _tbxArtist = new TextBox();
            _tbxArtist.TabStop = true;
            _tbxArtist.TabIndex = 2;
            _tbxArtist.Width = sForm.Width - pt.X - sOff.Width;
            _tbxArtist.Height = tbxHeight;
            _tbxArtist.Location = pt;
            dlg.Controls.Add(_tbxArtist);
            pt.Y += _tbxArtist.Height + sSpacer.Height;

            Label lblAlbum = new Label();
            lblAlbum.Text = "Album Folder Name:";
            lblAlbum.Width = sForm.Width - pt.X - sOff.Width;
            lblAlbum.Height = lblHeight;
            lblAlbum.Location = pt;
            dlg.Controls.Add(lblAlbum);
            pt.Y += lblAlbum.Height;

            _tbxAlbum = new TextBox();
            _tbxAlbum.TabStop = true;
            _tbxAlbum.TabIndex = 3;
            _tbxAlbum.Width = sForm.Width - pt.X - sOff.Width;
            _tbxAlbum.Height = tbxHeight;
            _tbxAlbum.Location = pt;
            dlg.Controls.Add(_tbxAlbum);
            pt.Y += _tbxAlbum.Height + sOff.Height;

            // we position the buttons relative to the bottom and left of the form.
            pt = (Point)dlg.ClientSize;
            pt -= sOff;

            Button btn = new Button();
            btn.TabStop = true;
            btn.TabIndex = 5;
            pt -= btn.Size;
            btn.Text = "Cancel";
            btn.Location = pt;
            btn.Click += FormHelper.OnCancel_Click;
            dlg.Controls.Add(btn);
            dlg.CancelButton = btn;
            pt.X -= (btn.Width + 10);

            btn = new Button();
            btn.TabStop = true;
            btn.TabIndex = 4;
            btn.Text = "OK";
            btn.Location = pt;
            btn.Click += OnTrackSplitsConfirmed;
            dlg.Controls.Add(btn);
            dlg.AcceptButton = btn;
            pt.X -= (btn.Width + 10);

            // position prompt beside buttons
            pt = new Point(sOff.Width, dlg.ClientSize.Height - sSpacer.Height - btn.Size.Height);

            Label lblPrompt = new Label();
            lblPrompt.Text = "Press OK to apply final processing and split tracks...";
            lblPrompt.Width = sForm.Width - pt.X - sOff.Width;
            lblPrompt.Height = lblHeight;
            lblPrompt.Location = pt;
            dlg.Controls.Add(lblPrompt);
            pt.Y -= lblPrompt.Height;

            dlg.ShowDialog(hOwner);
            return dlg.DialogResult;
        }

        private bool ValidatePathDetails()
        {
            if (!Directory.Exists(_tbxRootFolder.Text))
            {
                Output.ToMessageBox(MessageBoxIcon.Error, "Root library folder path does not exist.");
                return false;
            }
            string album = GetPathSafeSegmentName(_tbxAlbum.Text);
            string artist = GetPathSafeSegmentName(_tbxArtist.Text);
            string path = Path.Combine(Path.Combine(_tbxRootFolder.Text, artist), album);
            if (Directory.Exists(path))
            {
                Output.ToMessageBox(MessageBoxIcon.Error, "Output path already exists: \"{0}\"", path);
                return false;
            }
            _outputDirectory = path;
            return true;
        }

        private void OnTrackSplitsConfirmed(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            Form form = (Form)btn.Parent;
            if (!ValidatePathDetails())
                return;
            form.DialogResult = DialogResult.OK;
            form.Close();
        }
        #endregion // Results Form

        public static string GetPathSafeSegmentName(string name)
        {
            const char replChar = ';';
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, replChar);
            }
            return name.Replace(replChar + "+", replChar.ToString());
        }

        private void DoFinalAudioClean()
        {
            _fileTasks.ZoomOutFull();
            _fileTasks.ApplyEffectPreset(App, _fileTasks.SelectAll(), "Sony Paragraphic EQ", "Remove very low and inaudible frequencies below 20 Hz", EffectOptions.EffectOnly, Output.ToScriptWindow);
            CleanVinylRecording(FinalCleaningPreset, 2, _noiseprintSelection);
            _fileTasks.ApplyEffectPreset(App, _fileTasks.SelectAll(), "Sony Paragraphic EQ", FinalCleaningPreset, EffectOptions.EffectOnly, Output.ToScriptWindow);
            _fileTasks.ApplyEffectPreset(App, _fileTasks.SelectAll(), "Normalize", "Maxixmize peak value", EffectOptions.EffectOnly, Output.ToScriptWindow);
            Output.LineBreakToScriptWindow();
        }

        private SplitTrackList GetSplitTrackDefinitions()
        {
            long fadeOutLengthSamples = _file.SecondsToPosition(_vinylRipOptions.TrackAddFadeOutLengthInSeconds);
            return _splitTrackList.InitTracks(_vinylRipOptions.TrackFadeInLengthInSamples, fadeOutLengthSamples);
        }

        private void DoTrackSplitting(SplitTrackList tracks)
        {
            //App.FindRenderer("FLAC Audio", "flac");
            //foreach (ISfRenderer r in App.Renderers)
            //{
            //    Output.LineBreakToScriptWindow();
            //    Output.ToScriptWindow(r.Name);
            //    Output.ToScriptWindow(r.Extension);
            //    Output.ToScriptWindow(r.Guid);
            //    foreach (ISfGenericPreset preset in r.Templates)
            //    {
            //        Output.ToScriptWindow(preset.Name);
            //    }
            //}    
            //tracks.Clear();
            foreach (SplitTrackDefinition track in tracks)
            {
                ISfFileHost trackFile = _file.NewFile(track.GetSelectionWithFades());
                trackFile.Markers.Clear();
                trackFile.Summary.Album = _tbxAlbum.Text;
                trackFile.Summary.Artist = _tbxArtist.Text;
                trackFile.Summary.TrackNo = string.Concat(track.Number, "/", tracks.Count);

                FileTasks trackTasks = new FileTasks(trackFile);
                if (track.AddFadeIn)
                {
                    Output.ToScriptWindow("Track {0}: Fade In {1} Samples", track.Number, track.FadeInLength);
                    trackFile.Window.SetSelectionAndScroll(0, track.FadeInLength, DataWndScrollTo.NoMove);
                    App.DoMenuAndWait("Process.FadeIn", false);
                }
                if (track.AddFadeOut)
                {
                    Output.ToScriptWindow("Track {0}: Fade Out {1} Samples", track.Number, track.FadeOutLength);
                    trackFile.Window.SetSelectionAndScroll(track.FadeOutStartPosition, trackFile.Length, DataWndScrollTo.NoMove);
                    App.DoMenuAndWait("Process.FadeOut", false);
                }
                trackTasks.ApplyEffectPreset(App, trackTasks.SelectAll(), "iZotope MBIT+ Dither", "Convert to 16 bit (advanced light dither)", EffectOptions.EffectOnly, Output.ToScriptWindow);

                string savePath = string.Concat(_outputDirectory, Path.DirectorySeparatorChar, _trackMarkerNameBuilder.GetRegionMarkerName(track.Number) + ".flac");
                trackFile.SaveAs(savePath, "FLAC Audio", "44,100 Hz, 16 Bit, Stereo", RenderOptions.SaveMetadata);
                trackFile.Close(CloseOptions.QuerySaveIfChanged);

                Output.ToScriptWindow("Saved '{0}'", savePath);
                Output.LineBreakToScriptWindow();
            }
        }
    }
}

