using System.Collections.Generic;
using System.Text.RegularExpressions;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib.VinylRip
{
    public class SplitTrackList : List<SplitTrackDefinition>
    {
        private readonly ISfFileHost _file;
        private static readonly Regex RegionNameRegex = new Regex(string.Concat("^", TrackRegionPrefix, "[0-9]{4}$"));

        private int _trackCount = 1;

        public const string TrackRegionPrefix = @"__TRACK__";

        public SplitTrackList(ISfFileHost file)
        {
            _file = file;
        }

        public SplitTrackDefinition AddNew(SfAudioMarker trackRegionMarker)
        {
            SplitTrackDefinition track = new SplitTrackDefinition(this, _file);
            track.Number = _trackCount++;
            track.TrackRegion = trackRegionMarker;
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

            GetTrackRegions().ForEach(tm => AddNew(tm)); ;

            foreach (SplitTrackDefinition track in this)
            {
                //TODO: what if a marker exists already? use it..

                track.FadeOutLength = defaultTrackFadeOutLengthInSamples;
                track.FadeInLength = defaultTrackFadeInLengthInSamples;

                //TODO: and set the 2 marker names.. which will be how we'll identify pre-existing ones
                track.FadeInEndMarker.Name = "TODO";
                track.FadeOutEndMarker.Name = "TODO";
            }
            return this;
        }
    }
}