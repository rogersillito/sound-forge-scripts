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
        private static readonly Regex RegionNameRegex = new Regex(string.Concat("^", TrackMarkerFactory.TrackRegionPrefix, "[0-9]{4}$"));

        private int _trackCount = 1;

        public SplitTrackList(ISfFileHost file, ICreateFadeMarkers markerFactory, ICreateTrackRegions regionFactory)
        {
            _file = file;
            _markerFactory = markerFactory;
            _regionFactory = regionFactory;
        }

        public SplitTrackDefinition AddNew(SfAudioMarker trackRegionMarker, long fadeInLength, long fadeOutLength)
        {
            SplitTrackDefinition track = new SplitTrackDefinition(this, _file, _markerFactory, _regionFactory);
            track.Number = _trackCount++;
            track.TrackRegion = trackRegionMarker;
            track.FadeInEndMarker = _markerFactory.CreateFadeInEnd(track.Number, trackRegionMarker.Start + fadeInLength);
            track.FadeOutEndMarker = _markerFactory.CreateFadeOutEnd(track.Number, MarkerHelper.GetMarkerEnd(track.TrackRegion) + fadeOutLength);
            Add(track);
            return track;
        }

        private List<SfAudioMarker> GetTrackRegions()
        {
            List<SfAudioMarker> trackMarkers = new List<SfAudioMarker>();
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (!marker.HasLength)
                    continue;
                if (!IsTrackRegion(marker.Name))
                    continue;
                trackMarkers.Add(marker);
            }
            return trackMarkers;
        }

        public static bool IsTrackRegion(string markerName)
        {
            return RegionNameRegex.IsMatch(markerName);
        }

        public SplitTrackList InitTracks(long defaultTrackFadeInLengthInSamples, long defaultTrackFadeOutLengthInSamples)
        {
            // TODO: I'm thinking we'll need a different method for synching gui-originating changes when we move between tracks in the vinyl 2 UI.
            // TODO: on synchronize - ensure fade end markers have no length, internal values match marker/regions

            GetTrackRegions().ForEach(tm => 
                AddNew(tm, defaultTrackFadeInLengthInSamples, defaultTrackFadeOutLengthInSamples)); ;

            foreach (SplitTrackDefinition track in this)
            {
                //TODO: what if a marker exists already? use it..
                track.FadeOutLength = defaultTrackFadeOutLengthInSamples;
                track.FadeInLength = defaultTrackFadeInLengthInSamples;
            }
            return this;
        }
    }
}