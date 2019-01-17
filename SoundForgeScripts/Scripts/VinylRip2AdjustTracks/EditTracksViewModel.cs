using SoundForgeScriptsLib.VinylRip;
using System.ComponentModel;
using SoundForgeScriptsLib;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public class EditTracksViewModel : INotifyPropertyChanged
    {
        private readonly FileTasks _fileTasks;

        private SplitTrackList _tracks;

        public EditTracksViewModel(FileTasks fileTasks)
        {
            _fileTasks = fileTasks;
        }

        public void Build(SplitTrackList tracks, string scriptTitle)
        {
            _formTitle = scriptTitle;
            _tracks = tracks;
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
        //protected bool SetField<T>(ref T field, T value, string propertyName)
        //{
        //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //    field = value;
        //    OnPropertyChanged(propertyName);
        //    return true;
        //}

        private string _formTitle;
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
    }
}