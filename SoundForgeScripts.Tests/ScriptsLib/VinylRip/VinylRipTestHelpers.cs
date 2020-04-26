using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Tests.ScriptsLib.VinylRip
{
    public class VinylRipTestHelpers
    {
        /// <summary>
        /// An implausible default fade out length (seconds) value: allows mock calls triggered by the incoming property of <see cref="VinylRipOptions"/> to be setup with appropriate sample values.
        /// </summary>
        public const long TrackFadeOutLengthInSecondsForMockSetup = 999999999999;

        /// <summary>
        /// An implausible minimum track length (seconds) value: allows mock calls triggered by the incoming property of <see cref="VinylRipOptions"/> to be setup with appropriate sample values.
        /// </summary>
        public const long MinimumTrackLengthInSecondsForMockSetup = 888888888888;
    }
}
