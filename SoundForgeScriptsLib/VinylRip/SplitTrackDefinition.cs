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

        public SfAudioSelection GetSelectionWithFades()
        {
            return new SfAudioSelection(TrackRegion.Start, FadeOutEndMarker.Start - TrackRegion.Start);
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
            get
            {
                if (_fadeInEndMarker == null) _fadeInEndMarker = new SfAudioMarker(TrackRegion.Start);
                //TODO: for these null checks - add to track region list?  I think so..
                return _fadeInEndMarker;
            }
            set
            {
                if (_fadeInEndMarker == null) _fadeInEndMarker = new SfAudioMarker(TrackRegion.Start);
                _fadeInEndMarker = value;
            }
        }

        private SfAudioMarker _fadeOutEndMarker;
        public SfAudioMarker FadeOutEndMarker
        {
            get
            {
                if (_fadeOutEndMarker == null) _fadeOutEndMarker = new SfAudioMarker(FadeOutStartPosition);
                return _fadeOutEndMarker;
            }
            set
            {
                if (_fadeOutEndMarker == null) _fadeOutEndMarker = new SfAudioMarker(FadeOutStartPosition);
                _fadeOutEndMarker = value;
            }
        }

        public long FadeInLength
        {
            get
            {
                if (FadeInEndMarker.Start < TrackRegion.Start)
                    return 0;
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
            get
            {
                if (FadeOutEndMarker.Start < FadeOutStartPosition)
                    return 0;
                return FadeOutEndMarker.Start - FadeOutStartPosition;
            }
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
            }
        }
    }
}