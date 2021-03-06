using System;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib
{
    public class FileTasks
    {
        private readonly ISfFileHost _file;

        public FileTasks(ISfFileHost file)
        {
            _file = file;
        }

        public SfAudioSelection EnforceNoisePrintSelection(IScriptableApp app)
        {
            return EnforceNoisePrintSelection(app, 2.0d);
        }

        public bool IsCurrentSelectionGreaterThan(IScriptableApp app, double minimumLengthInSeconds)
        {
            ISfDataWnd window = _file.Window;
            double selectionLengthSeconds = _file.PositionToSeconds(window.Selection.Length);
            return selectionLengthSeconds >= minimumLengthInSeconds;
        }

        public SfAudioSelection EnforceNoisePrintSelection(IScriptableApp app, double noiseprintLength)
        {
            if (!IsCurrentSelectionGreaterThan(app, noiseprintLength))
                throw new ScriptAbortedException("A noise selection of {0} seconds or more must be made before running this script.", noiseprintLength);

            ISfDataWnd window = _file.Window;
            WindowTasks.SelectBothChannels(window);

            long noiseprintSampleLength = _file.SecondsToPosition(noiseprintLength);
            SfAudioSelection selection = new SfAudioSelection(window.Selection.Start, noiseprintSampleLength, 0);

            return selection;
        }

        public long SecondsToPosition(double seconds) => _file.SecondsToPosition(seconds);
        public double PositionToSeconds(long position) => _file.PositionToSeconds(position);

        public void CopySelectionToStart(IScriptableApp app, SfAudioSelection selection)
        {
            _file.Window.SetSelectionAndScroll(selection.Start, selection.Length, DataWndScrollTo.NoMove);
            app.DoMenuAndWait("Edit.Copy", false);
            _file.Window.SetCursorAndScroll(0, DataWndScrollTo.NoMove);
            app.DoMenuAndWait("Edit.Paste", false);
        }

        public void ConvertStereoToMono()
        {
            double[,] aGainMap = { { 0.5, 0.5 } };
            _file.DoConvertChannels(1, 0, aGainMap, EffectOptions.EffectOnly);
        }

        public SfAudioStatistics GetChannelStatisticsOverSelection(uint channel, SfAudioSelection selection)
        {
            _file.UpdateStatistics(selection);
            SfStatus status = _file.WaitForDoneOrCancel();
            if (!_file.StatisticsAreUpToDate)
                throw new ScriptAbortedException("Failed to update statistics for selection: {0} - {1} samples (WaitForDoneOrCancel returned \"{2}\")", status);
            SfAudioStatistics statistics = _file.GetStatistics(channel);
            return statistics;
        }

        public SfAudioSelection ExpandSelectionAround(SfAudioSelection selection, long tryExpandBy)
        {
            SfAudioSelection newSelection = new SfAudioSelection(selection);
            newSelection.Start -= tryExpandBy;
            if (newSelection.Start < 0)
                newSelection.Start = 0;

            newSelection.Length += (2 * tryExpandBy);
            if (SelectionHelper.GetSelectionEnd(newSelection) > _file.Length)
                SelectionHelper.SetSelectionEnd(newSelection, _file.Length);

            return newSelection;
        }

        public void SetSelection(SfAudioSelection selection)
        {
            SetSelection(selection, DataWndScrollTo.NoMove);
        }

        public void SetSelection(SfAudioSelection selection, DataWndScrollTo scrollTo)
        {
            _file.Window.SetSelectionAndScroll(selection.Start, selection.Length, scrollTo);
        }

        public SfAudioSelection SelectAll()
        {
            SfAudioSelection selection = new SfAudioSelection(_file);
            SetSelection(selection);
            return selection;
        }

        public void RedrawWindow()
        {
            _file.Window.RedrawWindow(false);
        }

        public void ZoomOutFull()
        {
            _file.Window.ZoomToShow(0, _file.Length);
        }

        public void ZoomToShow(SfAudioSelection selection)
        {
            _file.Window.ZoomToShow(selection.Start, selection.Length);
        }

        public void ApplyEffectPreset(IScriptableApp app, SfAudioSelection selection, string effectName, string presetName, EffectOptions effectOption, OutputHelper.MessageLogger logger)
        {
            if (effectOption == EffectOptions.ReturnPreset || effectOption == EffectOptions.WaitForDoneOrCancel)
                throw new ScriptAbortedException("Invalid EffectOptions option: " + effectOption);

            ISfGenericEffect effect = app.FindEffect(effectName);
            if (effect == null)
                throw new ScriptAbortedException(String.Format("Effect '{0}' was not found.", effectName));

            ISfGenericPreset preset = effect.GetPreset(presetName);
            if (preset == null)
                throw new ScriptAbortedException(String.Format("Preset '{0}' was not found for effect '{1}'.", presetName, effectName));

            if (logger != null)
                logger("Applying Effect '{0}', Preset '{1}'...", effect.Name, preset.Name);

            _file.DoEffect(effectName, presetName, selection, effectOption | EffectOptions.WaitForDoneOrCancel | EffectOptions.ReturnPreset);
        }

        public void EnforceStereoFileOpen()
        {
            if (_file == null || _file.Channels != 2)
            {
                throw new ScriptAbortedException("A stereo file must be open before this script can be run.");
            }
        }
    }
}
