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
using SoundForgeScriptsLib.EntryPoints;

namespace SoundForgeScripts.Scripts.VinylRip2FinalTrackProcessing
{
    [ScriptName("Vinyl Rip 2 - Adjust Tracks")]
    public class EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;

        private FindTracksOptions _findTracksOptions;
        private FileTasks _fileTasks;

        protected override void Execute()
        {
            _file = App.CurrentFile;
            ISfFileHost file = _file;

            _fileTasks = new FileTasks(file);
            _fileTasks.EnforceStereoFileOpen();
            _fileTasks.ZoomOutFull();

            //TODO: initial dialog to configure these:
            _findTracksOptions = new FindTracksOptions();
            _findTracksOptions.TrackAddFadeOutLengthInSeconds = 3;
            _findTracksOptions.TrackFadeInLengthInSamples = 20;
        }
    }
}
