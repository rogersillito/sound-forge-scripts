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

        protected override void Execute()
        {
            _file = App.CurrentFile;
            _fileTasks = new FileTasks(_file);
            _fileTasks.EnforceStereoFileOpen();
            _fileTasks.ZoomOutFull();

            FileMarkersWrapper markers = new FileMarkersWrapper(_file);
            TrackMarkerNameBuilder trackMarkerNameBuilder = new TrackMarkerNameBuilder();
            TrackMarkerFactory markerAndRegionFactory = new TrackMarkerFactory(markers, Output, trackMarkerNameBuilder);
            _splitTrackList = new SplitTrackList(markerAndRegionFactory, markerAndRegionFactory, trackMarkerNameBuilder, markers, new TrackMarkerSpecifications(), Output);

            _vinylRipOptions = new VinylRipOptions();

            // TODO: validate tracks

            _splitTrackList.InitTracks(_vinylRipOptions);
            _splitTrackList.DumpToScriptWindow();

            EditTracksViewModel viewModel = new EditTracksViewModel(_fileTasks);

            EditTracksController controller = new EditTracksController(App, new EditTracksFormFactory(), this, Output, _fileTasks);
            controller.Edit(viewModel, _splitTrackList, _vinylRipOptions);
        }
    }
}
