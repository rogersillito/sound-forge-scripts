using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using developwithpassion.specifications.moq;
using developwithpassion.specifications.extensions;
using Moq;
using Should;
using SoundForge;
using SoundForgeScriptsLib.VinylRip;
using It = Machine.Specifications.It;

namespace SoundForgeScripts.Tests.ScriptsLib
{   
    public class SplitTrackListTests
    {
        public abstract class SplitTrackListContext : Observes<SplitTrackList>
        {
            protected static ISfFileHost _file;

            private Establish context = () =>
            {
                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(1100000);

                // we need to avoid the real concrete SfAudioMarkerList in factory, so stub it:
                var fileMock = new Mock<ISfFileHost>();
                fileMock.Setup(x => x.Markers).Returns(new Mock<SfAudioMarkerList>(MockBehavior.Default, fileMock.Object).Object);
                var markerAndRegionFactory = new TrackMarkerFactory(fileMock.Object);

                sut_factory.create_using(() => new SplitTrackList(_file, markerAndRegionFactory, markerAndRegionFactory, new TrackMarkerSpecifications()));
            };

            protected static List<SfAudioMarker> ExistingMarkers;
        }

        [Subject(typeof(SplitTrackList))]
        public class when_initializing_list_of_tracks : SplitTrackListContext
        {
            Establish context = () =>
            {
                ExistingMarkers = new List<SfAudioMarker>
                {
                    //TODO: may be some DODGY TEST CASES HERE!! check all are relevant after I copied/pasted this whole test class
                    //TODO: add fade in/end markers for each track
                    //TODO: ensure we have coverage for overlapping fades!
                    new SfAudioMarker(100, 500000) { Name = $"0001{TrackMarkerFactory.TrackRegionSuffix}" },
                    new SfAudioMarker(500500), // ignore - unnamed marker
                    new SfAudioMarker(600000, 100000) { Name = $"0002{TrackMarkerFactory.TrackRegionSuffix}" }, // too close to next for full fade!
                    new SfAudioMarker(700500) { Name = "BOB" }, // ignore - named marker not fade-related
                    new SfAudioMarker(701000, 100000) { Name = $"0003{TrackMarkerFactory.TrackRegionSuffix}" },
                    new SfAudioMarker(900000, 100000) { Name = $"NOT_A_TRACK!" }, // name not expected format
                };

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray()));
            };

            Because of = () => { _tracks =  sut.InitTracks(30, 7000); };

            It should_ignore_non_track_regions = () =>
                _tracks.Count.ShouldEqual(3);

            It should_set_fade_in_length_to_each_track = () =>
                _tracks.All(t => t.FadeInLength == 30).ShouldBeTrue();

            It should_create_missing_fade_in_end_marker = () =>
            {
                _tracks[1].FadeInEndMarker.Start.ShouldEqual(600030);
                _tracks[1].FadeInEndMarker.Name.ShouldEqual($"0002{TrackMarkerFactory.TrackFadeInEndSuffix}");
                _tracks[2].FadeInEndMarker.Start.ShouldEqual(701030);
                _tracks[2].FadeInEndMarker.Name.ShouldEqual($"0003{TrackMarkerFactory.TrackFadeInEndSuffix}");
                _tracks.All(t => t.FadeInEndMarker.HasLength == false).ShouldBeTrue();
            };

            It should_create_missing_fade_out_end_marker = () =>
            {
                _tracks[0].FadeOutEndMarker.Start.ShouldEqual(507100);
                _tracks[0].FadeOutEndMarker.Name.ShouldEqual($"0001{TrackMarkerFactory.TrackFadeOutEndSuffix}");
                _tracks[1].FadeOutEndMarker.Start.ShouldEqual(707000);
                _tracks[1].FadeOutEndMarker.Name.ShouldEqual($"0002{TrackMarkerFactory.TrackFadeOutEndSuffix}");
                _tracks[2].FadeOutEndMarker.Start.ShouldEqual(808000);
                _tracks[2].FadeOutEndMarker.Name.ShouldEqual($"0003{TrackMarkerFactory.TrackFadeOutEndSuffix}");
                _tracks.All(t => t.FadeOutEndMarker.HasLength == false).ShouldBeTrue();;
            };

            It should_set_start_to_be_same_as_original_marker = () =>
                _tracks.All(t => t.GetSelectionWithFades().Start == t.TrackRegion.Start).ShouldBeTrue();

