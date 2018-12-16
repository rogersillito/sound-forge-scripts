using System.Text.RegularExpressions;
using SoundForge;

namespace SoundForgeScriptsLib.VinylRip
{
    public interface ITrackMarkerSpecifications
    {
        bool IsTrackRegion(SfAudioMarker marker);
        bool IsTrackFadeInEndMarker(SfAudioMarker marker, int track);
        bool IsTrackFadeOutEndMarker(SfAudioMarker marker, int track);
    }

    public class TrackMarkerSpecifications : ITrackMarkerSpecifications
    {
        private static readonly Regex RegionNameRegex = new Regex(string.Concat("^", TrackMarkerFactory.TrackRegionPrefix, "([0-9]{4})$"));
        public static readonly Regex FadeInEndNameRegex = new Regex(string.Concat("^", TrackMarkerFactory.TrackFadeInEndPrefix, "([0-9]{4})$"));
        public static readonly Regex FadeOutEndNameRegex = new Regex(string.Concat("^", TrackMarkerFactory.TrackFadeOutEndPrefix, "([0-9]{4})$"));

        public bool IsTrackRegion(SfAudioMarker marker)
        {
            return marker.HasLength && RegionNameRegex.IsMatch(marker.Name);
        }

        public bool IsTrackFadeInEndMarker(SfAudioMarker marker)
        {
            return !marker.HasLength && FadeInEndNameRegex.IsMatch(marker.Name);
        }

        public bool IsTrackFadeInEndMarker(SfAudioMarker marker, int track)
        {
            if (!IsTrackFadeInEndMarker(marker)) return false;
            Match m = FadeInEndNameRegex.Match(marker.Name);
            return int.Parse(m.Groups[1].Value).Equals(track);
        }

        public bool IsTrackFadeOutEndMarker(SfAudioMarker marker)
        {
            return !marker.HasLength && FadeOutEndNameRegex.IsMatch(marker.Name);
        }

        public bool IsTrackFadeOutEndMarker(SfAudioMarker marker, int track)
        {
            if (!IsTrackFadeOutEndMarker(marker)) return false;
            Match m = FadeOutEndNameRegex.Match(marker.Name);
            return int.Parse(m.Groups[1].Value).Equals(track);
        }
    }
}