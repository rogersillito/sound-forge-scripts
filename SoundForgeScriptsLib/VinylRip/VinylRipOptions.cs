using SoundForge;

namespace SoundForgeScriptsLib.VinylRip
{
    public class VinylRipOptions
    {
        public double DefaultTrackFadeOutLengthInSeconds { get; set; } = 3;

        public double ScanWindowLengthInSeconds { get; set; } = 1.0;

        public long ScanWindowLengthInSamples(ISfFileHost file)
        {
            return file.SecondsToPosition(ScanWindowLengthInSeconds);
        }

        public double GapNoisefloorThresholdInDecibels { get; set; } = -70;

        public double MinimumTrackGapInSeconds { get; set; } = 1;

        public double MinimumTrackLengthInSeconds { get; set; } = 10;

        public long StartScanFilePositionInSamples { get; set; }

        public long DefaultTrackFadeInLengthInSamples { get; set; } = 20;

        public void Validate()
        {
            //TODO: instead of validating, set defaults? use in an initial UI to allow reconfiguring?
            const double minWinLength = 0.1;
            if (ScanWindowLengthInSeconds < minWinLength)
                throw new ScriptAbortedException("ScanWindowLengthInSeconds must be >= {0}", minWinLength);

            const double minNoiseFloor = -100;
            if (GapNoisefloorThresholdInDecibels < minNoiseFloor)
                throw new ScriptAbortedException("GapNoisefloorThresholdInDecibels must be >= {0}", minNoiseFloor);

            const double minTrackGap = 0.5;
            if (MinimumTrackGapInSeconds < minTrackGap)
                throw new ScriptAbortedException("MinimumTrackGapInSeconds must be >= {0}", minTrackGap);

            const double minTrackLength = 5.0;
            if (MinimumTrackLengthInSeconds < minTrackLength)
                throw new ScriptAbortedException("MinimumTrackLengthInSeconds must be >= {0}", minTrackLength);
        }
    }
}
