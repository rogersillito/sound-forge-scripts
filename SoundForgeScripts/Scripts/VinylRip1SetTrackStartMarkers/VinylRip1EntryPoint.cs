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
using SoundForge;
using SoundForgeScriptsLib;
using SoundForgeScriptsLib.EntryPoints;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    [ScriptName("Vinyl Rip - 1 Set Track Start Markers")]
    public class EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;

        private FindTracksOptions _findTracksOptions;
        private SfAudioSelection _noiseprintSelection;
        private TrackList _trackList;
        private FileTasks _fileTasks;

        private const string AggressiveCleaningPreset = "Vinyl Processing (Pre-Track Splitting)";
        private const string TrackRegionPrefix = @"__TRACK__";

        //TODO: clear console
        //TODO: come up with a way of saving settings between script runs, and between the 2 scripts (save options to json?)
        protected override void Execute()
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

            _trackList = FindTracks(App, _file);

            App.DoMenuAndWait("Edit.UndoAll", false);

            foreach (TrackDefinition track in _trackList)
            {
                _file.Markers.AddRegion(track.StartPosition, track.Length, CreateMarkerName(track.Number));
            }
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
            long trackStartWindowLength = _file.SecondsToPosition(0.05); //TODO: make configurable
            long trackEndWindowLength = _file.SecondsToPosition(0.1); //TODO: make configurable
            long trackEndWindowOverlap = trackEndWindowLength - (long)Math.Round(trackEndWindowLength / 100.0f);

            for (int i = 0; i < tracks.Count; i++)
            {
                TrackDefinition track = tracks[i];
                List<ScanResult> refineStartResults = DoStatisticsScan(trackStartWindowLength, track.StartPosition, track.StartPosition + scanSelectionLength);
                foreach (ScanResult scanResult in refineStartResults)
                {
                    if (!scanResult.RmsLevelExceeds(_findTracksOptions.GapNoisefloorThresholdInDecibels))
                    {
                        Output.ToScriptWindow("-- Track {0} NOT START: {1} -> {2}", track.Number, 
                            OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(scanResult.SelectionStart)),
                            OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(scanResult.SelectionEnd))
                        );
                        continue;
                    }
                    track.StartPosition = scanResult.SelectionStart;
                    Output.ToScriptWindow("Track {0} - Moving START to {1}", track.Number, OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.StartPosition)));
                    break;
                }

                //TODO: use longer, overlapping windows (but with short intervals between) to find track ends??
                long trackEndScanPosition = track.EndPosition + trackEndWindowLength;
                bool isLastTrack = track == tracks.LastAdded;
                if (trackEndScanPosition > tracks.FileLength)
                    trackEndScanPosition = tracks.FileLength;
                else if (!isLastTrack && trackEndScanPosition > tracks[i+1].StartPosition)
                    trackEndScanPosition = tracks[i+1].StartPosition - 1;

                List<ScanResult> refineEndResults = DoStatisticsScan(trackEndWindowLength, track.EndPosition - scanSelectionLength, trackEndScanPosition, trackEndWindowOverlap);
                for (int j = 0; j < refineEndResults.Count; j++)
                {
                    ScanResult scanResult = refineEndResults[j];
                    if (scanResult.RmsLevelExceeds(_findTracksOptions.GapNoisefloorThresholdInDecibels))
                    {
                        Output.ToScriptWindow("-- Track {0} NOT END: {1} -> {2}", track.Number, 
                            OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(scanResult.SelectionStart)),
                            OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(scanResult.SelectionEnd))
                        );
                        continue;
                    }
                    track.EndPosition = scanResult.SelectionStart - 1;
                    Output.ToScriptWindow("Track {0} - Moving END to {1}", track.Number, OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.EndPosition)));
                    break;
                }
            }
        }

        private List<ScanResult> DoStatisticsScan(long scanWindowLength, long startPosition, long scanEndPosition)
        {
            return DoStatisticsScan(scanWindowLength, startPosition, scanEndPosition, 0);
        }

        private List<ScanResult> DoStatisticsScan(long scanWindowLength, long startPosition, long scanEndPosition, long windowOverlap)
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
                startPosition = selectionEnd + 1 - windowOverlap;
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
    }
}
