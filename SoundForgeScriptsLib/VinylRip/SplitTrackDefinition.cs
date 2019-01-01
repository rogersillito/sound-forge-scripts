using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib.VinylRip
{
    public class SplitTrackDefinition
    {
        private readonly SplitTrackList _splitTrackList;
        private readonly ISfFileHost _originalFile;
        private readonly ICreateFadeMarkers _markerFactory;
        private readonly ICreateTrackRegions _regionFactory;

        public SplitTrackDefinition(SplitTrackList splitTrackList, ISfFileHost file, ICreateFadeMarkers markerFactory, ICreateTrackRegions regionFactory)
        {
            _splitTrackList = splitTrackList;
            _originalFile = file;
            _markerFactory = markerFactory;
            _regionFactory = regionFactory;
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
                return _fadeInEndMarker ?? (_fadeInEndMarker = _markerFactory.CreateFadeInEnd(Number, TrackRegion.Start));
            }
            set
            {
                _fadeInEndMarker = value;
            }
        }

        private SfAudioMarker _fadeOutEndMarker;
        public SfAudioMarker FadeOutEndMarker
        {
            get
            {
                return _fadeOutEndMarker ?? (_fadeOutEndMarker = _markerFactory.CreateFadeOutEnd(Number, TrackRegion.Start));
            }
            set
            {
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
                if (value < 0) value = 0;
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
                if (value < 0) value = 0;
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