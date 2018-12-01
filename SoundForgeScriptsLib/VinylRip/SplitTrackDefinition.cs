using SoundForge;
using SoundForgeScriptsLib.Utils;

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
            _trackRegionMarker = new SfAudioMarker();
            _trackRegionMarker.Type = MarkerType.Region;
            _fadeInEndMarker = new SfAudioMarker();
            _fadeOutEndMarker = new SfAudioMarker();
        }

        public bool CanAddFadeIn
        {
            get { return FadeInLength > 0; }
        }

        public bool CanAddFadeOut
        {
            get { return FadeOutStartPosition < FadeOutEndMarker.Start; }
        }

        public bool IsLastTrack
        {
            get { return _number == _splitTrackList.Count; }
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

        private SfAudioMarker _trackRegionMarker;
        public SfAudioMarker TrackRegion
        {
            get { return _trackRegionMarker; }
            set { _trackRegionMarker = value; }
        }

        private SfAudioMarker _fadeInEndMarker;
        public SfAudioMarker FadeInEndMarker
        {
            get { return _fadeInEndMarker; }
            set { _fadeInEndMarker = value; }
        }

        private SfAudioMarker _fadeOutEndMarker;
        public SfAudioMarker FadeOutEndMarker
        {
            get { return _fadeOutEndMarker; }
            set { _fadeOutEndMarker = value; }
        }

        public long FadeInLength
        {
            get
            {
                //TODO: handle marker deleted.. ALSO SETTER
                //if (_fadeInEndMarker == null) return TrackRegion.Start;
                return FadeInEndMarker.Start - TrackRegion.Start;
            }
            set
            {
                long fadeInEnd = TrackRegion.Start + value;
                if (fadeInEnd > FadeOutStartPosition)
                    fadeInEnd = FadeOutStartPosition;
                FadeInEndMarker.Start = fadeInEnd;
            }
        }

        public long FadeOutStartPosition
        {
            get { return MarkerHelper.GetMarkerEnd(TrackRegion); }
            set { MarkerHelper.SetMarkerEnd(TrackRegion, value); }
        }

        public long FadeOutLength
        {
            get { return Selection.Length - FadeOutStartPosition; }
            set
            {
                long maxEndPosition = _originalFile.Length; // cannot be past end of file
                if (!IsLastTrack)
                {
                    int nextIdx = _splitTrackList.IndexOf(this) + 1;
                    maxEndPosition = _splitTrackList[nextIdx].TrackRegion.Start; // cannot overlap next track
                }
                long maxLength = maxEndPosition - FadeOutStartPosition;
                if (value > maxLength)
                    value = maxLength;
                FadeOutEndMarker.Start = FadeOutStartPosition + value;

                //Selection.Length = FadeOutStartPosition + value;
                //FadeOutEndMarker.Start = Selection.Start + Selection.Length;
                //_splitTrackList.Constrain(this);
            }
        }
        
        // todo: synchronize- ensure fadeend markers have no length, internal values match marker/regions
    }
}