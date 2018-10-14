/* =======================================================================================================
 *	Script Name: Vinyl Rip 2 - Adjust Tracks
*	Description: Allows editing of track regions found in "Vinyl Rip 1 - Find Tracks"
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
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    [ScriptName("Vinyl Rip 2 - Adjust Tracks")]
    public class EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;

        private FindTracksOptions _findTracksOptions;
        private FileTasks _fileTasks;
        private SplitTrackList _splitTrackList;

        protected override void Execute()
        {
            _file = App.CurrentFile;
            ISfFileHost file = _file;

            _fileTasks = new FileTasks(file);
            _fileTasks.EnforceStereoFileOpen();
            _fileTasks.ZoomOutFull();

            _splitTrackList = new SplitTrackList(_file);

            //TODO: initial dialog to configure these:
            _findTracksOptions = new FindTracksOptions();
            _findTracksOptions.TrackAddFadeOutLengthInSeconds = 3;
            _findTracksOptions.TrackFadeInLengthInSamples = 20;

            GetSplitTrackDefinitions();
        }

        private void GetSplitTrackDefinitions()
        {
            long fadeOutLengthSamples = _file.SecondsToPosition(_findTracksOptions.TrackAddFadeOutLengthInSeconds);
            SplitTrackList tracks = _splitTrackList.InitTracks(_findTracksOptions.TrackFadeInLengthInSamples, fadeOutLengthSamples);
            Output.ToScriptWindow("Found {0} tracks:", tracks.Count);
            foreach (SplitTrackDefinition track in tracks)
            {
                Output.ToScriptWindow("{0}:\t{1}\t{2}\t(Start fade @ {3})", track.Number,
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.Selection.Start)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.Selection.Length)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.FadeOutStartPosition)));
                
            }
            Output.LineBreakToScriptWindow();
        }
    }
}
