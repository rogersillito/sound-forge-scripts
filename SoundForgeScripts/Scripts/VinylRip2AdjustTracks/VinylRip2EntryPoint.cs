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

using System.Threading;
using SoundForge;
using SoundForgeScriptsLib;
using SoundForgeScriptsLib.EntryPoints;
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Scripts.VinylRip2AdjustTracks
{
    public delegate void DeleteMarker(int marker);

    [ScriptName("Vinyl Rip 2 - Adjust Tracks")]
    public class EntryPoint : EntryPointBase
    {
        private ISfFileHost _file;

        private VinylRipOptions _vinylRipOptions;
        private FileTasks _fileTasks;
        private SplitTrackList _splitTrackList;
        private WindowTasks _windowTasks;

        protected override void Execute()
        {
            _file = App.CurrentFile;
            _fileTasks = new FileTasks(_file);
            _windowTasks = new WindowTasks();
            _fileTasks.EnforceStereoFileOpen();
            _fileTasks.ZoomOutFull();

            FileMarkersWrapper markers = new FileMarkersWrapper(_file);
            TrackMarkerNameBuilder trackMarkerNameBuilder = new TrackMarkerNameBuilder();
            TrackMarkerFactory markerAndRegionFactory = new TrackMarkerFactory(markers, Output, trackMarkerNameBuilder);
            _splitTrackList = new SplitTrackList(markerAndRegionFactory, markerAndRegionFactory, trackMarkerNameBuilder, markers, new TrackMarkerSpecifications(), Output);

            //TODO: initial dialog to configure these:
            _vinylRipOptions = new VinylRipOptions();
            _vinylRipOptions.DefaultTrackFadeOutLengthInSeconds = 3;
            _vinylRipOptions.DefaultTrackFadeInLengthInSamples = 20;
            _vinylRipOptions.MinimumTrackLengthInSeconds = 10;

            GetSplitTrackDefinitions(_splitTrackList);

            EditTracksViewModel viewModel = new EditTracksViewModel(_fileTasks);

            EditTracksController controller = new EditTracksController(App, new EditTracksFormFactory(), this, Output, _fileTasks);
            controller.Edit(viewModel, _splitTrackList, _vinylRipOptions);
        }

        private void GetSplitTrackDefinitions(SplitTrackList tracks)
        {
            tracks.InitTracks(_vinylRipOptions);
            Output.ToScriptWindow("Found {0} tracks:", tracks.Count);
            foreach (SplitTrackDefinition track in tracks)
            {
                Output.ToScriptWindow("{0}:\t{1}\t{2}\t(Start fade @ {3})", track.Number,
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.TrackRegion.Start)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.GetSelectionWithFades().Length)),
                    OutputHelper.FormatToTimeSpan(_file.PositionToSeconds(track.FadeOutStartPosition)));
            }

            Output.LineBreakToScriptWindow();
        }
    }
}