            It should_return_true_when_checking_add_fade_in_outs = () =>
            {
                _tracks.All(t => t.CanAddFadeIn).ShouldBeTrue();
                var fadeOutFailMsg = _tracks.Where(t => !t.CanAddFadeOut).Select(t => t.Number).Aggregate("!CanAddFadeOut", (s, n) => $"{s},{n}");
                _tracks.All(t => t.CanAddFadeOut).ShouldBeTrue(fadeOutFailMsg);
            };

            It should_set_requested_fadeout_when_possible = () =>
            {
                _tracks[0].GetSelectionWithFades().Length.ShouldEqual(507000);
                _tracks[2].GetSelectionWithFades().Length.ShouldEqual(107000);
            };

            It should_set_fadeout_to_end_on_next_track_start_when_gap_too_short_for_requested_fade = () =>
            {
                _tracks[1].GetSelectionWithFades().Length.ShouldEqual(101000);
            };

            It should_set_incremental_track_numbers = () =>
            {
                _tracks[0].Number.ShouldEqual(1);
                _tracks[1].Number.ShouldEqual(2);
                _tracks[2].Number.ShouldEqual(3);
            };

            It should_set_found_region_on_each_track = () =>
            {
                _tracks[0].TrackRegion.ShouldEqual(ExistingMarkers[0]);
                _tracks[1].TrackRegion.ShouldEqual(ExistingMarkers[4]);
                _tracks[2].TrackRegion.ShouldEqual(ExistingMarkers[6]);
            };

            It should_set_found_fade_in_end_markers = () =>
                _tracks[0].FadeInEndMarker.ShouldEqual(ExistingMarkers[1]);

            It should_set_found_fade_out_end_markers = () =>
            {
                _tracks[0].FadeOutEndMarker.ShouldEqual(ExistingMarkers[2]);
                _tracks[2].FadeOutEndMarker.ShouldEqual(ExistingMarkers[8]);
            };

