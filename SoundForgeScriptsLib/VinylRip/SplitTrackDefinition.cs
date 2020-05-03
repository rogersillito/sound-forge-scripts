using System;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib.VinylRip
{
    public class SplitTrackDefinition : IComparable<SplitTrackDefinition>
    {
        private readonly SplitTrackList _splitTrackList;
        private readonly ISfFileHost _originalFile;
        private readonly ICreateFadeMarkers _markerFactory;
        private readonly ICreateTrackRegions _regionFactory;
        private readonly IOutputHelper _output;

        public SplitTrackDefinition(
            SplitTrackList splitTrackList,
            ISfFileHost file,
            VinylRipOptions options,
            ICreateFadeMarkers markerFactory,
            ICreateTrackRegions regionFactory,
            IOutputHelper output)
        {
            _options = options;
            _splitTrackList = splitTrackList;
            _originalFile = file;
            _markerFactory = markerFactory;
            _regionFactory = regionFactory;
            _output = output;
            _trackRegionMarker = new SfAudioMarker();
            _trackRegionMarker.Type = MarkerType.Region;
        }

        public bool AddFadeIn
        {
            get { return FadeInLength > 0; }
        }

        public bool AddFadeOut
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
            get { return _fadeInEndMarker ?? (_fadeInEndMarker = _markerFactory.CreateFadeInEnd(Number, TrackRegion.Start)); }
            set
            {
                _fadeInEndMarker = value;
            }
        }

        private SfAudioMarker _fadeOutEndMarker;
        private VinylRipOptions _options;

        public SfAudioMarker FadeOutEndMarker
        {
            get { return _fadeOutEndMarker ?? (_fadeOutEndMarker = _markerFactory.CreateFadeOutEnd(Number, TrackRegion.Start)); }
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

        public int CompareTo(SplitTrackDefinition other) => TrackRegion.Start.CompareTo(other.TrackRegion.Start);

        public bool CanMoveStartBy(long samples)
        {
            if (TrackRegion.Start + samples >= MarkerHelper.GetMarkerEnd(TrackRegion))
                return false;

            if (Number > 1)
            {
                if (TrackRegion.Start + samples < _splitTrackList.GetTrack(Number - 1).FadeOutEndMarker.Start)
                    return false;
            }
            else
            {
                if (TrackRegion.Start + samples < 0)
                    return false;
            }

            return true;
        }

        public void MoveStartBy(long samples)
        {
            TrackRegion.Start += samples;
            TrackRegion.Length -= samples;
            if (!CanMoveFadeInBy(samples))
                samples = MarkerHelper.GetMarkerEnd(TrackRegion) - FadeInEndMarker.Start;
            MoveFadeInBy(samples);
        }

        public bool CanMoveFadeInBy(long samples)
        {
            if (FadeInEndMarker.Start + samples < TrackRegion.Start)
                return false;

            if (FadeInEndMarker.Start + samples > MarkerHelper.GetMarkerEnd(TrackRegion))
                return false;

            return true;
        }

        public void MoveFadeInBy(long samples)
        {
            FadeInEndMarker.Start += samples;
        }

        public bool CanMoveEndBy(long samples)
        {
            var newEndPosition = MarkerHelper.GetMarkerEnd(TrackRegion) + samples;

            if (IsLastTrack && newEndPosition > _originalFile.Length)
                return false;

            if (!IsLastTrack && newEndPosition > _splitTrackList.GetTrack(Number + 1).TrackRegion.Start)
                return false;

            return newEndPosition > TrackRegion.Start;
        }

        public void MoveEndBy(long samples)
        {
            TrackRegion.Length += samples;

            if (CanMoveFadeOutBy(samples))
            {
                MoveFadeOutBy(samples);
            }
            else
            {
                FadeOutEndMarker.Start = IsLastTrack
                    ? _originalFile.Length
                    : _splitTrackList.GetTrack(Number + 1).TrackRegion.Start;
            }

            if (MarkerHelper.GetMarkerEnd(TrackRegion) < FadeInEndMarker.Start)
                FadeInEndMarker.Start = MarkerHelper.GetMarkerEnd(TrackRegion);
        }

        public bool CanMoveFadeOutBy(long samples)
        {
            if (samples < 0 && Math.Abs(samples) > FadeOutLength)
                return false;

            if (samples > 0)
            {
                if (!IsLastTrack && FadeOutEndMarker.Start + samples > _splitTrackList.GetTrack(Number + 1).TrackRegion.Start)
                    return false;

                if (IsLastTrack && FadeOutEndMarker.Start + samples > _originalFile.Length)

                    return false;
            }

            return true;
        }

        public void MoveFadeOutBy(long samples)
        {
            FadeOutEndMarker.Start += samples;
        }

        private long MinimumTrackInsertionLength =>
            _originalFile.SecondsToPosition(_options.DefaultTrackFadeOutLengthInSeconds) +
            _originalFile.SecondsToPosition(_options.MinimumTrackLengthInSeconds);

        private long PreceedingInsertionLimitPoint => Number > 1
            ? _splitTrackList.GetTrack(Number - 1).FadeOutEndMarker.Start
            : 0;

        private long FollowingInsertionLimitPoint => IsLastTrack
            ? _originalFile.Length
            : _splitTrackList.GetTrack(Number + 1).TrackRegion.Start;

        public bool CanInsertTrackBefore()
        {
            return PreceedingInsertionLimitPoint + MinimumTrackInsertionLength <= TrackRegion.Start;
        }

        public bool CanInsertTrackAfter()
        {
            return FadeOutEndMarker.Start + MinimumTrackInsertionLength <= FollowingInsertionLimitPoint;
        }

        public SplitTrackDefinition InsertTrackBefore()
        {
            var newTrackLength = TrackRegion.Start - _originalFile.SecondsToPosition(_options.DefaultTrackFadeOutLengthInSeconds) - PreceedingInsertionLimitPoint;
            var newTrackNumber = Number;
            var newRegion = _regionFactory.CreateRegion(newTrackNumber, PreceedingInsertionLimitPoint, newTrackLength);
            _splitTrackList.Insert(Number - 1, null);
            return CreateTrackInList(newTrackNumber, newRegion);
        }

        public SplitTrackDefinition InsertTrackAfter()
        {
            var newTrackLength = FollowingInsertionLimitPoint - FadeOutEndMarker.Start - _originalFile.SecondsToPosition(_options.DefaultTrackFadeOutLengthInSeconds);
            var newTrackNumber = Number + 1;
            var newRegion = _regionFactory.CreateRegion(newTrackNumber, FadeOutEndMarker.Start, newTrackLength);
            _splitTrackList.Insert(Number, null);
            return CreateTrackInList(newTrackNumber, newRegion);
        }

        private SplitTrackDefinition CreateTrackInList(int newTrackNumber, SfAudioMarker newRegion)
        {
            _splitTrackList.RenumberMarkers(newTrackNumber + 1);
            var newTrack = _splitTrackList.AddNew(newRegion, newTrackNumber, _options);
            _splitTrackList.RenumberMarkers();
            return newTrack;
        }
    }
}
