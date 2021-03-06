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

        public void DumpToScriptWindow()
        {
            _output.ToScriptWindow("Found {0} tracks:", Count);
            foreach (SplitTrackDefinition track in this)
            {
                _output.ToScriptWindow("{0}:\t{1}\t{2}\t(Start fade @ {3})", track.Number,
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.TrackRegion.Start)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.GetSelectionWithFades().Length)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.FadeOutStartPosition)));
            }
            _output.LineBreakToScriptWindow();
        }

        public SplitTrackDefinition AddNew(SfAudioMarker trackRegionMarker, int trackNumber, VinylRipOptions options)
        {
            SplitTrackDefinition track = new SplitTrackDefinition(this, _file, options, _markerFactory, _regionFactory, _output);
            this[trackNumber - 1] = track;
            track.Number = trackNumber;
            track.TrackRegion = trackRegionMarker;
            track.FadeInEndMarker = GetTrackFadeInEndMarker(track.Number, options.DefaultTrackFadeInLengthInSamples);
            var fadeOutLengthInSamples = _file.SecondsToPosition(options.DefaultTrackFadeOutLengthInSeconds);
            track.FadeOutEndMarker = GetTrackFadeOutEndMarker(track.Number, fadeOutLengthInSamples);
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

        public SplitTrackList InitTracks(VinylRipOptions options)
        {
            List<SfAudioMarker> trackRegions = GetTrackRegions();
            SetListBounds(trackRegions.Count);
            for (int trackNumber = trackRegions.Count; trackNumber > 0; trackNumber--)
            {
                SfAudioMarker trackRegion = trackRegions[trackNumber - 1];
                AddNew(trackRegion, trackNumber, options);
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

        internal void RenumberMarkers(int renumberFromTrack = 1)
        {
            var n = 1;
            foreach (var track in this)
            {
                if (track != null && n >= renumberFromTrack)
                {
                    track.Number = n;
                    track.TrackRegion.Name = _trackMarkerNameBuilder.GetRegionMarkerName(n);
                    track.FadeInEndMarker.Name = _trackMarkerNameBuilder.GetFadeInEndMarkerName(n);
                    track.FadeOutEndMarker.Name = _trackMarkerNameBuilder.GetFadeOutEndMarkerName(n);
                }
                n++;
            }
        }

        public void Delete(SplitTrackDefinition splitTrackDefinition)
        {
            Remove(splitTrackDefinition);
            var trackRegion = splitTrackDefinition.TrackRegion;
            var fadeInEndMarker = splitTrackDefinition.FadeInEndMarker;
            var fadeOutEndMarker = splitTrackDefinition.FadeOutEndMarker;
            splitTrackDefinition.TrackRegion = null;
            splitTrackDefinition.FadeInEndMarker = null;
            splitTrackDefinition.FadeOutEndMarker = null;

            _fileMarkers.Remove(trackRegion);
            _fileMarkers.Remove(fadeInEndMarker);
            _fileMarkers.Remove(fadeOutEndMarker);
            Sort();
            RenumberMarkers();
        }
    }
}
