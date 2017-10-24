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
using System.Windows.Forms;
using System.Collections.Generic;
using SoundForge;

public class EntryPoint
{
    public void FromSoundForge(IScriptableApp app)
    {
        ForgeApp = app; //execution begins here
        //app.SetStatusText(String.Format("Script '{0}' is running.", Script.Name));
        ForgeScript.Engine.Begin(app);
        //app.SetStatusText(msg != null ? msg : String.Format("Script '{0}' is done.", Script.Name));
    }
    public static IScriptableApp ForgeApp = null;
    //public static void DPF(string sz) { ForgeApp.OutputText(sz); }
    //public static void DPF(string fmt, params object[] args) { ForgeApp.OutputText(String.Format(fmt, args)); }
    //public static string GETARG(string key, string str) { string val = Script.Args.ValueOf(key); if (val == null) val = str; return val; }
} //EntryPoint



namespace ForgeScript
{
    public class OutputHelper
    {
        private readonly IScriptableApp _app;

        public OutputHelper(IScriptableApp app)
        {
            _app = app;
        }

        public void ToMessageBox(string fmt, params object[] args)
        {
             MessageBox.Show(string.Format(fmt, args));
        }

        public void ToScriptWindow(string fmt, params object[] args)
        {
            _app.OutputText(string.Format(fmt, args));
        }

        public void ToStatusBar(string fmt, params object[] args)
        {
            _app.SetStatusText(string.Format(fmt, args));
        }

        public void ToStatusField1(string fmt, params object[] args)
        {
            _app.SetStatusField(0, string.Format(fmt, args));
        }

        public void ToStatusField2(string fmt, params object[] args)
        {
            _app.SetStatusField(1, string.Format(fmt, args));
        }
    }

    public class Engine
    {
        private List<long> _markerPositions = new List<long>();
        private OutputHelper _outputHelper;
        private readonly IScriptableApp _app;
        private readonly ISfFileHost _file;

        private Engine(IScriptableApp app)
        {
            _app = app;
            _file = _app.CurrentFile;
            _outputHelper = new OutputHelper(app);
        }

        public static void Begin(IScriptableApp app)
        {
            new Engine(app).Execute();
        }

        public void Execute()
        {
            if (_file == null)
            {
                _outputHelper.ToMessageBox("A file must be open before this script can be run.");
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
            _outputHelper.ToScriptWindow("Selection length: {0}", window.SelectionLength);
            double lengthSeconds = _file.PositionToSeconds(window.Selection.Length);
            _outputHelper.ToScriptWindow("Selection length: {0}", lengthSeconds);
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