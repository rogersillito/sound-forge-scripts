using SoundForgeScriptsLib.VinylRip;
using System.ComponentModel;
using SoundForge;
using SoundForgeScriptsLib;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksViewModel : INotifyPropertyChanged
    {
        private readonly FileTasks _fileTasks;

        private SplitTrackList _tracks;
        private string _formTitle;
        private long _plusOrMinusSamples = 20;
        private long _plusPlusOrMinusMinusSamples = 400;
        private long _zoomPadding = 3000;

        public EditTracksViewModel(FileTasks fileTasks)
        {
            _fileTasks = fileTasks;
        }

        public void Build(SplitTrackList tracks, string scriptTitle)
        {
            _formTitle = scriptTitle;
            _tracks = tracks;
        }

        public long ZoomPadding
        {
            get { return _zoomPadding; }
            set { _zoomPadding = value; }
        }

        public long PlusOrMinusSamples
        {
            get { return _plusOrMinusSamples; }
            set { _plusOrMinusSamples = value; }
        }

        public long PlusPlusOrMinusMinusSamples
        {
            get { return _plusPlusOrMinusMinusSamples; }
            set { _plusPlusOrMinusMinusSamples = value; }
        }

        public bool HasTracks
        {
            get { return _tracks.Count > 0; }
        }

        private SplitTrackDefinition _currentTrack;

        public SplitTrackDefinition CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                _currentTrack = value;
                if (_currentTrack != null)
                {
                    _fileTasks.SetSelection(_currentTrack.GetSelectionWithFades());
                }
                OnPropertyChanged("TrackName");
                OnPropertyChanged("CanNavigateNext");
                OnPropertyChanged("CanNavigatePrevious");
                OnPropertyChanged("HasTracks");
                TriggerCanMoveStartPropertiesChanged();
                TriggerCanMoveFadeInPropertiesChanged();
                TriggerCanMoveEndPropertiesChanged();
                TriggerCanMoveFadeOutPropertiesChanged();
            }
        }

        public string TrackName
        {
            get { return HasTracks ? string.Format("Track {0}", _currentTrack.Number) : "NO TRACKS FOUND!"; }
        }

        public int CurrentTrackIndex
        {
            get { return HasTracks && _currentTrack != null ? _tracks.IndexOf(_currentTrack) : -1; }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FormTitle
        {
            get { return _formTitle; }
        }

        public bool CanNavigatePrevious
        {
            get { return HasTracks && CurrentTrack.Number > 1; }
        }

        public bool CanNavigateNext
        {
            get { return HasTracks && !CurrentTrack.IsLastTrack; }
        }

        private void ZoomToCurrentTrackStart()
        {
            long selectionLength = CurrentTrack.FadeInEndMarker.Start - CurrentTrack.TrackRegion.Start;
            SfAudioSelection selection = new SfAudioSelection(CurrentTrack.TrackRegion.Start, selectionLength);
            _fileTasks.SetSelection(selection);
            _fileTasks.ZoomToShow(_fileTasks.ExpandSelectionAround(selection, ZoomPadding));
            _fileTasks.RedrawWindow();
        }

        private void ZoomToCurrentTrackEnd()
        {
            long selectionLength = CurrentTrack.FadeOutEndMarker.Start - MarkerHelper.GetMarkerEnd(CurrentTrack.TrackRegion);
            SfAudioSelection selection = new SfAudioSelection(MarkerHelper.GetMarkerEnd(CurrentTrack.TrackRegion), selectionLength);
            _fileTasks.SetSelection(selection);
            _fileTasks.ZoomToShow(_fileTasks.ExpandSelectionAround(selection, ZoomPadding));
            _fileTasks.RedrawWindow();
        }

        public bool MoveFadeIn(long samples)
        {
            if (!CurrentTrack.CanMoveFadeInBy(samples))
                return false;
            CurrentTrack.MoveFadeInBy(samples);
            TriggerCanMoveFadeInPropertiesChanged();
            ZoomToCurrentTrackStart();
            return true;
        }

        private void TriggerCanMoveFadeInPropertiesChanged()
        {
            OnPropertyChanged("CanMoveFadeInPlus");
            OnPropertyChanged("CanMoveFadeInPlusPlus");
            OnPropertyChanged("CanMoveFadeInMinus");
            OnPropertyChanged("CanMoveFadeInMinusMinus");
        }

        public bool CanMoveFadeInPlus
        {
            get { return CurrentTrack.CanMoveFadeInBy(PlusOrMinusSamples); }
        }

        public bool CanMoveFadeInPlusPlus
        {
            get { return CurrentTrack.CanMoveFadeInBy(PlusPlusOrMinusMinusSamples); }
        }

        public bool CanMoveFadeInMinus
        {
            get { return CurrentTrack.CanMoveFadeInBy(-PlusOrMinusSamples); }
        }

        public bool CanMoveFadeInMinusMinus
        {
            get { return CurrentTrack.CanMoveFadeInBy(-PlusPlusOrMinusMinusSamples); }
        }

        public bool MoveStart(long samples)
        {
            if (!CurrentTrack.CanMoveStartBy(samples))
                return false;
            CurrentTrack.MoveStartBy(samples);
            TriggerCanMoveStartPropertiesChanged();
            TriggerCanMoveFadeInPropertiesChanged();
            ZoomToCurrentTrackStart();
            return true;
        }

        private void TriggerCanMoveStartPropertiesChanged()
        {
            OnPropertyChanged("CanMoveStartPlus");
            OnPropertyChanged("CanMoveStartPlusPlus");
            OnPropertyChanged("CanMoveStartMinus");
            OnPropertyChanged("CanMoveStartMinusMinus");
        }

        public bool CanMoveStartPlus
        {
            get { return CurrentTrack.CanMoveStartBy(PlusOrMinusSamples); }
        }

        public bool CanMoveStartPlusPlus
        {
            get { return CurrentTrack.CanMoveStartBy(PlusPlusOrMinusMinusSamples); }
        }

        public bool CanMoveStartMinus
        {
            get { return CurrentTrack.CanMoveStartBy(-PlusOrMinusSamples); }
        }

        public bool CanMoveStartMinusMinus
        {
            get { return CurrentTrack.CanMoveStartBy(-PlusPlusOrMinusMinusSamples); }
        }

        public bool MoveFadeOut(long samples)
        {
            if (!CurrentTrack.CanMoveFadeOutBy(samples))
                return false;
            CurrentTrack.MoveFadeOutBy(samples);
            TriggerCanMoveFadeOutPropertiesChanged();
            ZoomToCurrentTrackEnd();
            return true;
        }

        private void TriggerCanMoveFadeOutPropertiesChanged()
        {
            OnPropertyChanged("CanMoveFadeOutPlus");
            OnPropertyChanged("CanMoveFadeOutPlusPlus");
            OnPropertyChanged("CanMoveFadeOutMinus");
            OnPropertyChanged("CanMoveFadeOutMinusMinus");
        }

        public bool CanMoveFadeOutPlus
        {
            get { return CurrentTrack.CanMoveFadeOutBy(PlusOrMinusSamples); }
        }

        public bool CanMoveFadeOutPlusPlus
        {
            get { return CurrentTrack.CanMoveFadeOutBy(PlusPlusOrMinusMinusSamples); }
        }

        public bool CanMoveFadeOutMinus
        {
            get { return CurrentTrack.CanMoveFadeOutBy(-PlusOrMinusSamples); }
        }

        public bool CanMoveFadeOutMinusMinus
        {
            get { return CurrentTrack.CanMoveFadeOutBy(-PlusPlusOrMinusMinusSamples); }
        }

        public bool MoveEnd(long samples)
        {
            if (!CurrentTrack.CanMoveEndBy(samples))
                return false;
            CurrentTrack.MoveEndBy(samples);
            TriggerCanMoveEndPropertiesChanged();
            TriggerCanMoveFadeInPropertiesChanged();
            TriggerCanMoveFadeOutPropertiesChanged();
            ZoomToCurrentTrackEnd();
            return true;
        }

        private void TriggerCanMoveEndPropertiesChanged()
        {
            OnPropertyChanged("CanMoveEndPlus");
            OnPropertyChanged("CanMoveEndPlusPlus");
            OnPropertyChanged("CanMoveEndMinus");
            OnPropertyChanged("CanMoveEndMinusMinus");
        }

        public bool CanMoveEndPlus
        {
            get { return CurrentTrack.CanMoveEndBy(PlusOrMinusSamples); }
        }

        public bool CanMoveEndPlusPlus
        {
            get { return CurrentTrack.CanMoveEndBy(PlusPlusOrMinusMinusSamples); }
        }

        public bool CanMoveEndMinus
        {
            get { return CurrentTrack.CanMoveEndBy(-PlusOrMinusSamples); }
        }

        public bool CanMoveEndMinusMinus
        {
            get { return CurrentTrack.CanMoveEndBy(-PlusPlusOrMinusMinusSamples); }
        }
    }
}
