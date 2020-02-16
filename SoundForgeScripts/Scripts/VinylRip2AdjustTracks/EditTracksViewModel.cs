using SoundForgeScriptsLib.VinylRip;
using System.ComponentModel;
using SoundForge;
using SoundForgeScriptsLib;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksViewModel : INotifyPropertyChanged
    {
        private readonly FileTasks _fileTasks;

        private SplitTrackList _tracks;
        private string _formTitle;
        private long _plusOrMinusSamples = 10;
        private long _plusPlusOrMinusMinusSamples = 100;
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

                OnPropertyChanged("CanMoveFadeInPlus");
                OnPropertyChanged("CanMoveFadeInPlusPlus");
                OnPropertyChanged("CanMoveFadeInMinus");
                OnPropertyChanged("CanMoveFadeInMinusMinus");
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

        public void MoveFadeIn(long samples)
        {
            //TODO: make this return bool so we can exit when not possible (and avoid zoom/selection being changed)
            CurrentTrack.MoveFadeInBy(samples);
            OnPropertyChanged("CanMoveFadeInPlus");
            OnPropertyChanged("CanMoveFadeInPlusPlus");
            OnPropertyChanged("CanMoveFadeInMinus");
            OnPropertyChanged("CanMoveFadeInMinusMinus");
            long selectionLength = CurrentTrack.FadeInEndMarker.Start - CurrentTrack.TrackRegion.Start;
            SfAudioSelection selection = new SfAudioSelection(CurrentTrack.TrackRegion.Start, selectionLength);
            _fileTasks.SetSelection(selection);
            _fileTasks.ZoomToShow(_fileTasks.ExpandSelectionAround(selection, ZoomPadding));
            _fileTasks.RedrawWindow();
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
    }
}
