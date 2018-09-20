using System.Collections.Generic;
using SoundForge;

namespace SoundForgeScripts.Scripts.VinylRip2FinalTrackProcessing
{
    public class TrackList : List<TrackDefinition>
    {
        private readonly FindTracksOptions _findTracksOptions;
        private readonly ISfFileHost _file;

        public TrackList(FindTracksOptions findTracksOptions, ISfFileHost file)
        {
            _findTracksOptions = findTracksOptions;
            _file = file;
        }

        public bool CanAddNextTrack(long nextTrackStartPosition)
        {
            if (LastAdded == null)
                return true;

            // would track gap be too short ?
            long minimumAllowableStartPosition = LastAdded.EndPosition + _file.SecondsToPosition(_findTracksOptions.MinimumTrackGapInSeconds);
            return minimumAllowableStartPosition <= nextTrackStartPosition;
        }

        public bool CanSetTrackBreak()
        {
            if (LastAdded == null)
                return true;

            // would track length be too short ?
            return _file.PositionToSeconds(LastAdded.Length) >= _findTracksOptions.MinimumTrackLengthInSeconds;
        }

        public TrackDefinition LastAdded
        {
            get { return Count > 0 ? this[Count - 1] : null; }
        }

        public void AddNew()
        {
            Add(new TrackDefinition());
        }
    }
}