using SoundForge;

namespace SoundForgeScriptsLib.VinylRip
{
    public class SplitTrackDefinition
    {
        private readonly SplitTrackList _splitTrackList;
        private readonly ISfFileHost _originalFile;


        public SplitTrackDefinition(SplitTrackList splitTrackList, ISfFileHost file)
        {
            _splitTrackList = splitTrackList;
            _originalFile = file;
        }

        public bool CanAddFadeIn
        {
            get { return _fadeInLength > 0; }
        }

        public bool CanAddFadeOut
        {
            get { return _fadeOutStartPosition < _selection.Length; }
        }

        private int _number;
        public int Number
        {
            get { return _number; }
            set { _number = value; }
        }

        private SfAudioSelection _selection;
        public SfAudioSelection Selection
        {
            get { return _selection; }
            set { _selection = value; }
        }

        private SfAudioMarker _regionFound;
        public SfAudioMarker RegionFound
        {
            get { return _regionFound; }
            set { _regionFound = value; }
        }

        private long _fadeInLength;
        public long FadeInLength
        {
            get { return _fadeInLength; }
            set { _fadeInLength = value; }
        }

        private long _fadeOutStartPosition;
        public long FadeOutStartPosition
        {
            get { return _fadeOutStartPosition; }
            set { _fadeOutStartPosition = value; }
        }

        private long _fadeOutLength;
        public long FadeOutLength
        {
            get { return Selection.Length - FadeOutStartPosition; }
            set { Selection.Length = FadeOutStartPosition + value; }
        }
    }
}