            private static SplitTrackList _tracks;
        }

        [Subject(typeof(SplitTrackList))]
        public class when_initializing_list_of_tracks_existing_fade_in_out_markers : SplitTrackListContext
        {
            Establish context = () =>
            {
                ExistingMarkers = new List<SfAudioMarker>
                {
                    //TODO: may be some DODGY TEST CASES HERE!! check all are relevant after I copied/pasted this whole test class
                    //TODO: add fade in/end markers for each track
                    //TODO: ensure we have coverage for overlapping fades!
                    new SfAudioMarker(100, 500000) { Name = $"0001{TrackMarkerFactory.TrackRegionSuffix}" },
                    new SfAudioMarker(105) { Name = $"0001{TrackMarkerFactory.TrackFadeInEndSuffix}"},
                    new SfAudioMarker(500333) { Name = $"0001{TrackMarkerFactory.TrackFadeOutEndSuffix}"},
                    new SfAudioMarker(500500), // ignore - unnamed marker
                    new SfAudioMarker(600000, 100000) { Name = $"0002{TrackMarkerFactory.TrackRegionSuffix}" }, // too close to next for full fade!
                    new SfAudioMarker(700500) { Name = "BOB" }, // ignore - named marker not fade-related
                    new SfAudioMarker(701000, 100000) { Name = $"0003{TrackMarkerFactory.TrackRegionSuffix}" },
                    new SfAudioMarker(900000, 100000) { Name = $"NOT_A_TRACK!" }, // name not expected format
                    new SfAudioMarker(100123) { Name = $"0003{TrackMarkerFactory.TrackFadeOutEndSuffix}"},
                };

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray()));
            };

            Because of = () => { _tracks =  sut.InitTracks(30, 7000); };

            It should_ignore_non_track_regions = () =>
                _tracks.Count.ShouldEqual(3);

            It should_set_fade_in_length_to_each_track = () =>
                _tracks.All(t => t.FadeInLength == 30).ShouldBeTrue();

            It should_create_missing_fade_in_end_marker = () =>
            {
                _tracks[1].FadeInEndMarker.Start.ShouldEqual(600030);
                _tracks[1].FadeInEndMarker.Name.ShouldEqual($"0002{TrackMarkerFactory.TrackFadeInEndSuffix}");
                _tracks[2].FadeInEndMarker.Start.ShouldEqual(701030);
                _tracks[2].FadeInEndMarker.Name.ShouldEqual($"0003{TrackMarkerFactory.TrackFadeInEndSuffix}");
                _tracks.All(t => t.FadeInEndMarker.HasLength == false).ShouldBeTrue();
            };

            It should_create_missing_fade_out_end_marker = () =>
            {
                //_tracks[0].FadeOutEndMarker.Start.ShouldEqual(507100);
                //_tracks[0].FadeOutEndMarker.Name.ShouldEqual($"0001{TrackMarkerFactory.TrackFadeOutEndSuffix}");
                _tracks[1].FadeOutEndMarker.Start.ShouldEqual(707000);
                _tracks[1].FadeOutEndMarker.Name.ShouldEqual($"0002{TrackMarkerFactory.TrackFadeOutEndSuffix}");
                //_tracks[2].FadeOutEndMarker.Start.ShouldEqual(808000);
                //_tracks[2].FadeOutEndMarker.Name.ShouldEqual($"0003{TrackMarkerFactory.TrackFadeOutEndSuffix}");
                _tracks.All(t => t.FadeOutEndMarker.HasLength == false).ShouldBeTrue();;
            };

            It should_set_start_to_be_same_as_original_marker = () =>
                _tracks.All(t => t.GetSelectionWithFades().Start == t.TrackRegion.Start).ShouldBeTrue();

            It should_return_true_when_checking_add_fade_in_outs = () =>
            {
                _tracks.All(t => t.CanAddFadeIn).ShouldBeTrue();
                var fadeOutFailMsg = _tracks.Where(t => !t.CanAddFadeOut).Select(t => t.Number).Aggregate("!CanAddFadeOut", (s, n) => $"{s},{n}");
                _tracks.All(t => t.CanAddFadeOut).ShouldBeTrue(fadeOutFailMsg);
            };

            It should_set_requested_fadeout_when_possible = () =>
            {
                _tracks[0].GetSelectionWithFades().Length.ShouldEqual(507000);
                _tracks[2].GetSelectionWithFades().Length.ShouldEqual(107000);
            };

            It should_set_fadeout_to_end_on_next_track_start_when_gap_too_short_for_requested_fade = () =>
            {
                _tracks[1].GetSelectionWithFades().Length.ShouldEqual(101000);
            };

            It should_set_incremental_track_numbers = () =>
            {
                _tracks[0].Number.ShouldEqual(1);
                _tracks[1].Number.ShouldEqual(2);
                _tracks[2].Number.ShouldEqual(3);
            };

            It should_set_found_region_on_each_track = () =>
            {
                _tracks[0].TrackRegion.ShouldEqual(ExistingMarkers[0]);
                _tracks[1].TrackRegion.ShouldEqual(ExistingMarkers[4]);
                _tracks[2].TrackRegion.ShouldEqual(ExistingMarkers[6]);
            };

            It should_set_found_fade_in_end_markers = () =>
                _tracks[0].FadeInEndMarker.ShouldEqual(ExistingMarkers[1]);

            It should_set_found_fade_out_end_markers = () =>
            {
                _tracks[0].FadeOutEndMarker.ShouldEqual(ExistingMarkers[2]);
                _tracks[2].FadeOutEndMarker.ShouldEqual(ExistingMarkers[8]);
            };

            private static SplitTrackList _tracks;
        }

        [Subject(typeof(SplitTrackList))]
        public class when_getting_list_of_tracks_with_no_fades : SplitTrackListContext
        {
            Establish context = () =>
            {
                ExistingMarkers = new List<SfAudioMarker>
                {
                    new SfAudioMarker(100, 500000) { Name = $"0001{TrackMarkerFactory.TrackRegionSuffix}" },
                    new SfAudioMarker(100) { Name = $"0001{TrackMarkerFactory.TrackFadeInEndSuffix}"},
                    new SfAudioMarker(500100) { Name = $"0001{TrackMarkerFactory.TrackFadeOutEndSuffix}"},
                    new SfAudioMarker(600000, 100000) { Name = $"0002{TrackMarkerFactory.TrackRegionSuffix}" }, // too close to next for full fade!
                    new SfAudioMarker(701000, 100000) { Name = $"0003{TrackMarkerFactory.TrackRegionSuffix}" },
                };

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray()));
            };

            Because of = () => { _tracks =  sut.InitTracks(0,0); };

            It should_set_no_fade_in_length_to_each_track = () =>
                _tracks.All(t => t.FadeInLength == 0).ShouldBeTrue();

            It should_set_length_to_be_same_as_original_marker = () =>
                _tracks.All(t => t.GetSelectionWithFades().Length == t.TrackRegion.Length).ShouldBeTrue();

            It should_return_false_when_checking_add_fade_in_outs = () =>
            {
                _tracks.All(t => t.CanAddFadeIn).ShouldBeFalse();
                _tracks.All(t => t.CanAddFadeOut).ShouldBeFalse();
            };

            private static SplitTrackList _tracks;
        }
    }
}
