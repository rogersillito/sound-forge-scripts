using System.Windows.Forms;
using SoundForge;
using SoundForgeScriptsLib;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    public class WindowTasks
    {
        public static void SelectBothChannels(ISfDataWnd window)
        {
            if (window.File.Channels != 2)
                throw new ScriptAbortedException("Expected a 2-channel file.");
            switch (window.Selection.ChanMask)
            {
                case 0:
                    // both
                    return;
                case 1:
                    // left-only
                    window.ForwardKey(Keys.Tab);
                    window.ForwardKey(Keys.Tab);
                    return;
                case 2:
                    // right-only
                    window.ForwardKey(Keys.Tab);
                    break;
            }
        }

        public static SfAudioSelection NewSelectionUsingEndPosition(long ccStart, long ccEnd)
        {
            return new SfAudioSelection(ccStart, ccEnd - ccStart);
        }
    }
}