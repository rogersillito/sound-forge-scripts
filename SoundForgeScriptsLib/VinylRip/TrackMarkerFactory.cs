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
        readonly IFileMarkersWrapper _markers;
        private readonly OutputHelper _outputHelper;

        public TrackMarkerFactory(IFileMarkersWrapper markers, OutputHelper outputHelper)
        {
            _markers = markers;
            _outputHelper = outputHelper;
        }

        public TrackMarkerFactory(IFileMarkersWrapper markers) : this(markers, null)
        {
        }

        private SfAudioMarker AddMarkerToFile(SfAudioMarker marker)
        {
            int idx = _markers.Add(marker);
            marker = _markers[idx];
            _outputHelper?.ToScriptWindow($"Added Marker idx={idx} ({marker.Start} '{marker.Name}', type={marker.Type})");
            return marker;
        }

        public SfAudioMarker CreateRegion(int track, long startPosition, long length)
        {
            string name = $"{track:D4}{TrackRegionSuffix}";
            SfAudioMarker marker = new SfAudioMarker(startPosition, length) { Name = name };
            return AddMarkerToFile(marker);
        }

        public SfAudioMarker CreateFadeInEnd(int track, long startPosition)
        {
            string name = $"{track:D4}{TrackFadeInEndSuffix}";
            SfAudioMarker marker = new SfAudioMarker(startPosition) { Name = name };
            return AddMarkerToFile(marker);
        }

        public SfAudioMarker CreateFadeOutEnd(int track, long startPosition)
        {
            string name = $"{track:D4}{TrackFadeOutEndSuffix}";
            SfAudioMarker marker = new SfAudioMarker(startPosition) { Name = name };
            return AddMarkerToFile(marker);
        }

        public const string TrackRegionSuffix = @"__TRACK__";
        public const string TrackFadeInEndSuffix = @"_FadeInEnd_";
        public const string TrackFadeOutEndSuffix = @"_FadeOutEnd_";
    }
}