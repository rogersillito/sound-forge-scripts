using SoundForge;
using SoundForgeScriptsLib;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    public class FindTracksOptions
    {
        private double _scanWindowLengthInSeconds;
        private double _gapNoisefloorThresholdInDecibels;
        private double _minimumTrackGapInSeconds;
        private double _minimumTrackLengthInSeconds;
        private long _startScanFilePositionInSamples;
        private long _defaultTrackFadeInLengthInSamples;
        private long _defaultTrackFadeOutLengthInSamples;

        public double ScanWindowLengthInSeconds
        {
            get { return _scanWindowLengthInSeconds; }
            set { _scanWindowLengthInSeconds = value; }
        }

        public long ScanWindowLengthInSamples(ISfFileHost file)
        {
            return file.SecondsToPosition(_scanWindowLengthInSeconds);
        }

        public double GapNoisefloorThresholdInDecibels
        {
            get { return _gapNoisefloorThresholdInDecibels; }
            set { _gapNoisefloorThresholdInDecibels = value; }
        }

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

        public long StartScanFilePositionInSamples
        {
            get { return _startScanFilePositionInSamples; }
            set { _startScanFilePositionInSamples = value; }
        }

        public long DefaultTrackFadeInLengthInSamples
        {
            get { return _defaultTrackFadeInLengthInSamples; }
            set { _defaultTrackFadeInLengthInSamples = value; }
        }

        public long DefaultTrackFadeOutLengthInSamples
        {
            get { return _defaultTrackFadeOutLengthInSamples; }
            set { _defaultTrackFadeOutLengthInSamples = value; }
        }

        public void Validate()
        {
            //TODO: instead of validating, set defaults? use in an initial UI to allow reconfiguring?
            const double minWinLength = 0.1;
            if (_scanWindowLengthInSeconds < minWinLength)
                throw new ScriptAbortedException("ScanWindowLengthInSeconds must be >= {0}", minWinLength);

            const double minNoiseFloor = -100;
            if (_gapNoisefloorThresholdInDecibels < minNoiseFloor)
                throw new ScriptAbortedException("GapNoisefloorThresholdInDecibels must be >= {0}", minNoiseFloor);

            const double minTrackGap = 0.5;
            if (_minimumTrackGapInSeconds < minTrackGap)
                throw new ScriptAbortedException("MinimumTrackGapInSeconds must be >= {0}", minTrackGap);

            const double minTrackLength = 5.0;
            if (MinimumTrackLengthInSeconds < minTrackLength)
                throw new ScriptAbortedException("MinimumTrackLengthInSeconds must be >= {0}", minTrackLength);
        }
    }
}
