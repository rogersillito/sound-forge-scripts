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
using System.Runtime.InteropServices;
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
            //Output.ToScriptWindow(_file.UndosAreEnabled);

            FileTasks.CopyNoisePrintSelectionToStart(App, _file); // retain this after undo for subsequent Vinyl Rip Scripts to use

            //int undoId = _file.BeginUndo("PrepareAudio");
            AggressivelyCleanRecordedAudio();
            //_file.Markers.Clear();
            FindTracksOptions options = new FindTracksOptions();
            options.ScanWindowLengthInSeconds = 2;
            FindTracks(App, _file, options);
            //Output.ToMessageBox("end undo", MessageBoxIcon.Hand, "press ok");
            //_file.EndUndo(undoId, true); //TODO: retain marker positions in script and undo NOT working!
            //Output.ToMessageBox("Crack on...", MessageBoxIcon.Hand, "press ok");
            //App.DoMenuAndWait("Edit.UndoAll", false);

            //file.Window.SetCursorAndScroll(engine._markerPositions[0], DataWndScrollTo.Nearest);
            //file.Markers.AddMarker(engine._markerPositions[0], "bob");
            Output.ToScriptWindow("c");
        }

        private void FindTracks(IScriptableApp app, ISfFileHost file, FindTracksOptions findTracksOptions)
        {
            findTracksOptions.Validate();

            ScriptTimer.Reset();
            List<ScanResult> results = DoStatisticsScan(findTracksOptions);
            foreach (ScanResult scanResult in results)
            {
                Output.ToScriptWindow("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
                    scanResult.WindowNumber, 
                    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(scanResult.SelectionStart)), 
                    OutputHelper.FormatToTimeSpan(file.PositionToSeconds(scanResult.SelectionEnd)), 
                    SfHelpers.RatioTodB(scanResult.Ch1Statistics.RMSLevel), 
                    SfHelpers.RatioTodB(scanResult.Ch2Statistics.RMSLevel), 
                    scanResult.GetMaxRmsLevel()
                );
            }
            //TODO: statistics we're saving don't correspond to what we get when manually selecting a region and generating them...
            Output.ToScriptWindow("FindTracks Finished scanning:\r\n- Scanned: {0} windows\r\n- Window Length: {1}s\r\n- Scan Duration: {2}", results.Count, findTracksOptions.ScanWindowLengthInSeconds, ScriptTimer.Time());
        }

        private List<ScanResult> DoStatisticsScan(FindTracksOptions findTracksOptions)
        {
            long selectionStart = 0;
            long scanWindowLength = _file.SecondsToPosition(findTracksOptions.ScanWindowLengthInSeconds);
            long windowCount = 1;

            bool scannedToEnd = false;
            List<ScanResult> results = new List<ScanResult>();
            while (!scannedToEnd)
            {
                long selectionEnd = selectionStart + scanWindowLength;
                if (selectionEnd >= _file.Length)
                {
                    selectionEnd = _file.Length;
                    scannedToEnd = true;
                }
                //Output.ToScriptWindow("Start={0} End={1}", selectionStart, selectionEnd);
                SfAudioSelection windowedSelection = new SfAudioSelection(selectionStart, selectionEnd);

                ScanResult result = new ScanResult();
                result.WindowNumber = windowCount;
                result.SelectionStart = selectionStart;
                result.SelectionEnd = selectionEnd;
                result.Ch1Statistics = FileTasks.GetChannelStatisticsOverSelection(0, windowedSelection, _file);
                result.Ch2Statistics = FileTasks.GetChannelStatisticsOverSelection(1, windowedSelection, _file);
                results.Add(result);

                Output.ToStatusField1("{0}", windowCount);
                Output.ToStatusField2("{0}s", OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(selectionStart)));
                selectionStart = selectionEnd + 1;
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

            ISfGenericEffect effect = app.FindEffect(effectName);
            if (effect == null) throw new Exception(string.Format("Effect '{0}' was not found.", effectName));

            ISfGenericPreset preset = effect.GetPreset(presetName);
            if (preset == null) throw new Exception(string.Format("Preset '{0}' was not found for effect '{1}'", presetName, effectName));

            file.DoEffect(effect.Guid, preset.Name, selection, EffectOptions.EffectOnly);
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
    }

    public class FindTracksOptions
    {
        private double _scanWindowLengthInSeconds;
        private double _gapNoisefloorThresholdInDecibels;

        public double ScanWindowLengthInSeconds
        {
            get { return _scanWindowLengthInSeconds; }
            set { _scanWindowLengthInSeconds = value; }
        }

        public double GapNoisefloorThresholdInDecibels
        {
            get { return _gapNoisefloorThresholdInDecibels; }
            set { _gapNoisefloorThresholdInDecibels = value; }
        }

        public void Validate()
        {
            const double minWinLength = 0.2;
            if (_scanWindowLengthInSeconds < minWinLength)
                throw new ScriptAbortedException("ScanWindowLengthInSeconds must be >= {0}", minWinLength);

            //double minNoiseFloor = 0.2;
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

        public static SfAudioSelection NewWholeFileSelection()
        {
            return new SfAudioSelection(0, -1);
        }
    }
}