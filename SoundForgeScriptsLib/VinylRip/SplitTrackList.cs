using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        private int _trackCount = 1;

        public SplitTrackList(ISfFileHost file, ICreateFadeMarkers markerFactory, ICreateTrackRegions regionFactory, ITrackMarkerSpecifications markerSpecifications)
        {
            _file = file;
            _markerFactory = markerFactory;
            _regionFactory = regionFactory;
            _markerSpecifications = markerSpecifications;
        }

        public SplitTrackDefinition AddNew(SfAudioMarker trackRegionMarker, long fadeInLength, long fadeOutLength)
        {
            SplitTrackDefinition track = new SplitTrackDefinition(this, _file, _markerFactory, _regionFactory);
            track.Number = _trackCount++;
            track.TrackRegion = trackRegionMarker;
            track.FadeInEndMarker = GetTrackFadeInEndMarker(track.Number, trackRegionMarker.Start + fadeInLength);
            track.FadeOutEndMarker = GetTrackFadeOutEndMarker(track.Number, MarkerHelper.GetMarkerEnd(track.TrackRegion) + fadeOutLength);
            Add(track);
            return track;
        }

        private SfAudioMarker GetTrackFadeInEndMarker(int track, long position)
        {
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (_markerSpecifications.IsTrackFadeInEndMarker(marker, track))
                    return marker;
            }
            return _markerFactory.CreateFadeInEnd(track, position);
        }

        private SfAudioMarker GetTrackFadeOutEndMarker(int track, long position)
        {
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (_markerSpecifications.IsTrackFadeOutEndMarker(marker, track))
                    return marker;
            }
            return _markerFactory.CreateFadeOutEnd(track, position);
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
            // TODO: on synchronize - ensure fade end markers have no length, internal values match marker/regions

            foreach (SfAudioMarker trackRegion in GetTrackRegions())
            {
                AddNew(trackRegion, defaultTrackFadeInLengthInSamples, defaultTrackFadeOutLengthInSamples);
            }

            foreach (SplitTrackDefinition track in this)
            {
                track.FadeOutLength = defaultTrackFadeOutLengthInSamples;
                track.FadeInLength = defaultTrackFadeInLengthInSamples;
            }
            return this;
        }
    }
}