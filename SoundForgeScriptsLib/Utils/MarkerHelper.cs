using SoundForge;

namespace SoundForgeScriptsLib.Utils
{
    public class MarkerHelper
    {
        public static void SetMarkerEnd(SfAudioMarker marker, long ccEndIn)
        {
            if (marker == null || ccEndIn < marker.Start)
                return;
            marker.Length = ccEndIn - marker.Start;
        }

        public static long GetMarkerEnd(SfAudioMarker marker)
        {
            return marker.Start + marker.Length;
        }
    }
}