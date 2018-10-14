namespace SoundForgeScripts.Scripts.VinylRip3FinalTrackProcessing
{
    public class FindTracksOptions
    {
        private long _trackFadeInLengthInSamples;
        private double _trackAddFadeOutLengthInSeconds;
        public long TrackFadeInLengthInSamples
        {
            get { return _trackFadeInLengthInSamples; }
            set { _trackFadeInLengthInSamples = value; }
        }

        public double TrackAddFadeOutLengthInSeconds
        {
            get { return _trackAddFadeOutLengthInSeconds; }
            set { _trackAddFadeOutLengthInSeconds = value; }
        }
    }
}