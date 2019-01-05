using System.Collections.Generic;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib.VinylRip
{
    public class SplitTrackList : List<SplitTrackDefinition>
    {
        private readonly ISfFileHost _file;
        private readonly ICreateFadeMarkers _markerFactory;
        private readonly ICreateTrackRegions _regionFactory;
        private readonly ITrackMarkerSpecifications _markerSpecifications;
        private readonly IOutputHelper _output;

        public SplitTrackList(ISfFileHost file, ICreateFadeMarkers markerFactory, ICreateTrackRegions regionFactory, ITrackMarkerSpecifications markerSpecifications, IOutputHelper output)
        {
            _file = file;
            _markerFactory = markerFactory;
            _regionFactory = regionFactory;
            _markerSpecifications = markerSpecifications;
            _output = output;
        }

        public SplitTrackDefinition AddNew(SfAudioMarker trackRegionMarker, int trackNumber, long fadeInLength, long fadeOutLength)
        {
            SplitTrackDefinition track = new SplitTrackDefinition(this, _file, _markerFactory, _regionFactory, _output);
            this[trackNumber - 1] = track;
            track.Number = trackNumber;
            track.TrackRegion = trackRegionMarker;
            track.FadeInEndMarker = GetTrackFadeInEndMarker(track.Number, fadeInLength);
            track.FadeOutEndMarker = GetTrackFadeOutEndMarker(track.Number, fadeOutLength);
            return track;
        }

        private SfAudioMarker GetTrackFadeInEndMarker(int track, long fadeLength)
        {
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (_markerSpecifications.IsTrackFadeInEndMarker(marker, track))
                    return marker;
            }
            SplitTrackDefinition thisTrack = GetTrack(track);
            thisTrack.FadeInLength = fadeLength;
            return thisTrack.FadeInEndMarker;
        }

        private SfAudioMarker GetTrackFadeOutEndMarker(int track, long fadeLength)
        {
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (_markerSpecifications.IsTrackFadeOutEndMarker(marker, track))
                    return marker;
            }
            SplitTrackDefinition thisTrack = GetTrack(track);
            thisTrack.FadeOutLength = fadeLength;
            return thisTrack.FadeOutEndMarker;
        }

        public SplitTrackDefinition GetTrack(int number)
        {
            foreach (var track in this)
            {
                if (track == null)
                    continue;
                if (track.Number == number)
                    return track;
            }
            return null;
        }

        private List<SfAudioMarker> GetTrackRegions()
        {
            List<SfAudioMarker> trackMarkers = new List<SfAudioMarker>();
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (!_markerSpecifications.IsTrackRegion(marker))
                    continue;
                trackMarkers.Add(marker);
            }
            return trackMarkers;
        }

        public SplitTrackList InitTracks(long defaultTrackFadeInLengthInSamples, long defaultTrackFadeOutLengthInSamples)
        {
            // TODO: I'm thinking we'll need a different method for synching gui-originating changes when we move between tracks in the vinyl 2 UI.
            List<SfAudioMarker> trackRegions = GetTrackRegions();
            SetListBounds(trackRegions.Count);
            for (int trackNumber = trackRegions.Count; trackNumber > 0; trackNumber--)
            {
                SfAudioMarker trackRegion = trackRegions[trackNumber - 1];
                SplitTrackDefinition track = AddNew(trackRegion, trackNumber, defaultTrackFadeInLengthInSamples, defaultTrackFadeOutLengthInSamples);
            }
            return this;
        }

        private void SetListBounds(int trackCount)
        {
            Capacity = trackCount;
            for (int i = 0; i < Capacity; i++)
            {
                if (Count == i)
                    Insert(i, null);
            }
        }
    }
}