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
            EditTracksForm form = _formFactory.Create(viewModel);
            BindFormActions(form);
            if (_vm.HasTracks)
                _vm.CurrentTrack = tracks.GetTrack(1);
            form.ShowDialog(_app.Win32Window);
        }

        private void BindFormActions(EditTracksForm form)
        {
            //TODO: this fixes buttons not responding to clicks - but there's some woeful duplication...
            form.BtnPreviewAll.Click += delegate { PreviewAll(); };
            form.BtnStopPreview.Click += delegate { PreviewStop(); };
            form.BtnPreviewStart.Click += delegate { PreviewStart(); };
            form.BtnPreviewEnd.Click += delegate { PreviewEnd(); };
            form.BtnPrevious.Click += delegate { PreviousTrack(); };
            form.BtnNext.Click += delegate { NextTrack(); };
            form.BtnAddTrack.Click += delegate { AddTrack(); };
            form.BtnDelete.Click += delegate { DeleteTrack(); };

            //TODO: refactor/de-duplicate keybindings are invoking these event handlers
            form.PreviewAllClicked += delegate { PreviewAll(); };
            form.PreviewStartClicked += delegate { PreviewStart(); };
            form.PreviewEndClicked += delegate { PreviewEnd(); };
            form.DeleteClicked += delegate { DeleteTrack(); };
            form.NextClicked += delegate { NextTrack(); };
            form.PreviousClicked += delegate { PreviousTrack(); };
            form.AddTrackClicked += delegate { AddTrack(); };
            form.StopPreviewClicked += delegate { PreviewStop(); };
            form.LoopPlaybackClicked += delegate { ToggleLoopedPlayback(); };
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

        public void AddTrack()
        {
            //TODO: need this to work modally: "add before"/ "add after" ?
            bool selectionLongEnough = _fileTasks.IsCurrentSelectionGreaterThan(_app, _options.MinimumTrackLengthInSeconds);
            if (!selectionLongEnough)
            {
                _output.ToMessageBox(MessageBoxIcon.Exclamation, MessageBoxButtons.OK, "You must first make a selection of {0} seconds or more", _options.MinimumTrackLengthInSeconds);
                return;
            }
            //TODO: if overlaps with existing track adjust existing tracks to suit new track
            //TODO: insert a track
        }

        public void PreviousTrack()
        {
            if (!_vm.CanNavigatePrevious) return;
            int n = _vm.CurrentTrack.Number;
            _vm.CurrentTrack = _tracks.GetTrack(n - 1);
        }

        public void NextTrack()
        {
            if (!_vm.CanNavigateNext) return;
            int n = _vm.CurrentTrack == null ? 0 : _vm.CurrentTrack.Number;
            _vm.CurrentTrack = _tracks.GetTrack(n + 1);
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
