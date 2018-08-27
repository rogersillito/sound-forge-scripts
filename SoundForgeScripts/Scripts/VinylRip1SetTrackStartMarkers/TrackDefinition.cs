using SoundForge;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    public class TrackDefinition
    {
        public int Number;
        public long StartPosition;
        public long EndPosition;

        public SfAudioSelection WholeTrackSelection()
        {
            return new SfAudioSelection(StartPosition, Length);
        }

        public long Length
        {
            get { return EndPosition - StartPosition; }
        }
    }
}