using System.Collections.Generic;
using SoundForge;
using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    public class TrackList : List<TrackDefinition>
    {
        private readonly VinylRipOptions _vinylRipOptions;
        private readonly ISfFileHost _file;

        public TrackList(VinylRipOptions vinylRipOptions, ISfFileHost file)
        {
            _vinylRipOptions = vinylRipOptions;
            _file = file;
        }

        public bool CanAddNextTrack(long nextTrackStartPosition)
        {
            if (LastAdded == null)
                return true;

            // would track gap be too short ?
            long minimumAllowableStartPosition = LastAdded.EndPosition + _file.SecondsToPosition(_vinylRipOptions.MinimumTrackGapInSeconds);
            return minimumAllowableStartPosition <= nextTrackStartPosition;
        }

        public bool CanSetTrackBreak()
        {
            if (LastAdded == null)
                return true;

            // would track length be too short ?
            return _file.PositionToSeconds(LastAdded.Length) >= _vinylRipOptions.MinimumTrackLengthInSeconds;
        }

        public TrackDefinition LastAdded
        {
            get { return Count > 0 ? this[Count - 1] : null; }
        }

        public void AddNew()
        {
            Add(new TrackDefinition(this));
        }

        public long FileLength
        {
            get { return _file.Length; }
        }
    }
}