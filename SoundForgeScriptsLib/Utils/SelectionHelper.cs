using SoundForge;

namespace SoundForgeScriptsLib.Utils
{
    public class SelectionHelper
    {
        public static void SetSelectionEnd(SfAudioSelection selection, long ccEndIn)
        {
            if (selection == null || ccEndIn < selection.Start)
                return;
            selection.Length = ccEndIn - selection.Start;
        }

        public static long GetSelectionEnd(SfAudioSelection selection)
        {
            return selection.Start + selection.Length;
        }

        public static string PrettyPrint(SfAudioSelection selection)
        {
            return $"Start: {selection.Start}, End: {GetSelectionEnd(selection)}, Length: {selection.Length}";
        }
    }
}
