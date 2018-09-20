using SoundForge;

namespace SoundForgeScripts.Scripts.VinylRip2FinalTrackProcessing
{
    public class ScanResult
    {
        public long WindowNumber;
        public long SelectionStart;
        public long SelectionEnd;
        public SfAudioStatistics Ch1Statistics;
        public SfAudioStatistics Ch2Statistics;

        public double GetMaxRmsLevel()
        {
            double loudest = Ch1Statistics.RMSLevel;
            if (Ch2Statistics.RMSLevel > loudest)
                loudest = Ch2Statistics.RMSLevel;
            return SfHelpers.RatioTodB(loudest);
        }

        public bool RmsLevelExceeds(double noisefloorThresholdInDecibels)
        {
            return GetMaxRmsLevel() >= noisefloorThresholdInDecibels;
        }
    }
}