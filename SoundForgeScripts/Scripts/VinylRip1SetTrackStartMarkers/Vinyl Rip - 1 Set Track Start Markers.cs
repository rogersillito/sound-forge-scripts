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

using System;
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
    public class EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;
        //private readonly List<long> _markerPositions = new List<long>();

        private TextBox _tbxAlbum;
        private TextBox _tbxArtist;
        private TextBox _tbxRootFolder;
        private FindTracksOptions _findTracksOptions;
        private SfAudioSelection _noiseprintSelection;
        private TrackList _trackList;
        private string _outputDirectory;

        private const string AggressiveCleaningPreset = "Vinyl Processing (Pre-Track Splitting)";
        private const string FinalCleaningPreset = "Vinyl Processing (Final)";
        private const string DefaultRootLibraryFolder = @"F:\My Music\From Vinyl\";
        private const string TrackRegionPrefix = @"__TRACK__";

        protected override void Execute()
        {
            ScriptTitle = "Process Vinyl Recording";
            _file = App.CurrentFile;
            //_markerPositions.Clear();

            if (_file == null || _file.Channels != 2)
            {
                throw new ScriptAbortedException("A stereo file must be open before this script can be run.");
            }
            FileTasks.ZoomOutFull(_file);

            //TODO: retain marker positions in script and undo NOT working!
            //_file.UndosAreEnabled = true;
            //int undoId = _file.BeginUndo("PrepareAudio");
            //_file.EndUndo(undoId, true); 

            //TODO: uncomment, for quicker testing though - use "Hi-Gloss_TEST-for_testing_track_find.pca"
            const int noiseprintLengthSeconds = 2;
            _noiseprintSelection = FileTasks.PromptNoisePrintSelection(App, _file, noiseprintLengthSeconds);
            CleanVinylRecording(AggressiveCleaningPreset, 4, _noiseprintSelection); //TODO: configure number of noise reduction passes?

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

            //App.DoMenuAndWait("Edit.UndoAll", false);

            foreach (TrackDefinition track in _trackList)
            {
                _file.Markers.AddRegion(track.StartPosition, track.Length, string.Format("{0}{1:D4}", TrackRegionPrefix, track.Number));
            }

            ConfirmTrackSplitsForm(App.Win32Window);

            //Output.ToMessageBox("Pausing...", MessageBoxIcon.Hand, "press ok");
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
                result.Ch1Statistics = FileTasks.GetChannelStatisticsOverSelection(0, windowedSelection, _file);
                result.Ch2Statistics = FileTasks.GetChannelStatisticsOverSelection(1, windowedSelection, _file);
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
            SfAudioSelection selection = WindowTasks.NewWholeFileSelection();
            ApplyEffectPreset(App, selection, "Sony Click and Crackle Removal", presetName);
            for (int i = 0; i < noiseReductionPasses; i++)
            {
                FileTasks.CopySelectionToStart(App, _file, noiseprintSelection);
                ApplyEffectPreset(App, selection, "Sony Noise Reduction", presetName);
                _file.Window.SetSelectionAndScroll(0, _noiseprintSelection.Length, DataWndScrollTo.NoMove);
                Output.ToMessageBox("Wait");
                App.DoMenuAndWait("Edit.Delete", false);
            }
        }

        //TODO: move to helper
        private static void ApplyEffectPreset(IScriptableApp app, SfAudioSelection selection, string effectName, string presetName)
        {
            ISfFileHost file = app.CurrentFile;

            ISfGenericEffect effect = app.FindEffect(effectName);
            if (effect == null) throw new ScriptAbortedException(string.Format("Effect '{0}' was not found.", effectName));

            ISfGenericPreset preset = effect.GetPreset(presetName);
            if (preset == null) throw new ScriptAbortedException(string.Format("Preset '{0}' was not found for effect '{1}'", presetName, effectName));

            file.DoEffect(effect.Guid, preset.Name, selection, EffectOptions.WaitForDoneOrCancel | EffectOptions.EffectOnly);
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
            lblRootFolder.TabIndex = 1;
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
            lblRootFolder.TabIndex = 2;
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
            lblRootFolder.TabIndex = 3;
            _tbxAlbum.Width = sForm.Width - pt.X - sOff.Width;
            _tbxAlbum.Height = tbxHeight;
            _tbxAlbum.Location = pt;
            dlg.Controls.Add(_tbxAlbum);
            pt.Y += _tbxAlbum.Height + sOff.Height;

            // we position the buttons relative to the bottom and left of the form.
            pt = (Point)dlg.ClientSize;
            pt -= sOff;

            Button btn = new Button();
            btn.TabIndex = 5;
            pt -= btn.Size;
            btn.Text = "Cancel";
            btn.Location = pt;
            btn.Click += FormHelper.OnCancel_Click;
            dlg.Controls.Add(btn);
            dlg.CancelButton = btn;
            pt.X -= (btn.Width + 10);

            btn = new Button();
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

            Directory.CreateDirectory(_outputDirectory);
            DoFinalAudioClean();
            DoTrackSplitting();

            Output.ToStatusBar(ScriptTitle + " DONE");
        }
        #endregion // Results Form

        public static string GetPathSafeSegmentName(string name)
        {
            const char replChar = ';';
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, replChar);
            }
            return name;
        }

        private void DoFinalAudioClean()
        {
            FileTasks.CopySelectionToStart(App, _file, _noiseprintSelection);
            FileTasks.ZoomOutFull(_file);
            ApplyEffectPreset(App, new SfAudioSelection(0, _file.Length), "Sony Paragraphic EQ", "[Sys] Remove very low and inaudible frequencies below 20 Hz");
            _file.Window.SetSelectionAndScroll(0, _file.Length, DataWndScrollTo.NoMove);
            CleanVinylRecording(FinalCleaningPreset, 3, _noiseprintSelection);
            //TODO: normalize
            //TODO: dither?
        }

        private void DoTrackSplitting()
        {
            Regex regionNameRegex = new Regex(string.Concat("^", TrackRegionPrefix, "[0-9]{4}$"));
            int saveTrackNumber = 1;
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (!marker.IsRegion || !regionNameRegex.IsMatch(marker.Name))
                    continue;

                long addFadeSamples = _file.SecondsToPosition(_findTracksOptions.TrackAddFadeOutLengthInSeconds);
                SfAudioSelection trackSelection = new SfAudioSelection(marker.Start, marker.Length + addFadeSamples);
                _file.NewFile(trackSelection);
                // TOD apply fade settings 
                // TOD apply fade settings 

                saveTrackNumber++;
            }
        }
    }


    public class TrackDefinition
    {
        public int Number;
        public long StartPosition;
        public long EndPosition;

        public SfAudioSelection WholeTrackSelection()
        {
            return new SfAudioSelection(StartPosition, Length);
        }

        public long Length
        {
            get { return EndPosition - StartPosition; }
        }
    }

    public class TrackList : List<TrackDefinition>
    {
        private readonly FindTracksOptions _findTracksOptions;
        private readonly ISfFileHost _file;

        public TrackList(FindTracksOptions findTracksOptions, ISfFileHost file)
        {
            _findTracksOptions = findTracksOptions;
            _file = file;
        }

        public bool CanAddNextTrack(long nextTrackStartPosition)
        {
            if (LastAdded == null)
                return true;

            // would track gap be too short ?
            long minimumAllowableStartPosition = LastAdded.EndPosition + _file.SecondsToPosition(_findTracksOptions.MinimumTrackGapInSeconds);
            return minimumAllowableStartPosition <= nextTrackStartPosition;
        }

        public bool CanSetTrackBreak()
        {
            if (LastAdded == null)
                return true;

            // would track length be too short ?
            return _file.PositionToSeconds(LastAdded.Length) >= _findTracksOptions.MinimumTrackLengthInSeconds;
        }

        public TrackDefinition LastAdded
        {
            get { return Count > 0 ? this[Count - 1] : null; }
        }

        public void AddNew()
        {
            Add(new TrackDefinition());
        }
    }

    public class ScanResult
    {
        public long WindowNumber;
        public long SelectionStart;
        public long SelectionEnd;
        public SfAudioStatistics Ch1Statistics;
        public SfAudioStatistics Ch2Statistics;

        public double GetMaxRmsLevel()
        {
            double loudest = Ch1Statistics.RMSLevel;
            if (Ch2Statistics.RMSLevel > loudest)
                loudest = Ch2Statistics.RMSLevel;
            return SfHelpers.RatioTodB(loudest);
        }

        public bool RmsLevelExceeds(double noisefloorThresholdInDecibels)
        {
            return GetMaxRmsLevel() >= noisefloorThresholdInDecibels;
        }
    }

    public class FindTracksOptions
    {
        private double _scanWindowLengthInSeconds;
        private double _gapNoisefloorThresholdInDecibels;
        private double _minimumTrackGapInSeconds;
        private double _minimumTrackLengthInSeconds;
        private long _startScanFilePositionInSamples;
        private long _trackFadeInLengthInSamples;
        private double _trackAddFadeOutLengthInSeconds;

        public double ScanWindowLengthInSeconds
        {
            get { return _scanWindowLengthInSeconds; }
            set { _scanWindowLengthInSeconds = value; }
        }

        public long ScanWindowLengthInSamples(ISfFileHost file)
        {
            return file.SecondsToPosition(_scanWindowLengthInSeconds);
        }

        public double GapNoisefloorThresholdInDecibels
        {
            get { return _gapNoisefloorThresholdInDecibels; }
            set { _gapNoisefloorThresholdInDecibels = value; }
        }

        public double MinimumTrackGapInSeconds
        {
            get { return _minimumTrackGapInSeconds; }
            set { _minimumTrackGapInSeconds = value; }
        }

        public double MinimumTrackLengthInSeconds
        {
            get { return _minimumTrackLengthInSeconds; }
            set { _minimumTrackLengthInSeconds = value; }
        }

        public long StartScanFilePositionInSamples
        {
            get { return _startScanFilePositionInSamples; }
            set { _startScanFilePositionInSamples = value; }
        }

        public long TrackFadeInLengthInSamples
        {
            get { return _trackFadeInLengthInSamples; }
            set { _trackFadeInLengthInSamples = value; }
        }

        public double TrackAddFadeOutLengthInSeconds
        {
            get { return _trackAddFadeOutLengthInSeconds; }
            set { _trackAddFadeOutLengthInSeconds = value; }
        }

        public void Validate()
        {
            //TODO: instead of validating, set defaults? use in an initial UI to allow reconfiguring?
            const double minWinLength = 0.1;
            if (_scanWindowLengthInSeconds < minWinLength)
                throw new ScriptAbortedException("ScanWindowLengthInSeconds must be >= {0}", minWinLength);

            const double minNoiseFloor = -100;
            if (_scanWindowLengthInSeconds < minWinLength)
                throw new ScriptAbortedException("GapNoisefloorThresholdInDecibels must be >= {0}", minNoiseFloor);

            const double minTrackGap = 0.5;
            if (_minimumTrackGapInSeconds < minTrackGap)
                throw new ScriptAbortedException("MinimumTrackGapInSeconds must be >= {0}", minTrackGap);

            const double minTrackLength = 5.0;
            if (MinimumTrackLengthInSeconds < minTrackLength)
                throw new ScriptAbortedException("MinimumTrackLengthInSeconds must be >= {0}", minTrackLength);
        }
    }

    public class FileTasks
    {
        public static SfAudioSelection PromptNoisePrintSelection(IScriptableApp app, ISfFileHost file)
        {
            return PromptNoisePrintSelection(app, file, 2.0d);
        }

        public static SfAudioSelection PromptNoisePrintSelection(IScriptableApp app, ISfFileHost file, double noiseprintLength)
        {
            ISfDataWnd window = file.Window;
            double selectionLengthSeconds = file.PositionToSeconds(window.Selection.Length);
            if (selectionLengthSeconds < noiseprintLength)
                throw new ScriptAbortedException("A noise selection of {0} seconds or more must be made before running this script.", noiseprintLength);

            WindowTasks.SelectBothChannels(window);

            long noiseprintSampleLength = file.SecondsToPosition(noiseprintLength);
            SfAudioSelection selection = new SfAudioSelection(window.Selection.Start, noiseprintSampleLength, 0);

            return selection;
        }

        public static void CopySelectionToStart(IScriptableApp app, ISfFileHost file, SfAudioSelection selection)
        {
            file.Window.SetSelectionAndScroll(selection.Start, selection.Length, DataWndScrollTo.NoMove);
            app.DoMenuAndWait("Edit.Copy", false);
            file.Window.SetCursorAndScroll(0, DataWndScrollTo.NoMove);
            app.DoMenuAndWait("Edit.Paste", false);
        }

        public static void ConvertStereoToMono(ISfFileHost file)
        {
            double[,] aGainMap = { { 0.5, 0.5 } };
            file.DoConvertChannels(1, 0, aGainMap, EffectOptions.EffectOnly);
        }

        public static SfAudioStatistics GetChannelStatisticsOverSelection(uint channel, SfAudioSelection selection, ISfFileHost file)
        {
            file.UpdateStatistics(selection);
            SfStatus status = file.WaitForDoneOrCancel();
            if (!file.StatisticsAreUpToDate)
                throw new ScriptAbortedException("Failed to update statistics for selection: {0} - {1} samples (WaitForDoneOrCancel returned \"{2}\")", status);
            SfAudioStatistics statistics = file.GetStatistics(channel);
            return statistics;
        }

        public static void ZoomOutFull(ISfFileHost file)
        {
            file.Window.ZoomToShow(0, file.Length);
        }
    }

    public class WindowTasks
    {
        public static void SelectBothChannels(ISfDataWnd window)
        {
            if (window.File.Channels != 2)
                throw new ScriptAbortedException("Expected a 2-channel file.");
            switch (window.Selection.ChanMask)
            {
                case 0:
                    // both
                    return;
                case 1:
                    // left-only
                    window.ForwardKey(Keys.Tab);
                    window.ForwardKey(Keys.Tab);
                    return;
                case 2:
                    // right-only
                    window.ForwardKey(Keys.Tab);
                    break;
            }
        }

        public static SfAudioSelection NewSelectionUsingEndPosition(long ccStart, long ccEnd)
        {
            return new SfAudioSelection(ccStart, ccEnd - ccStart);
        }

        public static SfAudioSelection NewWholeFileSelection()
        {
            return new SfAudioSelection(0, -1);
        }
    }

    public class FormHelper
    {
        public static DialogResult ShowScriptPauseDialog(IWin32Window hOwner)
        {
            return ShowScriptPauseDialog(hOwner, "Pause Script");
        }

        //TODO: refactor into a class with properties that have sensible defaults,  and assignable delegate handlers
        public static DialogResult ShowScriptPauseDialog(IWin32Window hOwner, string dialogTitle)
        {
            Form dlg = new Form();
            Size sForm = new Size(520, 150);

            dlg.Text = dialogTitle;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ClientSize = sForm;

            Point pt = new Point(10, 10);
            Size sOff = new Size(10, 10);
            Size sSpacer = new Size(10, 5);
            const int lblHeight = 16;

            // we position the buttons relative to the bottom and left of the form.
            pt = (Point)dlg.ClientSize;
            pt -= sOff;

            Button btn = new Button();
            btn.TabIndex = 5;
            pt -= btn.Size;
            btn.Text = "Cancel";
            btn.Location = pt;
            btn.Click += FormHelper.OnCancel_Click;
            dlg.Controls.Add(btn);
            dlg.CancelButton = btn;
            pt.X -= (btn.Width + 10);

            btn = new Button();
            btn.TabIndex = 4;
            btn.Text = "OK";
            btn.Location = pt;
            btn.Click += FormHelper.OnOK_Click;
            dlg.Controls.Add(btn);
            dlg.AcceptButton = btn;
            pt.X -= (btn.Width + 10);

            // position prompt beside buttons
            pt = new Point(sOff.Width, dlg.ClientSize.Height - sSpacer.Height - btn.Size.Height);

            Label lblPrompt = new Label();
            lblPrompt.Text = "Press OK to continue...";
            lblPrompt.Width = sForm.Width - pt.X - sOff.Width;
            lblPrompt.Height = lblHeight;
            lblPrompt.Location = pt;
            dlg.Controls.Add(lblPrompt);
            pt.Y -= lblPrompt.Height;

            DialogResult result = dlg.ShowDialog(hOwner);
            return result;
        }

        // generic Ok button click (sets dialog result and dismisses the form)
        public static void OnOK_Click(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            Form form = (Form)btn.Parent;
            form.DialogResult = DialogResult.OK;
            form.Close();
        }

        // generic Cancel button click (sets dialog result and dismisses the form)
        public static void OnCancel_Click(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            Form form = (Form)btn.Parent;
            form.DialogResult = DialogResult.Cancel;
            form.Close();
        }
    }
}
