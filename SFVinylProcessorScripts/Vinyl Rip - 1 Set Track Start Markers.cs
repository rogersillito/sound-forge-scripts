using System;
using System.Windows.Forms;
using System.Collections.Generic;
using SoundForge;

public class EntryPoint
{
    public void FromSoundForge(IScriptableApp app)
    {
        ForgeApp = app; //execution begins here
        app.SetStatusText(String.Format("Script '{0}' is running.", Script.Name));
        string msg = ForgeScript.Engine.Begin(app);
        app.SetStatusText(msg != null ? msg : String.Format("Script '{0}' is done.", Script.Name));
    }
    public static IScriptableApp ForgeApp = null;
    public static void DPF(string sz) { ForgeApp.OutputText(sz); }
    public static void DPF(string fmt, params object[] args) { ForgeApp.OutputText(String.Format(fmt, args)); }
    public static string GETARG(string key, string str) { string val = Script.Args.ValueOf(key); if (val == null) val = str; return val; }
} //EntryPoint

namespace ForgeScript
{
    public class Engine
    {
        private List<long> _markerPositions = new List<long>();

        public static string Begin(IScriptableApp app)
        {

            ISfFileHost file = app.CurrentFile;
            if (file == null)
            {
                MessageBox.Show("A file must be open before this script can be run.");
            }
            else
            {
                Engine engine = new Engine();
                int undoId = engine.PrepareAudio(app, file);
                file.Markers.Clear();
                engine.FindTrackStarts(app, file);
                file.EndUndo(undoId, false);
                //file.Window.SetCursorAndScroll(engine._markerPositions[0], DataWndScrollTo.Nearest);
                //file.Markers.AddMarker(engine._markerPositions[0], "bob");
            }
            return null;
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
            SfAudioSelection wholeFileSelection = new SfAudioSelection(0, -1);

            file.UndosAreEnabled = true;
            int undoId = file.BeginUndo("PrepareAudio");

            double[,] aGainMap = new double[1, 2] { { 0.5, 0.5 } };
            file.DoConvertChannels(1, 0, aGainMap, EffectOptions.EffectOnly);

            ApplyEffectPreset(app, wholeFileSelection, "Sony Click and Crackle Removal", "[Sys] For manually fixing small selections");
            ApplyEffectPreset(app, wholeFileSelection, "Sony Click and Crackle Removal", "[Sys] For manually fixing small selections");
            //TODO: need to get the noise gate setting right:
            /* too much and the find tool gets too many hits (it gates parts of the track), too little and there are many hits during the track gaps).*/
            //ApplyEffectPreset(app, wholeFileSelection, "Sony ExpressFX Noise Gate", "Pre-Vinyl Track Splitting");

            return undoId;
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