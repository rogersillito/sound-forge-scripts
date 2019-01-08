namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    //TODO: this duplicates VinylRip3FinalTrackProcessing.FindTracksOptions
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

        //TODO: this duplicates VinylRip1.FindTracksOptions.MinimumTrackLengthInSeconds
        private double _minimumTrackLengthInSeconds;
        public double MinimumTrackLengthInSeconds
        {
            get { return _minimumTrackLengthInSeconds; }
            set { _minimumTrackLengthInSeconds = value; }
        }
    }
}