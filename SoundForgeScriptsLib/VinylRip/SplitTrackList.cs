using System.Collections.Generic;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib.VinylRip
{
    public class SplitTrackList : List<SplitTrackDefinition>
    {
        private readonly ISfFileHost _file;
        private readonly ICreateFadeMarkers _markerFactory;
        private readonly ICreateTrackRegions _regionFactory;
        private readonly ICreateTrackMarkerNames _trackMarkerNameBuilder;
        private readonly IFileMarkersWrapper _fileMarkers;
        private readonly ITrackMarkerSpecifications _markerSpecifications;
        private readonly IOutputHelper _output;
        private long _defaultFadeInLength;
        private long _defaultFadeOutLength;

        public SplitTrackList(ICreateFadeMarkers markerFactory, ICreateTrackRegions regionFactory, ICreateTrackMarkerNames trackMarkerNameBuilder, IFileMarkersWrapper fileMarkers, ITrackMarkerSpecifications markerSpecifications, IOutputHelper output)
        {
            _markerFactory = markerFactory;
            _regionFactory = regionFactory;
            _trackMarkerNameBuilder = trackMarkerNameBuilder;
            _fileMarkers = fileMarkers;
            _file = fileMarkers.File;
            _markerSpecifications = markerSpecifications;
            _output = output;
        }

        public SplitTrackDefinition AddNew(SfAudioMarker trackRegionMarker, int trackNumber, long fadeInLength, long fadeOutLength)
        {
            SplitTrackDefinition track = new SplitTrackDefinition(this, _file, _markerFactory, _regionFactory, _output);
            this[trackNumber - 1] = track;
            track.Number = trackNumber;
            track.TrackRegion = trackRegionMarker;
            track.FadeInEndMarker = GetTrackFadeInEndMarker(track.Number, fadeInLength);
            track.FadeOutEndMarker = GetTrackFadeOutEndMarker(track.Number, fadeOutLength);
            return track;
        }

        private SfAudioMarker GetTrackFadeInEndMarker(int track, long fadeLength)
        {
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (_markerSpecifications.IsTrackFadeInEndMarker(marker, track))
                    return marker;
            }
            SplitTrackDefinition thisTrack = GetTrack(track);
            thisTrack.FadeInLength = fadeLength;
            return thisTrack.FadeInEndMarker;
        }

        private SfAudioMarker GetTrackFadeOutEndMarker(int track, long fadeLength)
        {
            foreach (SfAudioMarker marker in _file.Markers)
            {
                if (_markerSpecifications.IsTrackFadeOutEndMarker(marker, track))
                    return marker;
            }
            SplitTrackDefinition thisTrack = GetTrack(track);
            thisTrack.FadeOutLength = fadeLength;
            return thisTrack.FadeOutEndMarker;
        }

        public SplitTrackDefinition GetTrack(int number)
        {
            foreach (var track in this)
            {
                if (track == null)
                    continue;
                if (track.Number == number)
                    return track;
            }
            return null;
        }

        private List<SfAudioMarker> GetTrackRegions()
        {
            var trackMarkers = new List<SfAudioMarker>();
            foreach (var marker in _file.Markers)
            {
                if (!_markerSpecifications.IsTrackRegion((SfAudioMarker)marker))
                    continue;
                trackMarkers.Add((SfAudioMarker)marker);
            }
            return trackMarkers;
        }

        public SplitTrackList InitTracks(long defaultTrackFadeInLengthInSamples, long defaultTrackFadeOutLengthInSamples)
        {
            _defaultFadeInLength = defaultTrackFadeInLengthInSamples;
            _defaultFadeOutLength = defaultTrackFadeOutLengthInSamples;
            // TODO: I'm thinking we'll need a different method for synching gui-originating changes when we move between tracks in the vinyl 2 UI.
            List<SfAudioMarker> trackRegions = GetTrackRegions();
            SetListBounds(trackRegions.Count);
            for (int trackNumber = trackRegions.Count; trackNumber > 0; trackNumber--)
            {
                SfAudioMarker trackRegion = trackRegions[trackNumber - 1];
                AddNew(trackRegion, trackNumber, defaultTrackFadeInLengthInSamples, defaultTrackFadeOutLengthInSamples);
            }
            return this;
        }

        private void SetListBounds(int trackCount)
        {
            Capacity = trackCount;
            for (int i = 0; i < Capacity; i++)
            {
                if (Count == i)
                    Insert(i, null);
            }
        }

        private void RenumberMarkers()
        {
            var n = 1;
            Sort();
            foreach (var track in this)
            {
                track.Number = n;
                track.TrackRegion.Name = _trackMarkerNameBuilder.GetRegionMarkerName(n);
                track.FadeInEndMarker.Name = _trackMarkerNameBuilder.GetFadeInEndMarkerName(n);
                track.FadeOutEndMarker.Name = _trackMarkerNameBuilder.GetFadeOutEndMarkerName(n);
                n++;
            }
        }

        public void Delete(SplitTrackDefinition splitTrackDefinition)
        {
            //TODO:  try remove by index?
            //TODO:  remove from STD first?
            Remove(splitTrackDefinition);//todo: remove std from list first??
            //var trackRegion = splitTrackDefinition.TrackRegion;
            //var fadeInEndMarker = splitTrackDefinition.FadeInEndMarker;
            //var fadeOutEndMarker = splitTrackDefinition.FadeOutEndMarker;
            splitTrackDefinition.TrackRegion = null;
            splitTrackDefinition.FadeInEndMarker = null;
            splitTrackDefinition.FadeOutEndMarker = null;

            //_fileMarkers.Remove(trackRegion);
            //_fileMarkers.Remove(fadeInEndMarker);
            //_fileMarkers.Remove(fadeOutEndMarker);
            //RenumberMarkers();
        }
    }
}