using System.Collections.Generic;
using System.Linq;
using developwithpassion.specifications.extensions;
using developwithpassion.specifications.moq;
using Machine.Specifications;
using Should;
using SoundForge;
using SoundForgeScripts.Tests.Helpers;
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;
using It = Machine.Specifications.It;

namespace SoundForgeScripts.Tests.ScriptsLib.VinylRip
{   
    public class SplitTrackDefinitionResizeTests
    {
        public abstract class SplitTrackDefinitionContext : Observes<SplitTrackDefinition>
        {
            protected static ISfFileHost _file;

            private Establish context = () =>
            {
                ExistingMarkers = new List<SfAudioMarker>
                {
                    new SfAudioMarker(0, 10000) { Name = $"0001{TrackMarkerNameBuilder.TrackRegionSuffix}" },
                    new SfAudioMarker(20) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" },
                    new SfAudioMarker(10200) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" },

                    new SfAudioMarker(10300, 20000) { Name = $"0002{TrackMarkerNameBuilder.TrackRegionSuffix}" },
                    new SfAudioMarker(10320) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" },
                    new SfAudioMarker(30500) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" }
                };

                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(30500);

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray())
                );

                var fileMarkersHelper = new FileMarkersTestHelper();
                var markerList = fileMarkersHelper.CreateStubMarkerList(_file);
                fileMarkersHelper.RealMarkerList.AddRange(ExistingMarkers);
                var markerAndRegionFactory = new TrackMarkerFactory(markerList.Object);

                SplitTrackList = new SplitTrackList(markerAndRegionFactory, markerAndRegionFactory, new TrackMarkerNameBuilder(), markerList.Object, new TrackMarkerSpecifications(), depends.@on<IOutputHelper>());
                SplitTrackList.InitTracks(10, 100);
            };

            protected static List<SfAudioMarker> ExistingMarkers;
            protected internal static SplitTrackList SplitTrackList;
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_checking_edits_on_first_track : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            // Fade in - CAN
            private It should_get_true_when_checking_set_zero_fade_in = () =>
                sut.CanMoveFadeInBy(-20).ShouldBeTrue();

            private It should_get_true_when_checking_extend_fade_in = () =>
                sut.CanMoveFadeInBy(20).ShouldBeTrue();

            private It should_get_true_when_checking_set_fade_in_to_track_end = () =>
                sut.CanMoveFadeInBy(9980).ShouldBeTrue();

            // Fade in - CANNOT!
            private It should_get_false_when_checking_set_fade_in_before_track_start = () =>
                sut.CanMoveFadeInBy(-21).ShouldBeFalse();

            private It should_get_false_when_checking_set_fade_in_after_track_end = () =>
                sut.CanMoveFadeInBy(9981).ShouldBeFalse();

            // Start - CAN
            private It should_get_true_when_checking_move_start_forward = () =>
                sut.CanMoveStartBy(200).ShouldBeTrue();

            private It should_get_true_when_checking_move_start_to_just_before_track_region_end = () =>
                sut.CanMoveStartBy(9999).ShouldBeTrue();

            // Start - CANNOT!
            private It should_get_false_when_checking_set_start_before_file_start = () =>
                sut.CanMoveStartBy(-1).ShouldBeFalse();

            private It should_get_false_when_checking_set_start_equal_to_track_region_end = () =>
                sut.CanMoveStartBy(10000).ShouldBeFalse();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_checking_edits_on_second_track : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.Last());

            // Start - CAN
            private It should_get_true_when_checking_move_start_to_end_of_previous_track_fade_out = () =>
                sut.CanMoveStartBy(-100).ShouldBeTrue();

            // Start - CANNOT!
            private It should_get_false_when_checking_move_start_before_end_of_previous_track_fade_out = () =>
                sut.CanMoveStartBy(-101).ShouldBeFalse();

        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_fade_in : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () =>
                sut.MoveFadeInBy(-19);

            It should_update_fade_in_marker = () => sut.FadeInEndMarker.Start.ShouldEqual(1);
        }
    }
}
