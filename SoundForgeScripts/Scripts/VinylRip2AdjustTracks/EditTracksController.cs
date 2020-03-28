using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using SoundForge;
using SoundForgeScriptsLib;
using SoundForgeScriptsLib.EntryPoints;
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksController
    {
        private readonly OutputHelper _output;
        private readonly FileTasks _fileTasks;
        private readonly IScriptableApp _app;
        private readonly EditTracksFormFactory _formFactory;
        private readonly EntryPointBase _entryPoint;
        private EditTracksViewModel _vm;
        private SplitTrackList _tracks;
        private FindTracksOptions _options;
        private EditTracksForm _form;

        public EditTracksController(IScriptableApp app, EditTracksFormFactory formFactory, EntryPointBase entryPoint, OutputHelper output, FileTasks fileTasks)
        {
            _app = app;
            _formFactory = formFactory;
            _entryPoint = entryPoint;
            _output = output;
            _fileTasks = fileTasks;
        }

        public void Edit(EditTracksViewModel viewModel, SplitTrackList tracks, FindTracksOptions options)
        {
            _options = options;
            _tracks = tracks;
            _vm = viewModel;
            viewModel.Build(tracks, _entryPoint.ScriptTitle);
            _form = _formFactory.Create(viewModel, this, _output);
            BindFormActions();
            if (_vm.HasTracks)
                _vm.CurrentTrack = tracks.GetTrack(1);
            _form.ShowDialog(_app.Win32Window);
        }

        private void BindFormActions()
        {
            _form.BtnPreviewAll.Click += delegate { PreviewAll(); };
            _form.BtnStopPreview.Click += delegate { PreviewStop(); };
            _form.BtnPreviewStart.Click += delegate { PreviewStart(); };
            _form.BtnPreviewEnd.Click += delegate { PreviewEnd(); };
            _form.BtnPrevious.Click += delegate { PreviousTrack(); };
            _form.BtnNext.Click += delegate { NextTrack(); };
            _form.BtnAddTrackBefore.Click += delegate { AddTrackBefore(); };
            _form.BtnAddTrackAfter.Click += delegate { AddTrackBefore(); };
            _form.BtnDelete.Click += delegate { DeleteTrack(); };

            _form.BtnMoveStartPlus.Click += delegate { MoveStart(_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveStartMinus.Click += delegate { MoveStart(-_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveStartPlusPlus.Click += delegate { MoveStart(_vm.PlusPlusOrMinusMinusSamples); }; 
            _form.BtnMoveStartMinusMinus.Click += delegate { MoveStart(-_vm.PlusPlusOrMinusMinusSamples); }; 

            _form.BtnMoveFadeInPlus.Click += delegate { MoveFadeIn(_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveFadeInMinus.Click += delegate { MoveFadeIn(-_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveFadeInPlusPlus.Click += delegate { MoveFadeIn(_vm.PlusPlusOrMinusMinusSamples); }; 
            _form.BtnMoveFadeInMinusMinus.Click += delegate { MoveFadeIn(-_vm.PlusPlusOrMinusMinusSamples); }; 

            _form.BtnMoveFadeOutPlus.Click += delegate { MoveFadeOut(_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveFadeOutMinus.Click += delegate { MoveFadeOut(-_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveFadeOutPlusPlus.Click += delegate { MoveFadeOut(_vm.PlusPlusOrMinusMinusSamples); }; 
            _form.BtnMoveFadeOutMinusMinus.Click += delegate { MoveFadeOut(-_vm.PlusPlusOrMinusMinusSamples); }; 

            _form.BtnMoveEndPlus.Click += delegate { MoveEnd(_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveEndMinus.Click += delegate { MoveEnd(-_vm.PlusOrMinusSamples); }; 
            _form.BtnMoveEndPlusPlus.Click += delegate { MoveEnd(_vm.PlusPlusOrMinusMinusSamples); }; 
            _form.BtnMoveEndMinusMinus.Click += delegate { MoveEnd(-_vm.PlusPlusOrMinusMinusSamples); }; 
        }

        public void MoveStart(long samples)
        {
            if (_vm.MoveStart(samples))
                _output.ToStatusField1(string.Format("{0}: {1}", _form.LblMoveStart.Text, FormatSamples(samples)));
        }

        public void MoveFadeIn(long samples)
        {
            if (_vm.MoveFadeIn(samples))
                _output.ToStatusField1(string.Format("{0}: {1}", _form.LblMoveFadeIn.Text, FormatSamples(samples)));
        }

        public void MoveFadeOut(long samples)
        {
            if (_vm.MoveFadeOut(samples))
                _output.ToStatusField1(string.Format("{0}: {1}", _form.LblMoveFadeOut.Text, FormatSamples(samples)));
        }

        public void MoveEnd(long samples)
        {
            if (_vm.MoveEnd(samples))
                _output.ToStatusField1(string.Format("{0}: {1}", _form.LblMoveEnd.Text, FormatSamples(samples)));
        }

        private string FormatSamples(long samples)
        {
            return samples >= 0 ? string.Format("+{0}", samples) : samples.ToString();
        }

        public void DeleteTrack()
        {
            SplitTrackDefinition deleteTrack = _vm.CurrentTrack;

            _output.ToScriptWindow("DT: {0} FadeInEndMarker.Ident {1}, {2}, {3}", deleteTrack.Number, deleteTrack.FadeInEndMarker.Ident, deleteTrack.FadeOutEndMarker.Ident, deleteTrack.TrackRegion.Ident);
            int n = _vm.CurrentTrack.Number;

            SplitTrackDefinition nextCurrent = _tracks.GetTrack(n + 1);
            if (nextCurrent == null)
            {
                // look for previous track instead
                nextCurrent = _tracks.GetTrack(n - 1);
            }
            _tracks.Delete(deleteTrack);

            _vm.CurrentTrack = nextCurrent;
        }

        public void AddTrackBefore()
        {
            //TODO: need this to work modally: "add before"/ "add after" ?
            bool selectionLongEnough = _fileTasks.IsCurrentSelectionGreaterThan(_app, _options.MinimumTrackLengthInSeconds);
            if (!selectionLongEnough)
            {
                _output.ToMessageBox(MessageBoxIcon.Exclamation, MessageBoxButtons.OK, "You must first make a selection of {0} seconds or more", _options.MinimumTrackLengthInSeconds);
                return;
            }
            //TODO: if overlaps with existing track adjust existing tracks to suit new track
            //TODO: IMPLEMENT.. insert a track
        }

        public void AddTrackAfter()
        {
            //TODO...
        }

        public void PreviousTrack()
        {
            if (!_vm.CanNavigatePrevious) return;
            int n = _vm.CurrentTrack.Number;
            _vm.CurrentTrack = _tracks.GetTrack(n - 1);
            _fileTasks.ZoomOutFull();
        }

        public void NextTrack()
        {
            if (!_vm.CanNavigateNext) return;
            int n = _vm.CurrentTrack == null ? 0 : _vm.CurrentTrack.Number;
            _vm.CurrentTrack = _tracks.GetTrack(n + 1);
            _fileTasks.ZoomOutFull();
        }

        public void PreviewStart()
        {
            PlayStart(_vm.CurrentTrack, false);
        }

        private void PreviewEnd()
        {
            PlayEnd(_vm.CurrentTrack, false);
        }

        public void PreviewStop()
        {
            _app.DoMenuAndWait("Transport.Stop", false);
            _fileTasks.SetSelection(_vm.CurrentTrack.GetSelectionWithFades());
        }

        private void PreviewAll()
        {
            foreach (SplitTrackDefinition track in _tracks)
            {
                PlayStart(track, true);
                PlayEnd(track, true);
            }
        }

        private void PlayStart(SplitTrackDefinition track, bool wait)
        {
            if (track == null) return;
            int previewLengthSeconds = 1;
            _fileTasks.SetSelection(new SfAudioSelection(
                track.GetSelectionWithFades().Start,
                _fileTasks.SecondsToPosition(previewLengthSeconds)));
            _app.DoMenuAndWait("Transport.Play", false);
            if (wait) Thread.Sleep(1000 * previewLengthSeconds);
        }

        private void PlayEnd(SplitTrackDefinition track, bool wait)
        {
            if (track == null) return;
            _fileTasks.SetSelection(new SfAudioSelection(
                track.FadeOutStartPosition,
                track.FadeOutLength));
            
            _app.DoMenuAndWait("Transport.Play", false);
            if (wait) Thread.Sleep(Convert.ToInt32(Math.Round(1000 * _fileTasks.PositionToSeconds(track.FadeOutLength))));
        }

        public void ToggleLoopedPlayback()
        {
            _app.DoMenuAndWait("Options.LoopPlayback", false);
        }
    }
}
