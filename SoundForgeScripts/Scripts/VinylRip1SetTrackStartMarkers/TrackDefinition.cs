using SoundForge;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    public class TrackDefinition
    {
        private readonly TrackList _trackList;
        public int Number;
        public long StartPosition;
        public long EndPosition;

        public TrackDefinition(TrackList trackList)
        {
            _trackList = trackList;
        }

        public SfAudioSelection WholeTrackSelection()
        {
            return new SfAudioSelection(StartPosition, Length);
        }

        public long Length
        {
            get { return EndPosition - StartPosition; }
        }

        public bool IsLast
        {
            get { return _trackList.LastAdded == this; }
        }
    }
}