using SoundForgeScriptsLib;

namespace SoundForgeScripts.Scripts.VinylRip2FinalTrackProcessing
{
    public class FindTracksOptions
    {
        private double _minimumTrackGapInSeconds;
        private double _minimumTrackLengthInSeconds;
        private long _trackFadeInLengthInSamples;
        private double _trackAddFadeOutLengthInSeconds;

        public double MinimumTrackGapInSeconds
        {
            get { return _minimumTrackGapInSeconds; }
            set { _minimumTrackGapInSeconds = value; }
        }

        public double MinimumTrackLengthInSeconds
        {
            get { return _minimumTrackLengthInSeconds; }
            set { _minimumTrackLengthInSeconds = value; }
        }

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

        public void Validate()
        {
            //TODO: instead of validating, set defaults? use in an initial UI to allow reconfiguring?
            const double minTrackGap = 0.5;
            if (_minimumTrackGapInSeconds < minTrackGap)
                throw new ScriptAbortedException("MinimumTrackGapInSeconds must be >= {0}", minTrackGap);

            const double minTrackLength = 5.0;
            if (MinimumTrackLengthInSeconds < minTrackLength)
                throw new ScriptAbortedException("MinimumTrackLengthInSeconds must be >= {0}", minTrackLength);
        }
    }
}