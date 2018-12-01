using System.Collections.Generic;
using System.Text.RegularExpressions;
using SoundForge;
using SoundForgeScriptsLib.Utils;

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

        public class asdfsd : SfAudioMarker
        {

        }

        public SplitTrackList InitTracks(long defaultTrackFadeInLengthInSamples, long defaultTrackFadeOutLengthInSamples)
        {
            List<SfAudioMarker> trackMarkers = GetTrackRegions();
            foreach (SfAudioMarker trackRegionMarker in trackMarkers)
            {
                SplitTrackDefinition track = AddNew();
                track.TrackRegion = trackRegionMarker;
            }

            foreach (SplitTrackDefinition track in this)
            //for (int i = 0; i < trackMarkers.Count; i++)
            {
                //long maxEndPosition = _file.Length; // cannot be past end of file
                //if (i < trackMarkers.Count - 1)
                //{
                //    maxEndPosition = trackMarkers[i + 1].Start; // cannot overlap next track
                //}
                //long maxLength = maxEndPosition - track.TrackRegion.Start;
                //long lengthWithFadeOut = track.TrackRegion.Length + defaultTrackFadeOutLengthInSamples;
                //if (lengthWithFadeOut > maxLength)
                //    lengthWithFadeOut = maxLength;

        


                //TODO: what if a marker exists already? use it..
                //TODO: and set the 2 marker names.. which will be how we'l identify pre-existing ones
                track.FadeInEndMarker.Name = "TODO"; 

                //TODO: where i left this: ****************************
                // have tried moving the logic for init tracks (setting fade in/out) onto the track definition
                // this has broken some tests...
                // I'm thinking we'll need a different method for synching gui-originating changes when we move between tracks in the vinyl 2 UI.
                track.FadeOutLength = defaultTrackFadeOutLengthInSamples;
                track.FadeInLength = defaultTrackFadeInLengthInSamples;

                // TODO:do we need this - or should we be creating it on demand when we need a selction?
                track.Selection = new SfAudioSelection(track.TrackRegion.Start, track.FadeOutStartPosition + track.FadeInLength);

                //track.FadeOutEndMarker = new SfAudioMarker(trackRegionMarker.Start + lengthWithFadeOut); // TODO!
                //track.FadeInLength = defaultTrackFadeInLengthInSamples;
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