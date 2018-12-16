using SoundForge;

namespace SoundForgeScriptsLib.VinylRip
{
    public interface ICreateFadeMarkers
    {
        SfAudioMarker CreateFadeInEnd(int track, long startPosition);
        SfAudioMarker CreateFadeOutEnd(int track, long startPosition);
    }

    public interface ICreateTrackRegions
    {
        SfAudioMarker CreateRegion(int track, long startPosition, long length);
    }

    public class TrackMarkerFactory : ICreateFadeMarkers, ICreateTrackRegions
    {
        readonly ISfFileHost _file;

        public TrackMarkerFactory(ISfFileHost file)
        {
            _file = file;
        }

        public SfAudioMarker CreateRegion(int track, long startPosition, long length)
        {
            var name = string.Format("{0}{1:D4}", TrackRegionPrefix, track);
            return new SfAudioMarker(startPosition, length) { Name = name };
        }

        public SfAudioMarker CreateFadeInEnd(int track, long startPosition)
        {
            var name = string.Format("{0}{1:D4}", TrackFadeInEndPrefix, track);
            return new SfAudioMarker(startPosition) { Name = name };
        }

        public SfAudioMarker CreateFadeOutEnd(int track, long startPosition)
        {
            var name = string.Format("{0}{1:D4}", TrackFadeOutEndPrefix, track);
            return new SfAudioMarker(startPosition) { Name = name };
        }

        public const string TrackRegionPrefix = @"__TRACK__";
        public const string TrackFadeInEndPrefix = @"_FadeInEnd_";
        public const string TrackFadeOutEndPrefix = @"_FadeOutEnd_";
    }
}