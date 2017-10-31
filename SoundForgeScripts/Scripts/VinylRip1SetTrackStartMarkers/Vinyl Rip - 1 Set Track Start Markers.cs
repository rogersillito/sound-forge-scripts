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
        private readonly List<long> _markerPositions = new List<long>();

        protected override void Execute()
        {
            _file = App.CurrentFile;
            _markerPositions.Clear();

            if (_file == null || _file.Channels != 2)
            {
                throw new ScriptAbortedException("A stereo file must be open before this script can be run.");
            }
            //_file.UndosAreEnabled = true;

            //TODO: uncomment, for quicker testing though - use "Hi-Gloss_TEST-for_testing_track_find.pca"
            const int noisePrintLengthSeconds = 2;
            //FileTasks.CopyNoisePrintSelectionToStart(App, _file, noisePrintLengthSeconds); // retain this after undo for subsequent Vinyl Rip Scripts to use
            //int undoId = _file.BeginUndo("PrepareAudio");
            //AggressivelyCleanRecordedAudio();
            //_file.Markers.Clear();
            FindTracksOptions options = new FindTracksOptions();
            options.ScanWindowLengthInSeconds = 1;
            options.GapNoisefloorThresholdInDecibels = -70;
            options.MinimumTrackGapInSeconds = 1;
            options.MinimumTrackLengthInSeconds = 10;
            options.StartScanFilePositionInSamples = _file.SecondsToPosition(noisePrintLengthSeconds);
            FindTracks(App, _file, options);
            //_file.EndUndo(undoId, true); //TODO: retain marker positions in script and undo NOT working!
            //Output.ToMessageBox("Pausing...", MessageBoxIcon.Hand, "press ok");
            //App.DoMenuAndWait("Edit.UndoAll", false);

            //file.Window.SetCursorAndScroll(engine._markerPositions[0], DataWndScrollTo.Nearest);
            //file.Markers.AddMarker(engine._markerPositions[0], "bob");
        }

        private void FindTracks(IScriptableApp app, ISfFileHost file, FindTracksOptions findTracksOptions)
        {
            findTracksOptions.Validate();

            ScriptTimer.Reset();
            List<ScanResult> results = DoFirstPassStatisticsScan(findTracksOptions);

            TrackList tracks = new TrackList(findTracksOptions, file);
            int trackCount = 1;
            bool currentResultIsTrack = false;
            foreach (ScanResult scanResult in results)
            {
                //TODO: maybe create preliminary track list without validating; validate once true start/ends have been found?
                if (scanResult.RmsLevelExceeds(findTracksOptions.GapNoisefloorThresholdInDecibels))
                {
                    Output.ToScriptWindow("{0} above threshold", scanResult.WindowNumber);
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
//long foundExactStartPosition = file.FindAudioAbove(windowedSelection, SfHelpers.dBToRatio(findTracksOptions.GapNoisefloorThresholdInDecibels)

//file.SnapPositionToZeroCrossing(foundExactStartPosition, true); // This honors the 'Snap to zero crossing-slope' setting in Preferences -> Editing
//App.DoMenuAndWait(Edit.SnapToZero, false);
                
                //Output.ToScriptWindow("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                //    scanResult.WindowNumber,
                //    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(scanResult.SelectionStart)),
                //    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(scanResult.SelectionEnd)),
                //    SfHelpers.RatioTodB(scanResult.Ch1Statistics.RMSLevel),
                //    SfHelpers.RatioTodB(scanResult.Ch2Statistics.RMSLevel),
                //    scanResult.GetMaxRmsLevel()
            }

            Output.ToScriptWindow("FindTracks Finished scanning:\r\n- Scanned: {0} windows\r\n- Window Length: {1}s\r\n- Scan Duration: {2}", results.Count, findTracksOptions.ScanWindowLengthInSeconds, ScriptTimer.Time());

            RefineTrackDefinitionsByScanning(tracks, findTracksOptions);
            //tracks.RefineStartPositions(Output);
            foreach (TrackDefinition track in tracks)
            {

                Output.ToScriptWindow("{0}\t{1}\t{2}",
                    track.Number,
                    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(track.StartPosition)),
                    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(track.EndPosition))
                    );

                file.Markers.AddRegion(track.StartPosition, track.EndPosition - track.StartPosition, string.Format("Track_{0:D4}", track.Number));
            }
        }

        private List<ScanResult> DoFirstPassStatisticsScan(FindTracksOptions findTracksOptions)
        {
            return DoStatisticsScan(findTracksOptions.ScanWindowLengthInSamples(_file), findTracksOptions.StartScanFilePositionInSamples, _file.Length);
        }

        private void RefineTrackDefinitionsByScanning(TrackList tracks, FindTracksOptions findTracksOptions)
        {
            long windowLength = _file.SecondsToPosition(0.1);
            long scanSelectionLength = findTracksOptions.ScanWindowLengthInSamples(_file);

            for (int i = 0; i < tracks.Count; i++)
            {
                TrackDefinition track = tracks[i];
                List<ScanResult> refineStartResults = DoStatisticsScan(windowLength, track.StartPosition, track.StartPosition + scanSelectionLength);
                foreach (ScanResult scanResult in refineStartResults)
                {
                    if (!scanResult.RmsLevelExceeds(findTracksOptions.GapNoisefloorThresholdInDecibels)) continue;
                    track.StartPosition = scanResult.SelectionStart;
                    Output.ToScriptWindow("Track {0} - Moving START to {1}", track.Number, OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.StartPosition)));
                    break;
                }
                List<ScanResult> refineEndResults = DoStatisticsScan(windowLength, track.EndPosition - scanSelectionLength, track.EndPosition);
                for (int j = 0; j < refineEndResults.Count; j++)
                {
                    ScanResult scanResult = refineEndResults[j];
                    if (scanResult.RmsLevelExceeds(findTracksOptions.GapNoisefloorThresholdInDecibels)) continue;
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

        //private int FindTrackStartsOld(IScriptableApp app, ISfFileHost file)
        //{
        //    long fileLength = file.Length;
        //    long startPosition = 0;
        //    int markerId = 1;
        //    int foundCount = 0;

        //    while (startPosition < file.Length && markerId < 100)
        //    {
        //        SfAudioSelection selection = new SfAudioSelection(startPosition, file.Length);
        //        long foundPosition = file.FindAudioAbove(selection, 0.001, true);
        //        _markerPositions.Add(foundPosition);
        //        file.Markers.AddMarker(foundPosition, markerId.ToString());
        //        markerId++;
        //        app.OutputText(foundPosition.ToString());
        //        startPosition = foundPosition;
        //    }
        //    //MessageBox.Show(foundPosition.ToString());
        //    return 0;
        //}

        private void AggressivelyCleanRecordedAudio()
        {
            SfAudioSelection selection = WindowTasks.NewWholeFileSelection();
            const string stage1Preset = "Vinyl Processing (Pre-Track Splitting)";
            ApplyEffectPreset(App, selection, "Sony Click and Crackle Removal", stage1Preset);
            ApplyEffectPreset(App, selection, "Sony Noise Reduction", stage1Preset);
            ApplyEffectPreset(App, selection, "Sony Noise Reduction", stage1Preset); // 2 passes
        }

        private static void ApplyEffectPreset(IScriptableApp app, SfAudioSelection selection, string effectName, string presetName)
        {
            ISfFileHost file = app.CurrentFile;
//long foundExactStartPosition = file.FindAudioAbove(windowedSelection, SfHelpers.dBToRatio(findTracksOptions.GapNoisefloorThresholdInDecibels)

            ISfGenericEffect effect = app.FindEffect(effectName);
            if (effect == null) throw new Exception(string.Format("Effect '{0}' was not found.", effectName));

            ISfGenericPreset preset = effect.GetPreset(presetName);
            if (preset == null) throw new Exception(string.Format("Preset '{0}' was not found for effect '{1}'", presetName, effectName));

            file.DoEffect(effect.Guid, preset.Name, selection, EffectOptions.EffectOnly);
        }
    }

    public class TrackDefinition
    {
        public int Number;
        public long StartPosition;
        private long _endPosition;
        private bool _endWasSet;

        public long EndPosition
        {
            //TODO: do we need all this now? delete?
            get { return _endPosition; }
            set
            {
                _endWasSet = true;
                _endPosition = value;
            }
        }

        public bool EndWasSet
        {
            get { return _endWasSet; }
        }

        public SfAudioSelection WholeTrackSelection()
        {
            return new SfAudioSelection(StartPosition, Length);
        }

        public long Length
        {
//long foundExactStartPosition = file.FindAudioAbove(windowedSelection, SfHelpers.dBToRatio(findTracksOptions.GapNoisefloorThresholdInDecibels)
            get { return _endPosition - StartPosition; }
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

        public void Validate()
        {
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
        public static void CopyNoisePrintSelectionToStart(IScriptableApp app, ISfFileHost file)
        {
            CopyNoisePrintSelectionToStart(app, file, 2.0d);
        }

        public static void CopyNoisePrintSelectionToStart(IScriptableApp app, ISfFileHost file, double noisePrintLength)
        {
            ISfDataWnd window = file.Window;
            double selectionLengthSeconds = file.PositionToSeconds(window.Selection.Length);
            if (selectionLengthSeconds < noisePrintLength)
                throw new ScriptAbortedException("A noise selection of {0} seconds or more must be made before running this script.", noisePrintLength);

            WindowTasks.SelectBothChannels(window);

            long noisePrintSampleLength = file.SecondsToPosition(noisePrintLength);
            window.SetSelectionAndScroll(window.Selection.Start, noisePrintSampleLength, DataWndScrollTo.NoMove);
            app.DoMenuAndWait("Edit.Copy", false);

            window.SetCursorAndScroll(0, DataWndScrollTo.NoMove);
            file.Markers.AddMarker(0, "Noise-Print-End");
            app.DoMenuAndWait("Edit.Paste", false);
        }

        public static void ConvertStereoToMono(ISfFileHost file)
        {
            double[,] aGainMap = new double[1, 2] { { 0.5, 0.5 } };
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
}