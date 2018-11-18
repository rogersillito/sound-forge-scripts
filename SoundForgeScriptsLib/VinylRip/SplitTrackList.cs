using System.Collections.Generic;
using System.Text.RegularExpressions;
using SoundForge;

namespace SoundForgeScriptsLib.VinylRip
{
    public class SplitTrackList: List<SplitTrackDefinition>
    {
        private readonly ISfFileHost _file;
        private static readonly Regex RegionNameRegex = new Regex(string.Concat("^", TrackRegionPrefix, "[0-9]{4}$"));

        private int _trackCount = 1;

        public const string TrackRegionPrefix = @"__TRACK__";

        public SplitTrackList(ISfFileHost file)
        {
            _file = file;
        }

        public SplitTrackDefinition AddNew()
        {
            SplitTrackDefinition track = new SplitTrackDefinition(this, _file);
            track.Number = _trackCount++;
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
            List<SfAudioMarker> trackMarkers = GetTrackRegions();

            for (int i = 0; i < trackMarkers.Count; i++)
            {
                SfAudioMarker marker = trackMarkers[i];
                long maxEndPosition = _file.Length; // cannot be past end of file
                if (i < trackMarkers.Count - 1)
                {
                    maxEndPosition = trackMarkers[i + 1].Start; // cannot overlap next track
                }
                long maxLength = maxEndPosition - marker.Start;
                long lengthWithFadeOut = marker.Length + defaultTrackFadeOutLengthInSamples;
                if (lengthWithFadeOut > maxLength)
                    lengthWithFadeOut = maxLength;

                SplitTrackDefinition track = AddNew();
                track.Marker = marker;
                track.Selection = new SfAudioSelection(marker.Start, lengthWithFadeOut);
                track.FadeInEnd = new SfAudioMarker(); // TODO!
                //track.FadeOutEnd = new SfAudioMarker(); // TODO!
                track.FadeInLength = defaultTrackFadeInLengthInSamples;
            }
            return this;
        }

        public void Constrain(SplitTrackDefinition constrainedTrack)
        {
//            int idx =  IndexOf(constrainedTrack);
//            if (idx < Count - 1)
//            {
//                SplitTrackDefinition next = this[idx + 1];
//                if (constrainedTrack.Marker.Start + constrainedTrack.Selection.Length > next.Marker.Start)
//                {
//constrainedTrack.Selection

//                }
//            }

//            throw new System.NotImplementedException();
        }
    }
}