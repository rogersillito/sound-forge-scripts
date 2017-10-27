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

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    public class EntryPoint: EntryPointBase
    {
        private ISfFileHost _file;
        private readonly List<long> _markerPositions = new List<long>();

        protected override void Execute()
        {
            _file = App.CurrentFile;
            _markerPositions.Clear();

            if (_file == null)
            {
                throw new ScriptAbortedException("A file must be open before this script can be run.");
            }
            else
            {
                CreateNoisePrint();
                //int undoId = engine.PrepareAudio(app, file);
                //file.Markers.Clear();
                //engine.FindTrackStarts(app, file);
                //file.EndUndo(undoId, false);
                //file.Window.SetCursorAndScroll(engine._markerPositions[0], DataWndScrollTo.Nearest);
                //file.Markers.AddMarker(engine._markerPositions[0], "bob");
            }
        }

        private int FindTrackStarts(IScriptableApp app, ISfFileHost file)
        {
            long fileLength = file.Length;
            long startPosition = 0;
            int markerId = 1;

            while (startPosition < file.Length && markerId < 100)
            {

                SfAudioSelection selection = new SfAudioSelection(startPosition, file.Length);
                long foundPosition = file.FindAudioAbove(selection, 0.001, true);
                _markerPositions.Add(foundPosition);
                file.Markers.AddMarker(foundPosition, markerId.ToString());
                markerId++;
                app.OutputText(foundPosition.ToString());
                startPosition = foundPosition;
            }
            //MessageBox.Show(foundPosition.ToString());
            return 0;
        }

        private int PrepareAudio(IScriptableApp app, ISfFileHost file)
        {
            CreateNoisePrint();

            SfAudioSelection wholeFileSelection = new SfAudioSelection(0, -1);

            file.UndosAreEnabled = true;
            int undoId = file.BeginUndo("PrepareAudio");

            double[,] aGainMap = new double[1, 2] { { 0.5, 0.5 } };
            file.DoConvertChannels(1, 0, aGainMap, EffectOptions.EffectOnly);

            const string stage1Preset = "Vinyl Processing (Pre-Track Splitting)";
            ApplyEffectPreset(app, wholeFileSelection, "Sony Click and Crackle Removal", stage1Preset);
            ApplyEffectPreset(app, wholeFileSelection, "Sony Noise Reduction", stage1Preset);
            ApplyEffectPreset(app, wholeFileSelection, "Sony Noise Reduction", stage1Preset); // 2 passes

            return undoId;
        }

        private void CreateNoisePrint()
        {
            ISfDataWnd window = _file.Window;
            double selectionLengthSeconds = _file.PositionToSeconds(window.Selection.Length);
            const double noisePrintLength = 2.0d;
            if (selectionLengthSeconds < noisePrintLength)
                throw new ScriptAbortedException("A noise selection of {0} seconds or more must be made before running this script.", noisePrintLength);

            SelectBothChannels(window);

            long noisePrintSampleLength = _file.SecondsToPosition(noisePrintLength);
            window.SetSelectionAndScroll(window.Selection.Start, noisePrintSampleLength, DataWndScrollTo.NoMove);
            ISfFileHost noisePrintData = window.File.NewFile(window.Selection);
            App.DoMenuAndWait("Edit.Copy", false);

            window.SetCursorAndScroll(0, DataWndScrollTo.NoMove);
            _file.Markers.AddMarker(0, "NoisePrint End");
            //TODO: fix - is pasting to a new window..?
            App.DoMenuAndWait("Edit.Paste", false);

            //_file.OverwriteAudio(0,0,noisePrintData, new SfAudioSelection(noisePrintData));

        }

        private static void SelectBothChannels(ISfDataWnd window)
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
}