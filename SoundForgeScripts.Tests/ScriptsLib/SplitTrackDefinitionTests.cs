using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using developwithpassion.specifications.moq;
using developwithpassion.specifications.extensions;
using Moq;
using Should;
using SoundForge;
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;
using It = Machine.Specifications.It;

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
                    new SfAudioMarker(0, 10000) { Name = $"0001{TrackMarkerFactory.TrackRegionSuffix}" },
                    new SfAudioMarker(10300, 20000) { Name = $"0002{TrackMarkerFactory.TrackRegionSuffix}" }
                };

                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(30500);

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray())
                );

                // we need to avoid the real concrete SfAudioMarkerList in factory, so stub it:
                var fileMock = new Mock<ISfFileHost>();
                fileMock.Setup(x => x.Markers).Returns(new Mock<SfAudioMarkerList>(MockBehavior.Default, fileMock.Object).Object);
                var markerAndRegionFactory = new TrackMarkerFactory(fileMock.Object);

                SplitTrackList = new SplitTrackList(_file, markerAndRegionFactory, markerAndRegionFactory, new TrackMarkerSpecifications());
                SplitTrackList.InitTracks(10, 100);
            };

            protected static List<SfAudioMarker> ExistingMarkers;
            protected internal static SplitTrackList SplitTrackList;
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_in_length : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => 
                sut.FadeInLength = 150;
            
            private It should_return_value_set = () =>
                sut.FadeInLength.ShouldEqual(150);

            private It should_move_fade_in_end_marker = () =>
                sut.FadeInEndMarker.Start.ShouldEqual(150);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_in_length_to_exceed_track_region : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => 
                sut.FadeInLength = 15000;
            
            private It should_return_value_set = () =>
                sut.FadeInLength.ShouldEqual(10000);

            private It should_move_fade_in_end_marker_to_end_of_track_region = () =>
                sut.FadeInEndMarker.Start.ShouldEqual(10000);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_in_length_negative_value : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => 
                sut.FadeInLength = -1;
            
            private It should_return_value_set = () =>
                sut.FadeInLength.ShouldEqual(0);

            private It should_move_fade_in_end_marker_to_start_of_track_region = () =>
                sut.FadeInEndMarker.Start.ShouldEqual(0);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_out_length : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => 
                sut.FadeOutLength = 200;
            
            private It should_update_selection = () =>
                sut.GetSelectionWithFades().Length.ShouldEqual(10200);

            private It should_not_update_fade_out_start = () =>
                sut.FadeOutStartPosition.ShouldEqual(10000);

            private It should_return_value_set = () =>
                sut.FadeOutLength.ShouldEqual(200);

            private It should_move_fade_out_end_marker = () =>
                sut.FadeOutEndMarker.Start.ShouldEqual(10200);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_out_length_to_exceed_file_length : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.Last());

            Because of = () => sut.FadeOutLength = 11000;

            private It should_update_selection_to_max_length_permitted_by_file_end = () =>
                sut.GetSelectionWithFades().Length.ShouldEqual(20200);

            private It should_not_update_fade_out_start = () =>
                sut.FadeOutStartPosition.ShouldEqual(30300);

            private It should_return_value_set_permitted_fade_length = () =>
                sut.FadeOutLength.ShouldEqual(200);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_out_length_to_overlap_next_track : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => sut.FadeOutLength = 400;

            private It should_update_selection_to_max_length_permitted_by_next_track = () =>
                sut.GetSelectionWithFades().Length.ShouldEqual(10300);

            private It should_not_change_update_fade_out_start = () =>
                sut.FadeOutStartPosition.ShouldEqual(10000);

            private It should_return_value_set_permitted_fade_length = () =>
                sut.FadeOutLength.ShouldEqual(300);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_out_length_negative_value : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => 
                sut.FadeOutLength = -1;
            
            private It should_return_value_set = () =>
                sut.FadeOutLength.ShouldEqual(0);

            private It should_move_fade_out_end_marker_to_end_of_track_region = () =>
                sut.FadeOutEndMarker.Start.ShouldEqual(10000);
        }

        //TODO: deleted marker (in/out end)
        /*
        [Subject(typeof(SplitTrackDefinition))]
        public class when_fade_in_end_marker_deleted : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => 
                _file.Markers.Remove(sut.FadeInEndMarker);
            
            private It should_recreate_fade_in_end_marker = () =>
                sut.FadeInEndMarker.ShouldNotBeNull();

            private It should_set_fade_in_end_at_start_of_track_region = () =>
            {
                sut.FadeInEndMarker.Start.ShouldEqual(sut.TrackRegion.Start);
                sut.FadeInLength.ShouldEqual(0);
            };
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_fade_out_end_marker_deleted : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () => 
                _file.Markers.Remove(sut.FadeOutEndMarker);
            
            private It should_recreate_fade_out_end_marker = () =>
                sut.FadeOutEndMarker.ShouldNotBeNull();

            private It should_set_fade_out_end_at_end_of_track_region = () =>
            {
                sut.FadeOutEndMarker.Start.ShouldEqual(MarkerHelper.GetMarkerEnd(sut.TrackRegion));
                sut.FadeOutLength.ShouldEqual(0);
            };
        }
        */

        //TODO: deleted region ??? code will throw Null ref...
    }
}
