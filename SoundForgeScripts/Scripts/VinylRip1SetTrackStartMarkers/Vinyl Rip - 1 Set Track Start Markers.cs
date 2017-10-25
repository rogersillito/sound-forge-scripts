/* =======================================================================================================
 *	Script Name: Vinyl Rip - 1 Set Track Start Markers
 *	Description: Inserts track start points (after aggresively cleaning audio).
 *
 *	Initial State: Run with a file open a selection containing at least 2 seconds of track noise
 *	
 *	Parameters (Args):
 *		None
 *
 *	Output:
 *	    None
 *
 * ==================================================================================================== */

using SoundForge;
using SoundForgeScriptsLib;

namespace SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers
{
    public class EntryPoint
    {
        public void FromSoundForge(IScriptableApp app)
        {
            new SoundForgeScriptsLib.EntryPoints.SetVinylTrackStartMarkers().Begin(app);
        }
    }

}