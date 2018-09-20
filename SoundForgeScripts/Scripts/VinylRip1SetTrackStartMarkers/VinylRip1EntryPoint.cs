/* =======================================================================================================
 *	Script Name: Vinyl Rip - 1 Set Track Start Markers
 *	Description: Inserts track start points (after aggresively cleaning audio).
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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SoundForge;
using SoundForgeScriptsLib;
using SoundForgeScriptsLib.EntryPoints;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    [ScriptName("Vinyl Rip - 1 Set Track Start Markers")]
    public class VinylRip1EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;

        private TextBox _tbxAlbum;
        private TextBox _tbxArtist;
        private TextBox _tbxRootFolder;
        private FindTracksOptions _findTracksOptions;
        private SfAudioSelection _noiseprintSelection;
        private TrackList _trackList;
        private string _outputDirectory;
        private FileTasks _fileTasks;

        private const string AggressiveCleaningPreset = "Vinyl Processing (Pre-Track Splitting)";
        private const string FinalCleaningPreset = "Vinyl Processing (Final)";
        private const string DefaultRootLibraryFolder = @"F:\My Music\From Vinyl\";
        private const string TrackRegionPrefix = @"__TRACK__";

        //TODO: clear console
        //TODO: address this! http://forum-archive.magix.info/showmessage.asp?messageid=505510
        //TODO: remove duplication between the 2 vinyl scripts
        //TODO: remove stuff now obsolete in this script (but used to vinyl rip 2)
        //TODO: move stuff used by both scripts to Lib
        //TODO: come up with a way of saving settings between script runs, and between the 2 scripts (save options to json?)
        protected override void Execute()
            //{
            //    _file = App.CurrentFile;

            //    if (_file == null || _file.Channels != 2)
            //    {
            //        throw new ScriptAbortedException("A stereo file must be open before this script can be run.");
            //    }
            //    const int noiseprintLengthSeconds = 2;
            //    _fileTasks = new FileTasks(_file);
            //    _noiseprintSelection = _fileTasks.PromptNoisePrintSelection(App, noiseprintLengthSeconds);
            //    _findTracksOptions = new FindTracksOptions();
            //    _findTracksOptions.ScanWindowLengthInSeconds = 1.0;
            //    _findTracksOptions.GapNoisefloorThresholdInDecibels = -70;
            //    _findTracksOptions.MinimumTrackGapInSeconds = 1;
            //    _findTracksOptions.MinimumTrackLengthInSeconds = 10;
            //    _findTracksOptions.StartScanFilePositionInSamples = _file.SecondsToPosition(noiseprintLengthSeconds);
            //    _findTracksOptions.TrackAddFadeOutLengthInSeconds = 3;
            //    _findTracksOptions.TrackFadeInLengthInSamples = 20;
            //    ConfirmTrackSplitsForm(Script.Application.Win32Window);
            //    DoFinalAudioClean();
            //    List<SplitTrackDefinition> tracks = GetSplitTrackDefinitions();
            //    _tbxAlbum = new TextBox();
            //    _tbxArtist = new TextBox();
            //    _tbxRootFolder = new TextBox();
            //    _tbxRootFolder.Text = "F:\\My Music\\From Vinyl";
            //    _tbxAlbum.Text = "Les Fleur";
            //    _tbxArtist.Text = "4 Hero";
            //    ValidatePathDetails();
            //    DoTrackSplitting(tracks);
            //}
            //protected void Execute2()
        {
            _file = App.CurrentFile;

            if (_file == null || _file.Channels != 2)
            {
                throw new ScriptAbortedException("A stereo file must be open before this script can be run.");
            }
            _fileTasks = new FileTasks(_file);
            _fileTasks.ZoomOutFull();

            //TODO: retain marker positions in script and undo NOT working!
            //_file.UndosAreEnabled = true;
            //int undoId = _file.BeginUndo("PrepareAudio");
            //_file.EndUndo(undoId, true); 

            const int noiseprintLengthSeconds = 2;
            _noiseprintSelection = _fileTasks.PromptNoisePrintSelection(App, noiseprintLengthSeconds);
            //TODO: add marker for noiseprint so it can be reused?
            _file.Markers.Add(new SfAudioMarker(_noiseprintSelection));
            CleanVinylRecording(AggressiveCleaningPreset, 3, _noiseprintSelection); //TODO: configure number of noise reduction passes?

            //TODO: initial dialog to configure these:
            _findTracksOptions = new FindTracksOptions();
            _findTracksOptions.ScanWindowLengthInSeconds = 1.0;
            _findTracksOptions.GapNoisefloorThresholdInDecibels = -70;
            _findTracksOptions.MinimumTrackGapInSeconds = 1;
            _findTracksOptions.MinimumTrackLengthInSeconds = 10;
            _findTracksOptions.StartScanFilePositionInSamples = _file.SecondsToPosition(noiseprintLengthSeconds);
            _findTracksOptions.TrackAddFadeOutLengthInSeconds = 1.8;
            _findTracksOptions.TrackFadeInLengthInSamples = 30;

            _trackList = FindTracks(App, _file);

            App.DoMenuAndWait("Edit.UndoAll", false);

            foreach (TrackDefinition track in _trackList)
            {
                _file.Markers.AddRegion(track.StartPosition, track.Length, CreateMarkerName(track.Number));
            }

            //TODO: temporarily exit here!
            //ConfirmTrackSplitsForm(App.Win32Window);
        }

        private static string CreateMarkerName(int number)
        {
            return string.Format("{0}{1:D4}", TrackRegionPrefix, number);
        }

        private void OutputTrackDetails(TrackList tracks)
        {
            foreach (TrackDefinition track in tracks)
            {
                Output.ToScriptWindow("Track {0}\t{1}\t{2}",
                    track.Number,
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.StartPosition)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.EndPosition))
                );
            }
            Output.LineBreakToScriptWindow();
        }

        private TrackList FindTracks(IScriptableApp app, ISfFileHost file)
        {
            _findTracksOptions.Validate();

            ScriptTimer.Reset();
            List<ScanResult> results = DoFirstPassStatisticsScan();

            TrackList tracks = new TrackList(_findTracksOptions, file);
            int trackCount = 1;
            bool currentResultIsTrack = false;
            foreach (ScanResult scanResult in results)
            {
                if (scanResult.RmsLevelExceeds(_findTracksOptions.GapNoisefloorThresholdInDecibels))
                {
                    //Output.ToScriptWindow("{0} above threshold", scanResult.WindowNumber);
                    if (!currentResultIsTrack && tracks.CanAddNextTrack(scanResult.SelectionStart))
                    {
                        tracks.AddNew();
                        tracks.LastAdded.StartPosition = scanResult.SelectionStart;
                        tracks.LastAdded.Number = trackCount++;
                        currentResultIsTrack = true;
                    }
                    tracks.LastAdded.EndPosition = scanResult.SelectionEnd;
                }
                else if (tracks.CanSetTrackBreak())
                {
                    currentResultIsTrack = false;
                }
                //Output.ToScriptWindow("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                //    scanResult.WindowNumber,
                //    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(scanResult.SelectionStart)),
                //    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(scanResult.SelectionEnd)),
                //    SfHelpers.RatioTodB(scanResult.Ch1Statistics.RMSLevel),
                //    SfHelpers.RatioTodB(scanResult.Ch2Statistics.RMSLevel),
                //    scanResult.GetMaxRmsLevel()
            }

            Output.ToScriptWindow("FindTracks Finished scanning:\r\n- Scanned: {0} windows\r\n- Window Length: {1}s\r\n- Scan Duration: {2}", results.Count, _findTracksOptions.ScanWindowLengthInSeconds, ScriptTimer.Time());
            Output.LineBreakToScriptWindow();
            OutputTrackDetails(tracks);

            ScriptTimer.Reset();
            RefineTrackDefinitionsByScanning(tracks);
            Output.LineBreakToScriptWindow();

            Output.ToScriptWindow("RefineTrackDefinitionsByScanning Finished scanning:\r\n- Scan Duration: {0}", ScriptTimer.Time());
            Output.LineBreakToScriptWindow();
            OutputTrackDetails(tracks);

            return tracks;
        }

        private List<ScanResult> DoFirstPassStatisticsScan()
        {
            return DoStatisticsScan(_findTracksOptions.ScanWindowLengthInSamples(_file), _findTracksOptions.StartScanFilePositionInSamples, _file.Length);
        }

        private void RefineTrackDefinitionsByScanning(TrackList tracks)
        {
            long scanSelectionLength = _findTracksOptions.ScanWindowLengthInSamples(_file);

            for (int i = 0; i < tracks.Count; i++)
            {
                TrackDefinition track = tracks[i];
                long trackStartWindowLength = _file.SecondsToPosition(0.05); //TODO: make configurable
                List<ScanResult> refineStartResults = DoStatisticsScan(trackStartWindowLength, track.StartPosition, track.StartPosition + scanSelectionLength);
                foreach (ScanResult scanResult in refineStartResults)
                {
                    if (!scanResult.RmsLevelExceeds(_findTracksOptions.GapNoisefloorThresholdInDecibels)) continue;
                    track.StartPosition = scanResult.SelectionStart;
                    Output.ToScriptWindow("Track {0} - Moving START to {1}", track.Number, OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.StartPosition)));
                    break;
                }
                long trackEndWindowLength = _file.SecondsToPosition(0.2); //TODO: make configurable
                //TODO: use longer, overlapping windows (but with short intervals between) to find track ends??
                List<ScanResult> refineEndResults = DoStatisticsScan(trackEndWindowLength, track.EndPosition - scanSelectionLength, track.EndPosition);
                for (int j = 0; j < refineEndResults.Count; j++)
                {
                    ScanResult scanResult = refineEndResults[j];
                    if (scanResult.RmsLevelExceeds(_findTracksOptions.GapNoisefloorThresholdInDecibels)) continue;
                    track.EndPosition = scanResult.SelectionStart - 1;
                    Output.ToScriptWindow("Track {0} - Moving END to {1}", track.Number, OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.EndPosition)));
                    break;
                }
            }
        }

        private List<ScanResult> DoStatisticsScan(long scanWindowLength, long startPosition, long scanEndPosition)
        {
            long windowCount = 1;

            bool scannedToEnd = false;
            List<ScanResult> results = new List<ScanResult>();
            while (!scannedToEnd)
            {
                long selectionEnd = startPosition + scanWindowLength;
                if (selectionEnd >= scanEndPosition)
                {
                    selectionEnd = scanEndPosition;
                    scannedToEnd = true;
                }
                //Output.ToScriptWindow("Start={0} End={1}", selectionStart, selectionEnd);
                SfAudioSelection windowedSelection = WindowTasks.NewSelectionUsingEndPosition(startPosition, selectionEnd);

                ScanResult result = new ScanResult();
                result.WindowNumber = windowCount;
                result.SelectionStart = startPosition;
                result.SelectionEnd = selectionEnd;
                result.Ch1Statistics = _fileTasks.GetChannelStatisticsOverSelection(0, windowedSelection);
                result.Ch2Statistics = _fileTasks.GetChannelStatisticsOverSelection(1, windowedSelection);
                results.Add(result);

                Output.ToStatusField1("{0}", windowCount);
                Output.ToStatusField2("{0}s", OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(startPosition)));
                startPosition = selectionEnd + 1;
                windowCount++;
            }
            return results;
        }

        private void CleanVinylRecording(string presetName, int noiseReductionPasses, SfAudioSelection noiseprintSelection)
        {
            SfAudioSelection selection = _fileTasks.SelectAll();
            _fileTasks.ApplyEffectPreset(App, selection, "Sony Click and Crackle Removal", presetName, EffectOptions.EffectOnly, Output.ToScriptWindow);
            for (int i = 1; i <= noiseReductionPasses; i++)
            {
                Output.ToScriptWindow("Noise Reduction (pass #{0})", i);
                EffectOptions noiseReductionOption = EffectOptions.EffectOnly;
                //if (i == 1)
                //    noiseReductionOption = EffectOptions.DialogFirst;
                _fileTasks.CopySelectionToStart(App, noiseprintSelection);
                _fileTasks.ApplyEffectPreset(App, selection, "Sony Noise Reduction", presetName, noiseReductionOption, Output.ToScriptWindow);
                _file.Window.SetSelectionAndScroll(0, _noiseprintSelection.Length, DataWndScrollTo.NoMove);
                App.DoMenuAndWait("Edit.Delete", false);
            }
            Output.LineBreakToScriptWindow();
        }

        #region Results Form
        public void ConfirmTrackSplitsForm(IWin32Window hOwner)
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

            dlg.Show(hOwner);
        }

        private bool ValidatePathDetails()
        {
            if (!Directory.Exists(_tbxRootFolder.Text))
            {
                Output.ToMessageBox(ScriptTitle, MessageBoxIcon.Error, "Root library folder path does not exist.");
                return false;
            }
            string album = GetPathSafeSegmentName(_tbxAlbum.Text);
            string artist = GetPathSafeSegmentName(_tbxArtist.Text);
            string path = Path.Combine(Path.Combine(_tbxRootFolder.Text, artist), album);
            if (Directory.Exists(path))
            {
                Output.ToMessageBox(ScriptTitle, MessageBoxIcon.Error, "Output path already exists: \"{0}\"", path);
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

            DoFinalAudioClean();
            Directory.CreateDirectory(_outputDirectory);
            List<SplitTrackDefinition> tracks = GetSplitTrackDefinitions();
            DoTrackSplitting(tracks);
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

        private List<SplitTrackDefinition> GetSplitTrackDefinitions()
        {
            List<SfAudioMarker> trackMarkers = GetTrackRegions();

            long addFadeSamples = _file.SecondsToPosition(_findTracksOptions.TrackAddFadeOutLengthInSeconds);
            List<SplitTrackDefinition> splitTrackDefinitions = new List<SplitTrackDefinition>();
            Output.ToScriptWindow("Track Splits");
            for (int i = 1; i <= trackMarkers.Count; i++)
            {
                int mi = i - 1;
                SfAudioMarker marker = trackMarkers[mi];
                long maxEndPosition = _file.Length; // cannot be past end of file
                if (i < trackMarkers.Count)
                {
                    maxEndPosition = trackMarkers[mi + 1].Start - 1; // cannot overlap next track
                }
                long maxLength = maxEndPosition - marker.Start;
                long lengthWithFade = marker.Length + addFadeSamples;
                if (lengthWithFade > maxLength)
                    lengthWithFade = maxLength;
                SplitTrackDefinition track = new SplitTrackDefinition();
                track.Number = i;
                track.Selection = new SfAudioSelection(marker.Start, lengthWithFade);
                track.FadeInLength = _findTracksOptions.TrackFadeInLengthInSamples;
                track.FadeOutStartPosition = marker.Length;
                splitTrackDefinitions.Add(track);

                Output.ToScriptWindow("{0}:\t{1}\t{2}\t(Start fade @ {3})", track.Number,
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.Selection.Start)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.Selection.Length)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.FadeOutStartPosition)));
            }
            Output.LineBreakToScriptWindow();
            return splitTrackDefinitions;
        }

        private List<SfAudioMarker> GetTrackRegions()
        {
            Regex regionNameRegex = new Regex(string.Concat("^", TrackRegionPrefix, "[0-9]{4}$"));
            List<SfAudioMarker> trackMarkers = new List<SfAudioMarker>();
            SfAudioMarker[] markers = GetMarkerList();
            foreach (SfAudioMarker marker in markers)
            {
                if (!marker.HasLength)
                    continue;
                if (!regionNameRegex.IsMatch(marker.Name))
                    continue;
                trackMarkers.Add(marker);
            }
            return trackMarkers;
        }

        private SfAudioMarker[] GetMarkerList()
        {
            SfAudioMarker[] markers = new SfAudioMarker[_file.Markers.Count];
            _file.Markers.CopyTo(markers, 0);
            return markers;
        }

        private void DoTrackSplitting(List<SplitTrackDefinition> tracks)
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
                ISfFileHost trackFile = _file.NewFile(track.Selection);
                trackFile.Markers.Clear();
                trackFile.Summary.Album = _tbxAlbum.Text;
                trackFile.Summary.Artist = _tbxArtist.Text;
                trackFile.Summary.TrackNo = string.Concat(track.Number, "/", tracks.Count);

                FileTasks trackTasks = new FileTasks(trackFile);
                if (track.FadeInLength > 0)
                {
                    Output.ToScriptWindow("Track {0}: Fade In {1} Samples", track.Number, track.FadeInLength);
                    trackFile.Window.SetSelectionAndScroll(0, track.FadeInLength, DataWndScrollTo.NoMove);
                    App.DoMenuAndWait("Process.FadeIn", false);
                }
                if (track.FadeOutStartPosition < track.Selection.Length)
                {
                    Output.ToScriptWindow("Track {0}: Fade Out {1} Samples", track.Number, track.Selection.Length - track.FadeOutStartPosition);
                    trackFile.Window.SetSelectionAndScroll(track.FadeOutStartPosition, trackFile.Length, DataWndScrollTo.NoMove);
                    App.DoMenuAndWait("Process.FadeOut", false);
                }
                trackTasks.ApplyEffectPreset(App, trackTasks.SelectAll(), "iZotope MBIT+ Dither", "Convert to 16 bit (advanced light dither)", EffectOptions.EffectOnly, Output.ToScriptWindow);

                string savePath = string.Concat(_outputDirectory, Path.PathSeparator, CreateMarkerName(track.Number) + ".flac");
                trackFile.SaveAs(savePath, "FLAC Audio", "44,100 Hz, 16 Bit, Stereo", RenderOptions.SaveMetadata);
                trackFile.Close(CloseOptions.QuerySaveIfChanged);

                Output.ToScriptWindow("Saved '{0}'", savePath);
                Output.LineBreakToScriptWindow();
            }
        }
    }
}
