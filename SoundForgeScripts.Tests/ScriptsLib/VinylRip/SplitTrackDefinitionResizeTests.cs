using System.Collections.Generic;
using System.Linq;
using developwithpassion.specifications.extensions;
using developwithpassion.specifications.moq;
using Machine.Fakes;
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
                    new SfAudioMarker(20) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" }, // fade in == 20
                    new SfAudioMarker(10200) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" }, // fade out == 200

                    new SfAudioMarker(10300, 20000) { Name = $"0002{TrackMarkerNameBuilder.TrackRegionSuffix}" },
                    new SfAudioMarker(10700) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" }, // fade in == 400
                    new SfAudioMarker(30500) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" } // fade out == 200
                };

                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(30600);

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray())
                );

                var fileMarkersHelper = new FileMarkersTestHelper();
                var markerList = fileMarkersHelper.CreateStubMarkerList(_file);
                fileMarkersHelper.RealMarkerList.AddRange(ExistingMarkers);
                var markerAndRegionFactory = new TrackMarkerFactory(markerList.Object);

                SplitTrackList = new SplitTrackList(markerAndRegionFactory, markerAndRegionFactory, new TrackMarkerNameBuilder(), markerList.Object, new TrackMarkerSpecifications(), depends.@on<IOutputHelper>());
                _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(100);
                SplitTrackList.InitTracks(new VinylRipOptions
                {
                    DefaultTrackFadeInLengthInSamples = 10,
                    DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup
                });
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

            // Fade out - CAN
            private It should_get_true_when_checking_set_zero_fade_out = () =>
                sut.CanMoveFadeOutBy(-200).ShouldBeTrue();

            private It should_get_true_when_checking_extend_fade_out = () =>
                sut.CanMoveFadeOutBy(20).ShouldBeTrue();

            private It should_get_true_when_checking_set_fade_out_to_start_of_next_track = () =>
                sut.CanMoveFadeOutBy(100).ShouldBeTrue();

            // Fade out - CANNOT!
            private It should_get_false_when_checking_set_fade_out_before_track_region_end = () =>
                sut.CanMoveFadeOutBy(-201).ShouldBeFalse();

            private It should_get_false_when_checking_set_fade_out_after_start_of_next_track = () =>
                sut.CanMoveFadeOutBy(9981).ShouldBeFalse();

            // End - CAN
            private It should_get_true_when_checking_move_end_forward = () =>
                sut.CanMoveEndBy(100).ShouldBeTrue();

            private It should_get_true_when_checking_move_end_to_start_of_next_track = () =>
                sut.CanMoveEndBy(300).ShouldBeTrue();

            private It should_get_true_when_checking_move_end_to_just_after_track_start = () =>
                sut.CanMoveEndBy(-9999).ShouldBeTrue();

            // End - CANNOT!
            private It should_get_false_when_checking_set_end_after_start_of_next_track = () =>
                sut.CanMoveEndBy(301).ShouldBeFalse();

            private It should_get_false_when_checking_set_end_equal_to_track_region_start = () =>
                sut.CanMoveEndBy(-10000).ShouldBeFalse();
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

            // Fade out - CAN
            private It should_get_true_when_checking_set_fade_out_to_file_end = () =>
                sut.CanMoveFadeOutBy(100).ShouldBeTrue();

            // Fade out - CANNOT!
            private It should_get_false_when_checking_set_fade_out_after_file_end = () =>
                sut.CanMoveFadeOutBy(101).ShouldBeFalse();

            // End - CAN
            private It should_get_true_when_checking_move_end_to_file_end = () =>
                sut.CanMoveEndBy(300).ShouldBeTrue();

            // End - CANNOT!
            private It should_get_false_when_checkin_move_end_after_file_end = () =>
                sut.CanMoveEndBy(301).ShouldBeFalse();

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

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_start_forwards : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () =>
                sut.MoveStartBy(20);

            It should_update_start_marker = () => sut.TrackRegion.Start.ShouldEqual(20);

            It should_update_fade_in_marker_by_same = () => sut.FadeInEndMarker.Start.ShouldEqual(40);

            It should_not_update_end_marker = () => MarkerHelper.GetMarkerEnd(sut.TrackRegion).ShouldEqual(10000);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_start_backwards : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.Last());

            Because of = () =>
                sut.MoveStartBy(-100);

            It should_update_start_marker = () => sut.TrackRegion.Start.ShouldEqual(10200);

            It should_update_fade_in_marker_by_same = () => sut.FadeInEndMarker.Start.ShouldEqual(10600);

            It should_not_update_end_marker = () => MarkerHelper.GetMarkerEnd(sut.TrackRegion).ShouldEqual(30300);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_start_and_fade_in_cannot_be_moved_by_requested_amount : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.Last());

            Because of = () =>
                sut.MoveStartBy(19990);

            It should_update_start_marker = () => sut.TrackRegion.Start.ShouldEqual(30290);

            It should_update_fade_in_marker_by_maximum_possible_value = () => sut.FadeInEndMarker.Start.ShouldEqual(30300);

            It should_not_update_end_marker = () => MarkerHelper.GetMarkerEnd(sut.TrackRegion).ShouldEqual(30300);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_fade_out : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () =>
                sut.MoveFadeOutBy(-50);

            It should_update_fade_out_marker = () => sut.FadeOutEndMarker.Start.ShouldEqual(10150);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_end : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () =>
                sut.MoveEndBy(75);

            It should_update_end_marker = () => MarkerHelper.GetMarkerEnd(sut.TrackRegion).ShouldEqual(10075);

            It should_update_fade_out_marker_by_same_amount = () => sut.FadeOutEndMarker.Start.ShouldEqual(10275);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_end_and_fade_out_cannot_be_moved_by_requested_amount : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.First());

            Because of = () =>
                sut.MoveEndBy(297);

            It should_update_end_marker = () => MarkerHelper.GetMarkerEnd(sut.TrackRegion).ShouldEqual(10297);

            It should_update_fade_out_marker_by_maximum_possible_value = () => sut.FadeOutEndMarker.Start.ShouldEqual(10300);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_end_of_last_track_and_fade_out_cannot_be_moved_by_requested_amount : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.Last());

            Because of = () =>
                sut.MoveEndBy(297);

            It should_update_end_marker = () => MarkerHelper.GetMarkerEnd(sut.TrackRegion).ShouldEqual(30597);

            It should_update_fade_out_marker_by_maximum_possible_value = () => sut.FadeOutEndMarker.Start.ShouldEqual(30600);
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_moving_end_near_track_start : SplitTrackDefinitionContext
        {
            Establish context = () =>
                sut_factory.create_using(() => SplitTrackList.Last());

            Because of = () =>
                sut.MoveEndBy(-19900);

            It should_update_end_marker = () => MarkerHelper.GetMarkerEnd(sut.TrackRegion).ShouldEqual(10400);

            It should_update_fade_out_marker_by_same_amount = () => sut.FadeOutEndMarker.Start.ShouldEqual(10600);

            It should_update_fade_in_marker_to_avoid_overlap = () => sut.FadeInEndMarker.Start.ShouldEqual(10400);
        }
    }
}
