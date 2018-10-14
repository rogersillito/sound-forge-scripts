using SoundForge;

namespace SoundForgeScripts.Scripts.VinylRip3FinalTrackProcessing
{
    public class SplitTrackDefinition
    {
        private readonly SplitTrackList _splitTrackList;
        private readonly ISfFileHost _originalFile;

        public int Number;
        public SfAudioSelection Selection;
        public SfAudioMarker RegionFound;
        public long FadeInLength;
        public long FadeOutStartPosition;

        public SplitTrackDefinition(SplitTrackList splitTrackList, ISfFileHost file)
        {
            _splitTrackList = splitTrackList;
            _originalFile = file;
        }

        public bool CanAddFadeIn
        {
            get { return FadeInLength > 0; }
        }

        public bool CanAddFadeOut
        {
            get { return FadeOutStartPosition < Selection.Length; }
        }
    }
}