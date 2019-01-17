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
        private readonly EditTracksForm _form;
        private readonly EntryPointBase _entryPoint;
        private EditTracksViewModel _vm;
        private SplitTrackList _tracks;
        private FindTracksOptions _options;

        public EditTracksController(IScriptableApp app, EditTracksForm form, EntryPointBase entryPoint, OutputHelper output, FileTasks fileTasks)
        {
            _app = app;
            _form = form;
            _entryPoint = entryPoint;
            _output = output;
            _fileTasks = fileTasks;
        }

        private DeleteMarker _markerDeleteCallback;
        public DeleteMarker MarkerDeleteCallback
        {
            set { _markerDeleteCallback = value; }
        }

        public void Edit(EditTracksViewModel viewModel, SplitTrackList tracks, FindTracksOptions options)
        {
            _options = options;
            _tracks = tracks;
            _vm = viewModel;
            viewModel.Build(tracks, _entryPoint.ScriptTitle);
            Bind();
            _form.Create(viewModel);
            if (_vm.HasTracks)
                _vm.CurrentTrack = tracks.GetTrack(1);
            _form.Show(_app.Win32Window);
        }

        private void Bind()
        {
            //_form.PreviewAllClicked = delegate { AddTrack(); };
            //_form.PreviewStartClicked = delegate {  };
            //_form.PreviewEndClicked = delegate {  };
            _form.DeleteClicked = delegate { DeleteTrack(); };
            _form.NextClicked = delegate { NextTrack(); };
            _form.PreviousClicked = delegate { PreviousTrack(); };
            _form.AddTrackClicked = delegate { AddTrack(); };
        }

        public void DeleteTrack()
        {
            _form.Close();
            Thread.Sleep(1500);
            SplitTrackDefinition deleteTrack = _vm.CurrentTrack;

            List<int> idents = new List<int>();
            idents.Add(deleteTrack.FadeInEndMarker.Ident);
            idents.Add(deleteTrack.FadeOutEndMarker.Ident);
            idents.Add(deleteTrack.TrackRegion.Ident);
            _output.ToScriptWindow("DT: {0} FadeInEndMarker.Ident {1}, {2}, {3}", deleteTrack.Number, deleteTrack.FadeInEndMarker.Ident, deleteTrack.FadeOutEndMarker.Ident, deleteTrack.TrackRegion.Ident);
            int n = _vm.CurrentTrack.Number;

            SplitTrackDefinition nextCurrent = _tracks.GetTrack(n + 1);
            if (nextCurrent == null)
            {
                nextCurrent = _tracks.GetTrack(n - 1);
            }
            _tracks.Delete(deleteTrack);

            //delete_testy();
            //_markerDeleteCallback(deleteTrack.TrackRegion);
            foreach (int ident in idents)
            {
                _markerDeleteCallback(ident);
            }
            //_markerDeleteCallback(deleteTrack.FadeOutEndMarker);
            //SfAudioMarker trackRegion = deleteTrack.TrackRegion;
            //SfAudioMarker fadeInEndMarker = deleteTrack.FadeInEndMarker;
            //SfAudioMarker fadeOutEndMarker = deleteTrack.FadeOutEndMarker;
            _vm.CurrentTrack = nextCurrent;
        }

        private void delete_testy()
        {
            SfAudioMarkerList markers = _app.CurrentFile.Markers;
            _output.ToMessageBox(string.Format("marker count = {0}", markers.Count));
        }

        public void AddTrack()
        {
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
            //SelectCurrentTrack();
        }

        public void NextTrack()
        {
            if (!_vm.CanNavigateNext) return;
            int n = _vm.CurrentTrack == null ? 0 : _vm.CurrentTrack.Number;
            _vm.CurrentTrack = _tracks.GetTrack(n + 1);
            //SelectCurrentTrack();
        }

        //private void SelectCurrentTrack()
        //{
        //    if (_vm.CurrentTrack == null) return;
        //    _fileTasks.SetSelection(_vm.CurrentTrack.GetSelectionWithFades());
        //}
    }
}