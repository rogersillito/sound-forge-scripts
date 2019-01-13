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
        private readonly ICreateTrackMarkerNames _markerNameBuilder;

        public TrackMarkerFactory(IFileMarkersWrapper markers, OutputHelper outputHelper, ICreateTrackMarkerNames markerNameBuilder)
        {
            _markers = markers;
            _outputHelper = outputHelper;
            _markerNameBuilder = markerNameBuilder;
        }

        public TrackMarkerFactory(IFileMarkersWrapper markers) : this(markers, null, new TrackMarkerNameBuilder())
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
            string name = _markerNameBuilder.GetRegionMarkerName(track);
            SfAudioMarker marker = new SfAudioMarker(startPosition, length) { Name = name };
            return AddMarkerToFile(marker);
        }

        public SfAudioMarker CreateFadeInEnd(int track, long startPosition)
        {
            string name = _markerNameBuilder.GetFadeInEndMarkerName(track);
            SfAudioMarker marker = new SfAudioMarker(startPosition) { Name = name };
            return AddMarkerToFile(marker);
        }

        public SfAudioMarker CreateFadeOutEnd(int track, long startPosition)
        {
            string name = _markerNameBuilder.GetFadeOutEndMarkerName(track);
            SfAudioMarker marker = new SfAudioMarker(startPosition) { Name = name };
            return AddMarkerToFile(marker);
        }
    }
}