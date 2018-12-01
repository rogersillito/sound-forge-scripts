using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using developwithpassion.specifications.moq;
using developwithpassion.specifications.extensions;
using developwithpassion.specifications.faking;
using Should;
using SoundForge;
using SoundForgeScripts.Scripts.VinylRip3FinalTrackProcessing;
using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Tests.ScriptsLib
{   
    public class SplitTrackDefinitionTests
    {
        public abstract class SplitTrackDefinitionContext : Observes<SplitTrackDefinition>
        {
            protected static ISfFileHost _file;

            private Establish context = () =>
            {
                ExistingMarkers = new List<SfAudioMarker>
                {
                    new SfAudioMarker(0, 10000) { Name = $"{SplitTrackList.TrackRegionPrefix}0001" },
                    new SfAudioMarker(10300, 20000) { Name = $"{SplitTrackList.TrackRegionPrefix}0002" }
                };

                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(20000);

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray())
                );

                var splitTrackList = new SplitTrackList(_file);
                splitTrackList.InitTracks(10, 100);

                sut_factory.create_using(() => splitTrackList.First());
            };

            protected static List<SfAudioMarker> ExistingMarkers;
        }
        // TODO: when marker deleted..

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_out_length : SplitTrackDefinitionContext
        {
            Because of = () => sut.FadeOutLength = 200;

            private It should_update_selection = () =>
                sut.Selection.Length.ShouldEqual(10200);

            private It should_not_change_update_fade_out_start = () =>
                sut.FadeOutStartPosition.ShouldEqual(10000);

            private It should_return_value_set = () =>
                sut.FadeOutLength.ShouldEqual(200);

            private It should_move_fade_out_end_marker = () =>
                sut.FadeOutEndMarker.Start.ShouldEqual(10200);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_out_length_to_exceed_file_length : SplitTrackDefinitionContext
        {
            Because of = () => sut.FadeOutLength = 400;

            private It should_update_selection_to_max_length_permitted_by_next_track = () =>
                sut.Selection.Length.ShouldEqual(10300);

            private It should_not_change_update_fade_out_start = () =>
                sut.FadeOutStartPosition.ShouldEqual(10000);

            private It should_return_value_set_permitted_fade_length = () =>
                sut.FadeOutLength.ShouldEqual(300);
        }
    }
}
