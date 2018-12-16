using SoundForge;
using SoundForgeScriptsLib.Utils;

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
        private readonly OutputHelper _outputHelper;

        public TrackMarkerFactory(ISfFileHost file, OutputHelper outputHelper)
        {
            _file = file;
            _outputHelper = outputHelper;
        }

        public TrackMarkerFactory(ISfFileHost file): this (file, null)
        {
        }

        private SfAudioMarker AddMarkerToFile(SfAudioMarker marker)
        {
            int idx = _file.Markers.Add(marker);
            if (_outputHelper != null) _outputHelper.ToScriptWindow("Added Marker idx=" + idx);
            return marker;
        }

        public SfAudioMarker CreateRegion(int track, long startPosition, long length)
        {
            var name = string.Format("{0}{1:D4}", TrackRegionPrefix, track);
            return AddMarkerToFile(new SfAudioMarker(startPosition, length) { Name = name });
        }

        public SfAudioMarker CreateFadeInEnd(int track, long startPosition)
        {
            var name = string.Format("{0}{1:D4}", TrackFadeInEndPrefix, track);
            return AddMarkerToFile(new SfAudioMarker(startPosition) { Name = name });
        }

        public SfAudioMarker CreateFadeOutEnd(int track, long startPosition)
        {
            var name = string.Format("{0}{1:D4}", TrackFadeOutEndPrefix, track);
            return AddMarkerToFile(new SfAudioMarker(startPosition) { Name = name });
        }

        public const string TrackRegionPrefix = @"__TRACK__";
        public const string TrackFadeInEndPrefix = @"_FadeInEnd_";
        public const string TrackFadeOutEndPrefix = @"_FadeOutEnd_";
    }
}