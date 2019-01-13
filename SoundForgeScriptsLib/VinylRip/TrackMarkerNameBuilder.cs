namespace SoundForgeScriptsLib.VinylRip
{
    public interface ICreateTrackMarkerNames
    {
        string GetRegionMarkerName(int track);
        string GetFadeInEndMarkerName(int track);
        string GetFadeOutEndMarkerName(int track);
    }

    public class TrackMarkerNameBuilder : ICreateTrackMarkerNames
    {
        public string GetRegionMarkerName(int track) => $"{track:D4}{TrackRegionSuffix}";
        public string GetFadeInEndMarkerName(int track) => $"{track:D4}{TrackFadeInEndSuffix}";
        public string GetFadeOutEndMarkerName(int track) => $"{track:D4}{TrackFadeOutEndSuffix}";

        public const string TrackRegionSuffix = @"__TRACK__";
        public const string TrackFadeInEndSuffix = @"_FadeInEnd_";
        public const string TrackFadeOutEndSuffix = @"_FadeOutEnd_";
    }
}