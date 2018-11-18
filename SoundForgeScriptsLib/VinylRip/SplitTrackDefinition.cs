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

        public override int GetHashCode()
        {
            return Marker.Ident;
        }

        public bool CanAddFadeIn
        {
            get { return FadeInLength > 0; }
        }

        public bool CanAddFadeOut
        {
            get { return FadeOutStartPosition < Selection.Length; }
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

        private SfAudioMarker _marker;
        public SfAudioMarker Marker
        {
            get { return _marker; }
            set { _marker = value; }
        }

        private SfAudioMarker _fadeInEndMarker;
        public SfAudioMarker FadeInEnd
        {
            get { return _fadeInEndMarker; }
            set { _fadeInEndMarker = value; }
        }

        private long _fadeInLength;
        public long FadeInLength
        {
            get { return _fadeInLength; }
            set { _fadeInLength = value; }
        }

        public long FadeOutStartPosition
        {
            get { return Marker.Start + Marker.Length; }
            set { Marker.Length = Marker.Start + value; }
        }

        public long FadeOutLength
        {
            get { return Selection.Length - FadeOutStartPosition; }
            set
            {
                Selection.Length = FadeOutStartPosition + value;
                //TODO: implement once we have fadeinEndmarker,fadeoutendmarker
                _splitTrackList.Constrain(this);
            }
        }
    }
}