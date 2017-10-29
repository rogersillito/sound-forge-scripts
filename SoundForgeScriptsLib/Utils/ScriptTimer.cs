using System.Diagnostics;
using System.Globalization;

namespace SoundForgeScriptsLib.Utils
{
    public class ScriptTimer
    {
        private static Stopwatch _stopwatch;

        public static void Reset()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public static string Time()
        {
            return OutputHelper.FormatToTimeSpan(TimeSeconds());
        }

        public static long TimeMs()
        {
            return _stopwatch.ElapsedMilliseconds;
        }

        public static double TimeSeconds()
        {
            return (((double)TimeMs()) / 1000);
        }
    }
}